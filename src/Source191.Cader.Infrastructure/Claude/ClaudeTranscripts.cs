using Microsoft.Win32.SafeHandles;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Buffers;
using System.Collections.Concurrent;
using System.Text.Json;
namespace Source191.Cader.Adapters.Claude;

/// <summary>
/// Looks for <c>~\.claude\projects\&lt;project&gt;\&lt;sessionId&gt;.jsonl</c>. The project folder name encodes the
/// working directory, so every project folder is checked rather than reproducing that encoding. A transcript has
/// messages once it holds a user or assistant line: after <c>/bg</c> Claude writes a new session's transcript with only
/// its title and name (2.1.325), which <c>--resume</c> cannot find. Lines are read as UTF-8 bytes and never decoded whole:
/// of each line only the top-level type is read, and of the first <c>session-owner</c> line (2.1.325 on) its
/// <c>organizationUuid</c>.
/// </summary>
public sealed class ClaudeTranscripts(string claudeHome) : ITranscripts
{
    // An owner never changes, so one found is kept for good; a transcript without one is read again only once it has
    // changed, so a long one from an older Claude Code is not scanned on every refresh. Refreshes can overlap.
    private readonly ConcurrentDictionary<SessionId, OwnerRead> owners = new();

    public bool HasMessages(SessionId sessionId) => TranscriptsOf(sessionId).Any(HoldsAMessage);

    public SessionOwner OwnerOf(SessionId sessionId)
    {
        bool known = owners.TryGetValue(sessionId, out OwnerRead? kept);
        if (known && kept is { Owner: SessionOwner.Recorded })
        {
            return kept.Owner;
        }

        if (TranscriptsOf(sessionId).FirstOrDefault() is not string transcript)
        {
            return new SessionOwner.NotRecorded();
        }

        try
        {
            FileInfo file = new(transcript);
            Stamp stamp = new(file.Length, file.LastWriteTimeUtc);
            if (known && kept is not null && kept.Stamp == stamp)
            {
                return kept.Owner;
            }

            SessionOwner owner = ReadOwner(transcript);
            owners[sessionId] = new OwnerRead(owner, stamp);

            return owner;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // Not kept, so the next refresh tries again once Claude lets the file go.
            return new SessionOwner.NotRecorded();
        }
    }

    private IEnumerable<string> TranscriptsOf(SessionId sessionId)
    {
        string projects = Path.Combine(claudeHome, "projects");
        if (!Directory.Exists(projects))
        {
            return [];
        }

        return Directory.EnumerateDirectories(projects)
            .Select(project => Path.Combine(project, $"{sessionId.Value}.jsonl"))
            .Where(File.Exists);
    }

    /// <summary>A transcript that exists but cannot be read keeps the answer its existence gave before lines were read.</summary>
    private static bool HoldsAMessage(string transcript)
    {
        try
        {
            using TranscriptLines lines = new(transcript);
            while (lines.TryNext(out ReadOnlySpan<byte> line))
            {
                if (IsMessage(line))
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return true;
        }
    }

    private static SessionOwner ReadOwner(string transcript)
    {
        using TranscriptLines lines = new(transcript);
        while (lines.TryNext(out ReadOnlySpan<byte> line))
        {
            // Only a line naming the type is read: message lines can be large, and most are messages.
            if (line.IndexOf("\"session-owner\""u8) >= 0 && TryReadString(line, "type"u8, out Utf8JsonReader type) && type.ValueTextEquals("session-owner"u8))
            {
                return OwnerOn(line);
            }
        }

        return new SessionOwner.NotRecorded();
    }

    private static SessionOwner OwnerOn(ReadOnlySpan<byte> line) => TryReadString(line, "organizationUuid"u8, out Utf8JsonReader value) && OrganisationId.TryParse(value.GetString(), out OrganisationId? organisation)
        ? new SessionOwner.Recorded(organisation)
        : new SessionOwner.NotRecorded();

    private static bool IsMessage(ReadOnlySpan<byte> line) => TryReadString(line, "type"u8, out Utf8JsonReader type) && (type.ValueTextEquals("user"u8) || type.ValueTextEquals("assistant"u8));

    /// <summary>
    /// Reads the line as one whole JSON object, with the default options <see cref="JsonDocument.Parse(string, JsonDocumentOptions)"/>
    /// uses, and answers a reader at the value of the last top-level property of that name, as
    /// <see cref="JsonElement.TryGetProperty(string, out JsonElement)"/> finds it, when that value is a string. Every other
    /// value is skipped unread, and a line that is not one whole object, a line still being written included, answers false.
    /// </summary>
    private static bool TryReadString(ReadOnlySpan<byte> line, ReadOnlySpan<byte> name, out Utf8JsonReader value)
    {
        value = default;
        bool found = false;
        Utf8JsonReader reader = new(line);
        try
        {
            if (!reader.Read() || reader.TokenType is not JsonTokenType.StartObject)
            {
                return false;
            }

            while (reader.Read() && reader.TokenType is JsonTokenType.PropertyName)
            {
                bool named = reader.ValueTextEquals(name);
                reader.Read();
                found = named ? reader.TokenType is JsonTokenType.String : found;
                value = named ? reader : value;
                reader.Skip();
            }

            return !reader.Read() && found;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private sealed record Stamp(long Length, DateTime WrittenUtc);

    /// <summary>
    /// A transcript's lines as UTF-8 bytes, read through <see cref="RandomAccess"/> into a buffer from the shared pool that
    /// doubles while a line does not fit, so no line is decoded to read it. A line ends at a line feed (a carriage return
    /// before it is JSON whitespace) or at the end of the file, and a byte order mark before the first line is skipped as
    /// <see cref="StreamReader"/> skips it. The file is shared for reading, writing and deleting, as Claude may be writing it.
    /// </summary>
    private sealed class TranscriptLines(string transcript) : IDisposable
    {
        private readonly SafeFileHandle handle = File.OpenHandle(transcript, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        private byte[] buffer = ArrayPool<byte>.Shared.Rent(16 * 1024);
        private long offset;
        private int start;
        private int end;
        private bool atEnd;

        private static ReadOnlySpan<byte> ByteOrderMark => [0xEF, 0xBB, 0xBF];

        public bool TryNext(out ReadOnlySpan<byte> line)
        {
            int feed = buffer.AsSpan(start, end - start).IndexOf((byte)'\n');
            while (feed < 0 && !atEnd)
            {
                Fill();
                feed = buffer.AsSpan(start, end - start).IndexOf((byte)'\n');
            }

            int length = feed < 0 ? end - start : feed;
            bool any = feed >= 0 || length > 0;
            line = buffer.AsSpan(start, length);
            start = feed < 0 ? end : start + feed + 1;

            return any;
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(buffer);
            handle.Dispose();
        }

        /// <summary>
        /// Moves the unread bytes to the front, doubles the buffer when they fill it, and reads on from the file, stepping
        /// over a byte order mark at its start.
        /// </summary>
        private void Fill()
        {
            buffer.AsSpan(start, end - start).CopyTo(buffer);
            end -= start;
            start = 0;
            if (end == buffer.Length)
            {
                byte[] larger = ArrayPool<byte>.Shared.Rent(buffer.Length * 2);
                buffer.AsSpan(0, end).CopyTo(larger);
                ArrayPool<byte>.Shared.Return(buffer);
                buffer = larger;
            }

            bool first = offset == 0;
            int read = RandomAccess.Read(handle, buffer.AsSpan(end), offset);
            offset += read;
            end += read;
            atEnd = read == 0;
            start = first && buffer.AsSpan(0, end).StartsWith(ByteOrderMark) ? ByteOrderMark.Length : start;
        }
    }

    private sealed record OwnerRead(SessionOwner Owner, Stamp Stamp);
}
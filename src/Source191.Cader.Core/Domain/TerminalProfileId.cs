using System.Security.Cryptography;
using System.Text;

namespace Source191.Cader.Domain;

/// <summary>
/// A Windows Terminal profile's <c>guid</c>. Terminal gives a fragment's profile a version 5 UUID of its name under a
/// namespace that is the version 5 UUID of the fragment's folder name (its source) under Terminal's runtime namespace,
/// both names hashed as UTF-16LE; see <c>.claude\rules\windows.md</c>.
/// </summary>
public sealed record TerminalProfileId(Guid Value)
{
    private static readonly Guid RuntimeNamespace = new("f65ddb7e-706b-4499-8a50-40313caf510a");

    /// <summary>Terminal's spelling: lower case, in braces.</summary>
    public string Text => Value.ToString("B");

    public static TerminalProfileId ForFragment(string source, string name) => new(Version5(Version5(RuntimeNamespace, source), name));

    /// <summary>RFC 9562, section 5.5: the namespace's bytes in network order, then the name's, hashed.</summary>
    private static Guid Version5(Guid space, string name)
    {
        byte[] input = [.. space.ToByteArray(bigEndian: true), .. Encoding.Unicode.GetBytes(name)];
        // RFC 9562 defines version 5 over SHA-1; here it names a profile and secures nothing.
#pragma warning disable CA5350, S4790
        byte[] hash = SHA1.HashData(input);
#pragma warning restore CA5350, S4790
        hash[6] = (byte)((hash[6] & 0x0F) | 0x50);
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

        return new Guid(hash.AsSpan(0, 16), bigEndian: true);
    }
}
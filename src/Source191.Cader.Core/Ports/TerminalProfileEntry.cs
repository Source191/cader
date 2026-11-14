using Source191.Cader.Domain;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Source191.Cader.Ports;

/// <summary>
/// A profile's entry in Windows Terminal's settings file, as setup adds it to show a hidden profile again: the entry
/// issue #218's workaround showed bringing a profile back.
/// </summary>
public sealed record TerminalProfileEntry(TerminalProfileId Id, string Name, string Source)
{
    /// <summary>The entry on one line, as setup shows it and writes it, so what is shown is what is written.</summary>
    public string Json => $"{{ \"guid\": {Quoted(Id.Text)}, \"hidden\": false, \"name\": {Quoted(Name)}, \"source\": {Quoted(Source)} }}";

    private static string Quoted(string text) => $"\"{JsonEncodedText.Encode(text, JavaScriptEncoder.UnsafeRelaxedJsonEscaping)}\"";
}
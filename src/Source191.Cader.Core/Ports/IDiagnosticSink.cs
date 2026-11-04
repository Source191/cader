namespace Source191.Cader.Ports;

/// <summary>Where the diagnostic log's lines go besides memory: the <c>CADER_LOG</c> file. A sink never throws.</summary>
public interface IDiagnosticSink
{
    void Write(string line);
}
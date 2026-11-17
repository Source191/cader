namespace Source191.Cader.Application.Diagnostics;

/// <summary>A figure taken from the operating system: the value it gave, or <see cref="Unknown"/> when it refused.</summary>
public abstract record Reading<T>
{
    private Reading()
    {
    }

    public sealed record Read(T Value) : Reading<T>;

    public sealed record Unknown : Reading<T>;
}
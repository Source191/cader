using Source191.Cader.Application.Actions;
using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Application.Views;
using System.Reflection;
using System.Reflection.Emit;

namespace Source191.Cader.Core.Tests.Application.Diagnostics;

public sealed class CodeNameShould
{
    [Fact]
    public void NameAnEffectType()
    {
        CodeName name = CodeName.For(typeof(AttachToSession));

        name.ToString().ShouldBe("AttachToSession");
    }

    [Fact]
    public void NameANestedTypeWithTheTypeThatHoldsIt()
    {
        CodeName name = CodeName.For(typeof(StartSessionOutcome.NotListedYet));

        name.ToString().ShouldBe("StartSessionOutcome.NotListedYet");
    }

    [Fact]
    public void NameAGenericTypeWithoutItsArity()
    {
        CodeName name = CodeName.For(typeof(List<int>));

        name.ToString().ShouldBe("List");
    }

    [Fact]
    public void NameAMethodWithItsType()
    {
        MethodBase method = typeof(CodeNameShould).GetMethod(nameof(NameAMethodWithItsType))!;

        CodeName.For(method).ToString().ShouldBe("CodeNameShould.NameAMethodWithItsType");
    }

    [Fact]
    public void NameAMethodThatHasNoTypeByItsOwnName()
    {
        DynamicMethod compiled = new("Compiled", typeof(void), Type.EmptyTypes);

        CodeName.For(compiled).ToString().ShouldBe("Compiled");
    }

    [Fact]
    public void NameAnAsyncMethodByTheMethodTheCompilerMovedItOutOf()
    {
        MethodBase moveNext = typeof(CodeNameShould).GetNestedTypes(BindingFlags.NonPublic)
            .Single(type => type.Name.StartsWith($"<{nameof(WaitAsync)}>", StringComparison.Ordinal))
            .GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)!;

        CodeName.For(moveNext).ToString().ShouldBe("CodeNameShould.WaitAsync");
    }

    [Fact]
    public void NameALambdaByTheMethodItWasWrittenIn()
    {
        Func<int> lambda = static () => 1;

        CodeName.For(lambda.Method).ToString().ShouldBe("CodeNameShould.NameALambdaByTheMethodItWasWrittenIn");
    }

    [Fact]
    public void KeepTheNameOfAMethodThatImplementsAnInterfaceExplicitly()
    {
        MethodBase method = typeof(Explicit).GetMethod("System.IEquatable<System.String>.Equals", BindingFlags.NonPublic | BindingFlags.Instance)!;

        CodeName.For(method).ToString().ShouldBe("CodeNameShould.Explicit.System.IEquatable<System.String>.Equals");
    }

    [Fact]
    public void EqualAnotherNameForTheSameType()
    {
        CodeName.For(typeof(Quit)).ShouldBe(CodeName.For(typeof(Quit)));
    }

    private sealed class Explicit : IEquatable<string>
    {
        bool IEquatable<string>.Equals(string? other) => other is not null;
    }

    private static async Task WaitAsync()
    {
        await Task.Yield();
    }
}
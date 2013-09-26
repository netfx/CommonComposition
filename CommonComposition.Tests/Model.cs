namespace CommonComposition.Tests
{
    using System;
    using System.Linq;

    public interface INamed { }
    public interface IBase { }
    public interface IFoo : IBase { }
    public interface IBar : IBase { }

    [Component(IsSingleton = false)]
    public class Foo : IFoo
    {
        public Foo(IBar bar)
        {
            this.Bar = bar;
        }

        public IBar Bar { get; private set; }
    }

    [Component(IsSingleton = true)]
    internal class Bar : IBar, INamed { }
}
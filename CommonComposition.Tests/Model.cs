namespace CommonComposition.Tests
{

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
    public class Bar : IBar, INamed { }

    public interface IKeyed { }

    [Component]
    [Named("Keyed2")]
    public class Keyed2 : IKeyed { }

    [Component]
    [Named("Keyed")]
    public class Keyed : IKeyed { }

    [Component]
    public class ComponentWithKeyed
    {
        public ComponentWithKeyed(
            [Named("Keyed")] IKeyed dep)
        {
            this.Dep = dep;
        }

        public IKeyed Dep { get; private set; }
    }
}
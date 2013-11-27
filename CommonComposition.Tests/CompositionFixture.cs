namespace CommonComposition.Tests
{
    using Microsoft.Practices.ServiceLocation;
    using System;
    using System.Linq;
    using Xunit;

    public abstract class CompositionFixture
    {
        public CompositionFixture(IServiceLocator locator)
        {
            this.ServiceLocator = locator;
        }

        public IServiceLocator ServiceLocator { get; private set; }

        [Fact]
        public void when_resolving_concrete_type_then_succeeds()
        {
            Assert.NotNull(ServiceLocator.GetInstance<Bar>());
        }

        [Fact]
        public void when_resolving_interface_then_succeeds()
        {
            Assert.NotNull(ServiceLocator.GetInstance<IBar>());
        }

        [Fact]
        public void when_resolving_additional_interface_then_succeeds()
        {
            Assert.NotNull(ServiceLocator.GetInstance<INamed>());
        }

        [Fact]
        public void when_resolving_base_interface_then_succeeds()
        {
            Assert.Equal(2, ServiceLocator.GetAllInstances<IBase>().Count());
        }

        [Fact]
        public void when_resolving_type_with_dependency_then_succeeds()
        {
            Assert.NotNull(ServiceLocator.GetInstance<Foo>());
        }

        [Fact]
        public void when_resolving_singleton_twice_then_returns_same_instance()
        {
            var bar1 = ServiceLocator.GetInstance<Bar>();
            var bar2 = ServiceLocator.GetInstance<Bar>();

            Assert.Same(bar1, bar2);
        }

        [Fact]
        public void when_resolving_non_singleton_twice_then_returns_new_instance()
        {
            var foo1 = ServiceLocator.GetInstance<IFoo>();
            var foo2 = ServiceLocator.GetInstance<IFoo>();

            Assert.NotSame(foo1, foo2);
        }

        [Fact]
        public void when_resolving_keyed_component_then_succeeds()
        {
            var bar = ServiceLocator.GetInstance<IKeyed>("Keyed");

            Assert.NotNull(bar);
        }

        [Fact]
        public void when_resolving_keyed_component_by_implementation_then_succeeds()
        {
            var bar = ServiceLocator.GetInstance<Keyed>("Keyed");
            
            Assert.NotNull(bar);
        }

        [Fact]
        public void when_resolving_component_with_keyed_dependency_then_succeeds()
        {
            var foo = ServiceLocator.GetInstance<ComponentWithKeyed>();

            Assert.NotNull(foo);
            Assert.IsType<Keyed>(foo.Dep);
        }
    }
}
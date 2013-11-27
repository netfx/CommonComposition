namespace CommonComposition.Tests
{
    using Microsoft.Practices.ServiceLocation;
    using System;
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;
    using Xunit;

    public class MefFixture : CompositionFixture
    {
        public MefFixture()
            : base(BuildLocator())
        {
        }

        [Fact]
        public void when_action_then_assert()
        {
            when_resolving_non_singleton_twice_then_returns_new_instance();
        }

        private static IServiceLocator BuildLocator()
        {
            var catalog = new ComponentCatalog(typeof(IFoo).Assembly);
            var container = new CompositionContainer(catalog);

            return new Microsoft.Mef.CommonServiceLocator.MefServiceLocator(container);
        }
    }
}
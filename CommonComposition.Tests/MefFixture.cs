namespace CommonComposition.Tests
{
    using CommonComposition.Mef;
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

        private static IServiceLocator BuildLocator()
        {
            var catalog = new ComponentCatalog(typeof(IFoo).Assembly);
            var container = new CompositionContainer(catalog);

            return new Microsoft.Mef.CommonServiceLocator.MefServiceLocator(container);
        }
    }
}
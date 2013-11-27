namespace CommonComposition.Tests
{
    using CommonComposition;
    using Microsoft.Practices.ServiceLocation;
    using System;
    using System.Collections.Generic;
    using System.Composition;
    using System.Composition.Hosting;
    using System.Linq;
    using Xunit;

    public class MicrosoftMefFixture : CompositionFixture
    {
        public MicrosoftMefFixture()
            : base(BuildLocator())
        {
        }

        private static IServiceLocator BuildLocator()
        {
            var configuration = new ContainerConfiguration();
            configuration.RegisterComponents(typeof(IFoo).Assembly);

            var container = configuration.CreateContainer();

            return new MefServiceLocator(container);
        }

        private class MefServiceLocator : ServiceLocatorImplBase
        {
            private CompositionContext container;

            public MefServiceLocator(CompositionContext container)
            {
                this.container = container;
            }

            protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
            {
                return container.GetExports(serviceType);
            }

            protected override object DoGetInstance(Type serviceType, string key)
            {
                return container.GetExport(serviceType, key);
            }
        }
    }
}
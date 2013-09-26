namespace CommonComposition.Tests
{
    using Microsoft.Practices.ObjectBuilder2;
    using Microsoft.Practices.ServiceLocation;
    using Microsoft.Practices.Unity;
    using System;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class UnityFixture : CompositionFixture
    {
        public UnityFixture()
            : base(BuildLocator())
        {
        }

        private static IServiceLocator BuildLocator()
        {
            var container = new UnityContainer();

            container.RegisterComponents(typeof(IFoo).Assembly);

            return new UnityServiceLocator(container);
        }
    }
}
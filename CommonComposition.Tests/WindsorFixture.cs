namespace CommonComposition.Tests
{
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Microsoft.Practices.ServiceLocation;
    using System;
    using System.Linq;
    using Xunit;
    using System.Reflection;
    using CommonServiceLocator.WindsorAdapter.Unofficial;

    public class WindsorFixture : CompositionFixture
    {
        public WindsorFixture()
            : base(BuildLocator())
        {
        }

        private static IServiceLocator BuildLocator()
        {
            var container = new WindsorContainer();

            container.RegisterComponents(typeof(IFoo).Assembly);

            return new WindsorServiceLocator(container);
        }
    }
}
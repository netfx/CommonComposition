namespace CommonComposition.Tests
{
    using Microsoft.Practices.ServiceLocation;
    using System;
    using System.Linq;
    using Xunit;
    using System.Reflection;
    using CommonServiceLocator.NinjectAdapter.Unofficial;

    public class NinjectFixture : CompositionFixture
    {
        public NinjectFixture()
            : base(BuildLocator())
        {
        }

        private static IServiceLocator BuildLocator()
        {
            var kernel = new Ninject.StandardKernel();

            kernel.RegisterComponents(typeof(IFoo).Assembly);

            return new NinjectServiceLocator(kernel);
        }
    }
}
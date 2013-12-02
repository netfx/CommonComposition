namespace CommonComposition.Tests
{
    using CommonServiceLocator.NinjectAdapter.Unofficial;
    using Microsoft.Practices.ServiceLocation;

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
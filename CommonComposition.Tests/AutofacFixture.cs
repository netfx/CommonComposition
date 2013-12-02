namespace CommonComposition.Tests
{
    using Autofac;
    using Autofac.Extras.CommonServiceLocator;
    using Microsoft.Practices.ServiceLocation;

    public class AutofacFixture : CompositionFixture
    {
        public AutofacFixture()
            : base(BuildLocator())
        {
        }

        private static IServiceLocator BuildLocator()
        {
            var builder = new ContainerBuilder();
            builder.RegisterComponents(typeof(IFoo).Assembly);

            var container = builder.Build();

            return new AutofacServiceLocator(container);
        }
    }
}
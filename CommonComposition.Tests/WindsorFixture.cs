namespace CommonComposition.Tests
{
    using Castle.Windsor;
    using CommonServiceLocator.WindsorAdapter.Unofficial;
    using Microsoft.Practices.ServiceLocation;

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
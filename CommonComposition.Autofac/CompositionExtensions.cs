namespace CommonComposition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Autofac;
    using Autofac.Builder;
    using Autofac.Core;
    using Autofac.Features.Metadata;
    using Autofac.Features.Scanning;

    /// <summary>
    /// Provides automatic component registration by scanning assemblies and types for 
    /// those that have the <see cref="ComponentAttribute"/> annotation.
    /// </summary>
    public static class CompositionExtensions
    {
        /// <summary>
        /// Registers the components found in the given assemblies.
        /// </summary>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterComponents(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            // Allow non-public types just like MEF does.
            return RegisterComponents(builder, assemblies.SelectMany(x => x.GetTypes()));
        }

        /// <summary>
        /// Registers the components found in the given set of types.
        /// </summary>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterComponents(this ContainerBuilder builder, params Type[] types)
        {
            return RegisterComponents(builder, (IEnumerable<Type>)types);
        }

        /// <summary>
        /// Registers the components found in the given set of types.
        /// </summary>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterComponents(this ContainerBuilder builder, IEnumerable<Type> types)
        {
            var registration = builder
                .RegisterTypes(types.Where(t => t.GetCustomAttributes(true).OfType<ComponentAttribute>().Any()).ToArray())
                // Allow non-public constructors just like MEF does.
                .FindConstructorsWith(t => t.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                .AsSelf()
                .AsImplementedInterfaces()
                .PropertiesAutowired(PropertyWiringOptions.PreserveSetValues);

            // Optionally set the SingleInstance behavior.
            registration.ActivatorData.ConfigurationActions.Add((t, rb) =>
            {
                if (rb.ActivatorData.ImplementationType.GetCustomAttributes(true).OfType<ComponentAttribute>().First().IsSingleton)
                    rb.SingleInstance();
            });

            return registration;
        }
    }
}
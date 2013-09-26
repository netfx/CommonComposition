namespace CommonComposition
{
    using Microsoft.Practices.ObjectBuilder2;
    using Microsoft.Practices.Unity;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Provides automatic component registration by scanning assemblies and types for 
    /// those that have the <see cref="ComponentAttribute"/> annotation.
    /// </summary>
    public static class CompositionExtensions
    {
        /// <summary>
        /// Registers the components found in the given assemblies.
        /// </summary>
        public static void RegisterComponents(this IUnityContainer container, params Assembly[] assemblies)
        {
            // Allow non-public types just like MEF does.
            RegisterComponents(container, assemblies.SelectMany(x => x.GetTypes()));
        }

        /// <summary>
        /// Registers the components found in the given set of types.
        /// </summary>
        public static void RegisterComponents(this IUnityContainer container, params Type[] types)
        {
            RegisterComponents(container, (IEnumerable<Type>)types);
        }

        /// <summary>
        /// Registers the components found in the given set of types.
        /// </summary>
        public static void RegisterComponents(this IUnityContainer container, IEnumerable<Type> types)
        {
            if (container.Configure<ComponentContainerExtension>() == null)
            {
                container.AddNewExtension<ComponentContainerExtension>();
            }

            container.RegisterTypes(
                types.Where(t => t.GetCustomAttribute<ComponentAttribute>() != null),
                t => t.GetInterfaces(),
                WithName.Default,
                t => t.GetCustomAttribute<ComponentAttribute>().IsSingleton ?
                    (LifetimeManager)new ContainerControlledLifetimeManager() :
                    (LifetimeManager)new TransientLifetimeManager(),
                t => new InjectionMember[0],
                true);
        }

        private class ComponentContainerExtension : UnityContainerExtension
        {
            protected override void Initialize()
            {
                this.Context.Registering += OnRegistering;
            }

            private void OnRegistering(object sender, RegisterEventArgs e)
            {
                var component = e.TypeTo.GetCustomAttribute<ComponentAttribute>();
                if (component == null)
                    return;

                foreach (var iface in e.TypeTo.GetTypeInfo().ImplementedInterfaces)
                {
                    this.Context.RegisterNamedType(iface, e.TypeTo.FullName);
                    this.Context.Policies.Set<IBuildKeyMappingPolicy>(
                        new BuildKeyMappingPolicy(new NamedTypeBuildKey(e.TypeTo, null)),
                        new NamedTypeBuildKey(iface, e.TypeTo.FullName));
                }
            }
        }
    }
}
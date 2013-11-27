namespace CommonComposition
{
    using Microsoft.Practices.ObjectBuilder2;
    using Microsoft.Practices.Unity;
    using Microsoft.Practices.Unity.ObjectBuilder;
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
                types.Where(t => t.GetCustomAttribute<ComponentAttribute>(true) != null),
                t => t.GetInterfaces(),
                t => t.GetCustomAttributes<NamedAttribute>(true).Select(x => x.Name).FirstOrDefault(),
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
                this.Context.Policies.SetDefault<IConstructorSelectorPolicy>(
                    new WithKeyConstructorSelectorPolicy());
            }

            private void OnRegistering(object sender, RegisterEventArgs e)
            {
                var component = e.TypeTo.GetCustomAttribute<ComponentAttribute>();
                if (component == null)
                    return;
                
                foreach (var iface in e.TypeTo.GetTypeInfo().ImplementedInterfaces)
                {
                    // Register the named component with the full type name since 
                    // Unity does not allow multiple registrations with the same type.
                    this.Context.RegisterNamedType(iface, e.TypeTo.FullName);
                    // Next, we set the policy for building the key for the type without 
                    // key and map it to the one we registered above, with type + name.
                    this.Context.Policies.Set<IBuildKeyMappingPolicy>(
                        new BuildKeyMappingPolicy(new NamedTypeBuildKey(e.TypeTo, null)),
                        new NamedTypeBuildKey(iface, e.TypeTo.FullName));

                    var named = e.TypeTo.GetCustomAttribute<NamedAttribute>(true);

                    if (named != null)
                    {
                        // We must also register the key mapping with the custom key.
                        // key and map it to the one we registered above, with type + name.
                        this.Context.Policies.Set<IBuildKeyMappingPolicy>(
                            new BuildKeyMappingPolicy(new NamedTypeBuildKey(e.TypeTo, named.Name)),
                            new NamedTypeBuildKey(iface, e.TypeTo.FullName));
                    }
                }
            }
        }

        private class WithKeyConstructorSelectorPolicy : DefaultUnityConstructorSelectorPolicy
        {
            protected override IDependencyResolverPolicy CreateResolver(ParameterInfo parameter)
            {
                // Resolve all DependencyAttributes on this parameter, if any
                var attrs = parameter.GetCustomAttributes(false).OfType<NamedAttribute>().ToList();

                if (attrs.Count > 0)
                {
                    // Since this attribute is defined with MultipleUse = false, the compiler will
                    // enforce at most one. So we don't need to check for more.
                    return new NamedTypeDependencyResolverPolicy(parameter.ParameterType, attrs[0].Name);
                }

                return base.CreateResolver(parameter);
            }
        }
    }
}
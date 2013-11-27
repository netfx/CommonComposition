namespace CommonComposition
{
    using System;
    using System.Collections.Generic;
    using System.Composition.Hosting;
    using System.Reflection;
    using System.Linq;
    using System.Composition.Convention;

    /// <summary>
    /// Provides automatic component registration by scanning assemblies and types for 
    /// those that have the <see cref="ComponentAttribute"/> annotation.
    /// </summary>
    public static class CompositionExtensions
    {
        /// <summary>
        /// Registers the components found in the given assemblies.
        /// </summary>
        public static ContainerConfiguration RegisterComponents(this ContainerConfiguration configuration, params Assembly[] assemblies)
        {
            return RegisterComponents(configuration, assemblies.SelectMany(x => x.ExportedTypes));
        }

        /// <summary>
        /// Registers the components found in the given set of types.
        /// </summary>
        public static ContainerConfiguration RegisterComponents(this ContainerConfiguration configuration, params Type[] types)
        {
            return RegisterComponents(configuration, (IEnumerable<Type>)types);
        }

        /// <summary>
        /// Registers the components found in the given set of types.
        /// </summary>
        public static ContainerConfiguration RegisterComponents(this ContainerConfiguration configuration, IEnumerable<Type> types)
        {
            var builder = new ConventionBuilder();
            var candidates = types.Where(t => !t.GetTypeInfo().IsAbstract).ToArray();

            foreach (var type in candidates)
            {
                var info = type.GetTypeInfo();
                var component = info.GetCustomAttribute<ComponentAttribute>(true);
                if (component == null)
                    continue;

                var name = info.GetCustomAttributes<NamedAttribute>(true).Select(x => x.Name).FirstOrDefault();
                var part = builder.ForType(type);

                if (name != null)
                {
                    part.ExportInterfaces(
                            i => i != typeof(IDisposable),
                            (i, b) => b.AsContractName(name))
                        .Export(b => b.AsContractName(name));
                }
                else
                {
                    part.ExportInterfaces(i => i != typeof(IDisposable))
                        .Export();
                }

                if (component.IsSingleton)
                    part.Shared();

                part.SelectConstructor(ctors => ctors.OrderByDescending(ctor => ctor.GetParameters().Length).FirstOrDefault(),
                    (p, b) =>
                    {
                        var namedParam = p.GetCustomAttributes<NamedAttribute>(true).Select(x => x.Name).FirstOrDefault();
                        if (namedParam != null)
                            b.AsContractName(namedParam);
                    });
            }

            configuration.WithParts(candidates, builder);

            return configuration;
        }
    }
}

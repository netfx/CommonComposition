namespace CommonComposition
{
    using System.Composition.Convention;
    using System.Composition.Hosting;
    using System.Composition.Hosting.Core;
    using System.Reflection;
    using System.Linq;
    using System;
    using CommonComposition.Tests;

    public class Test
    {
        public void Do()
        {
            var configuration = new ContainerConfiguration();

            //.WithAssembly(this.GetType().GetTypeInfo().Assembly);

            var builder = new ConventionBuilder();
            var types = this.GetType().GetTypeInfo().Assembly.ExportedTypes.Where(t => !t.IsAbstract).ToArray();

            foreach (var type in types)
            {
                var component = type.GetTypeInfo().GetCustomAttribute<ComponentAttribute>(true);
                if (component == null)
                    continue;

                var name = type.GetTypeInfo().GetCustomAttributes<NamedAttribute>(true).Select(x => x.Name).FirstOrDefault();
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

            configuration.WithParts(types, builder);

            var container = configuration.CreateContainer();

            container.GetExport<Foo>();
        }

        //private class ComponentExportDescriptorProvider : ExportDescriptorProvider
        //{ 
        //}
    }
}

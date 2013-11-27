namespace CommonComposition
{
    using Castle.Components.DictionaryAdapter;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class CompositionExtensions
    {
        public static void RegisterComponents(this IWindsorContainer container, params Assembly[] assemblies)
        {
            RegisterComponents(container, null, assemblies.SelectMany(asm => asm.GetTypes()));
        }

        public static void RegisterComponents(this IWindsorContainer container, Action<BasedOnDescriptor> customize, params Assembly[] assemblies)
        {
            RegisterComponents(container, customize, assemblies.SelectMany(asm => asm.GetTypes()));
        }

        public static void RegisterComponents(this IWindsorContainer container, params Type[] types)
        {
            RegisterComponents(container, null, (IEnumerable<Type>)types);
        }

        public static void RegisterComponents(this IWindsorContainer container, Action<BasedOnDescriptor> customize, params Type[] types)
        {
            RegisterComponents(container, customize, (IEnumerable<Type>)types);
        }

        public static void RegisterComponents(this IWindsorContainer container, IEnumerable<Type> types)
        {
            RegisterComponents(container, null, types);
        }

        public static void RegisterComponents(this IWindsorContainer container, Action<BasedOnDescriptor> customize, IEnumerable<Type> types)
        {
            var descriptor = Classes.From(types)
                .Where(t => t.GetCustomAttributes(typeof(ComponentAttribute), true).OfType<ComponentAttribute>().Any())
                .WithServiceAllInterfaces()
                .WithServiceSelf()
                .Configure(reg =>
                {
                    if (reg.Implementation.GetCustomAttributes(typeof(ComponentAttribute), true).OfType<ComponentAttribute>().First().IsSingleton)
                        reg.LifestyleSingleton();
                    else
                        reg.LifestyleTransient();

                    var named = reg.Implementation.GetCustomAttributes(typeof(NamedAttribute), true)
                        .OfType<NamedAttribute>()
                        .Select(x => x.Name)
                        .FirstOrDefault();

                    if (named != null)
                        reg.Named(named);

                    var ctor = reg.Implementation.GetConstructors()
                        .OrderByDescending(c => c.GetParameters().Length).First();

                    foreach (var prm in ctor.GetParameters()
                        .Select(x => new { Parameter = x, Named = x.GetCustomAttributes(typeof(NamedAttribute), true).OfType<NamedAttribute>().FirstOrDefault() })
                        .Where(x => x.Named != null))
                    {
                        reg.DependsOn(Parameter.ForKey(prm.Parameter.Name).Eq("${" + prm.Named.Name + "}"));
                    }
                });

            if (customize != null)
                customize(descriptor);

            container.Register(descriptor);
        }
    }
}
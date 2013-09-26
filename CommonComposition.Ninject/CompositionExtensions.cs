namespace CommonComposition
{
    using Ninject;
    using Ninject.Syntax;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class CompositionExtensions
    {
        public static void RegisterComponents(this IKernel kernel, params Assembly[] assemblies)
        {
            RegisterComponents(kernel, null, assemblies.SelectMany(asm => asm.GetTypes()));
        }

        public static void RegisterComponents(this IKernel kernel, Action<IBindingNamedWithOrOnSyntax<object>> customize, params Assembly[] assemblies)
        {
            RegisterComponents(kernel, customize, assemblies.SelectMany(asm => asm.GetTypes()));
        }

        public static void RegisterComponents(this IKernel kernel, params Type[] types)
        {
            RegisterComponents(kernel, null, (IEnumerable<Type>)types);
        }

        public static void RegisterComponents(this IKernel kernel, Action<IBindingNamedWithOrOnSyntax<object>> customize, params Type[] types)
        {
            RegisterComponents(kernel, customize, (IEnumerable<Type>)types);
        }

        public static void RegisterComponents(this IKernel kernel, IEnumerable<Type> types)
        {
            RegisterComponents(kernel, null, types);
        }

        public static void RegisterComponents(this IKernel kernel, Action<IBindingNamedWithOrOnSyntax<object>> customize, IEnumerable<Type> types)
        {
            if (customize == null)
                customize = s => { };

            var infos = types
                .Select(t => new { Type = t, Component = (ComponentAttribute)t.GetCustomAttributes(typeof(ComponentAttribute), true).FirstOrDefault() })
                .Where(info => info.Component != null)
                .ToList();

            foreach (var info in infos.Where(i => i.Component.IsSingleton))
            {
                var syntax = kernel
                    .Bind(info.Type.GetInterfaces().Concat(new[] { info.Type }).ToArray())
                    .To(info.Type)
                    .InSingletonScope();

                customize.Invoke(syntax);
            }

            foreach (var info in infos.Where(i => !i.Component.IsSingleton))
            {
                var syntax = kernel
                    .Bind(info.Type.GetInterfaces().Concat(new[] { info.Type }).ToArray())
                    .To(info.Type)
                    .InTransientScope();

                customize.Invoke(syntax);
            }
        }
    }
}
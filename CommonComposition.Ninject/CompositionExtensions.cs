namespace CommonComposition
{
    using Ninject;
    using Ninject.Components;
    using Ninject.Planning;
    using Ninject.Planning.Directives;
    using Ninject.Planning.Strategies;
    using Ninject.Planning.Targets;
    using Ninject.Selection.Heuristics;
    using Ninject.Syntax;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Provides automatic component registration by scanning assemblies and types for 
    /// those that have the <see cref="ComponentAttribute"/> annotation.
    /// </summary>
    /// <remarks>
    /// Several overloads provide seamless Ninject registration custommization.
    /// <example>
    /// The following example registers all annotated components from the given 
    /// given assembly on the given Ninject kernel:
    ///     <code>
    ///     var kernel = new Ninject.StandardKernel();
    ///     
    ///     kernel.RegisterComponents(typeof(IFoo).Assembly);
    ///     </code>
    /// </example>
    /// </remarks>
    public static class CompositionExtensions
    {
        /// <summary>
        /// Registers the components found in the given assemblies.
        /// </summary>
        public static void RegisterComponents(this IKernel kernel, params Assembly[] assemblies)
        {
            RegisterComponents(kernel, null, assemblies.SelectMany(asm => asm.GetTypes()));
        }

        /// <summary>
        /// Registers the components found in the given assemblies.
        /// </summary>
        public static void RegisterComponents(this IKernel kernel, Action<IBindingNamedWithOrOnSyntax<object>> customize, params Assembly[] assemblies)
        {
            RegisterComponents(kernel, customize, assemblies.SelectMany(asm => asm.GetTypes()));
        }

        /// <summary>
        /// Registers the components found in the given types.
        /// </summary>
        public static void RegisterComponents(this IKernel kernel, params Type[] types)
        {
            RegisterComponents(kernel, null, (IEnumerable<Type>)types);
        }

        /// <summary>
        /// Registers the components found in the given types.
        /// </summary>
        public static void RegisterComponents(this IKernel kernel, Action<IBindingNamedWithOrOnSyntax<object>> customize, params Type[] types)
        {
            RegisterComponents(kernel, customize, (IEnumerable<Type>)types);
        }

        /// <summary>
        /// Registers the components found in the given types.
        /// </summary>
        public static void RegisterComponents(this IKernel kernel, IEnumerable<Type> types)
        {
            RegisterComponents(kernel, null, types);
        }

        /// <summary>
        /// Registers the components found in the given types.
        /// </summary>
        public static void RegisterComponents(this IKernel kernel, Action<IBindingNamedWithOrOnSyntax<object>> customize, IEnumerable<Type> types)
        {
            if (customize == null)
                customize = s => { };

            // TODO: auto-set properties?
            //kernel.Components.RemoveAll<IInjectionHeuristic>();
            //kernel.Components.Add<IInjectionHeuristic, AlwaysInjectPropertiesHeuristic>();

            //kernel.Components.Remove<IPlanningStrategy, ConstructorReflectionStrategy>();
            //kernel.Components.Remove < (typeof(ConstructorReflectionStrategy));

            if (!kernel.Components.GetAll<IPlanningStrategy>().OfType<NamedParametersStrategy>().Any())
                kernel.Components.Add<IPlanningStrategy, NamedParametersStrategy>();

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

                SetNamed(syntax, info.Type);

                customize.Invoke(syntax);
            }

            foreach (var info in infos.Where(i => !i.Component.IsSingleton))
            {
                var syntax = kernel
                    .Bind(info.Type.GetInterfaces().Concat(new[] { info.Type }).ToArray())
                    .To(info.Type)
                    .InTransientScope();

                SetNamed(syntax, info.Type);

                customize.Invoke(syntax);
            }
        }

        private static void SetNamed(IBindingNamedWithOrOnSyntax<object> syntax, Type type)
        {
            var named = type
                .GetCustomAttributes(typeof(NamedAttribute), true)
                .OfType<NamedAttribute>()
                .Select(x => x.Name)
                .FirstOrDefault();

            if (named != null)
                syntax.Named(named);
        }

        private class NamedParametersStrategy : NinjectComponent, IPlanningStrategy
        {
            public void Execute(IPlan plan)
            {
                if (plan.Has<ConstructorInjectionDirective>())
                {
                    var ctor = plan.GetOne<ConstructorInjectionDirective>();
                    for (int i = 0; i < ctor.Targets.Length; i++)
                    {
                        var prm = ctor.Targets[i] as ParameterTarget;
                        NamedAttribute named = null;
                        if (prm != null && (named = prm.GetCustomAttributes(typeof(NamedAttribute), true)
                            .OfType<NamedAttribute>().FirstOrDefault()) != null)
                        {
                            // Overwrite the target with a named one.
                            // We just replace the target with one that appends the named attribute.
                            ctor.Targets[i] = new ParameterTarget((MethodBase)prm.Member, new NamedParameterInfo(prm.Site, named.Name));
                        }
                    }
                }
            }
        }

        private class NamedParameterInfo : DelegatingParameterInfo
        {
            private Ninject.NamedAttribute named;

            public NamedParameterInfo(ParameterInfo parameter, string name)
                : base(parameter)
            {
                this.named = new Ninject.NamedAttribute(name);
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                var attrs = base.GetCustomAttributes(attributeType, inherit);
                var result = (object[])Array.CreateInstance(attributeType, attrs.Length + 1);
                attrs.CopyTo(result, 0);
                result[result.Length - 1] = named;
                attrs = result;

                return result;
            }
        }

        private class AlwaysInjectPropertiesHeuristic : StandardInjectionHeuristic
        {
            public override bool ShouldInject(MemberInfo member)
            {
                var propertyInfo = member as PropertyInfo;
                if (propertyInfo != null && propertyInfo.CanWrite)
                    return true;

                return base.ShouldInject(member);
            }
        }
    }
}
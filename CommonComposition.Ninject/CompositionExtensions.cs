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

        private class DelegatingParameterInfo : ParameterInfo
        {
            private readonly ParameterInfo _parameter;

            public DelegatingParameterInfo(ParameterInfo parameter)
            {
                this._parameter = parameter;
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                return this._parameter.GetCustomAttributes(inherit);
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return this._parameter.GetCustomAttributes(attributeType, inherit);
            }

            public override IList<CustomAttributeData> GetCustomAttributesData()
            {
                return this._parameter.GetCustomAttributesData();
            }

            public override Type[] GetOptionalCustomModifiers()
            {
                return this._parameter.GetOptionalCustomModifiers();
            }

            public override Type[] GetRequiredCustomModifiers()
            {
                return this._parameter.GetRequiredCustomModifiers();
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return this._parameter.IsDefined(attributeType, inherit);
            }

            public override string ToString()
            {
                return this._parameter.ToString();
            }

            public override ParameterAttributes Attributes
            {
                get
                {
                    return this._parameter.Attributes;
                }
            }

            public override object DefaultValue
            {
                get
                {
                    return this._parameter.DefaultValue;
                }
            }

            public override MemberInfo Member
            {
                get
                {
                    return this._parameter.Member;
                }
            }

            public override int MetadataToken
            {
                get
                {
                    return this._parameter.MetadataToken;
                }
            }

            public override string Name
            {
                get
                {
                    return this._parameter.Name;
                }
            }

            public override Type ParameterType
            {
                get
                {
                    return this._parameter.ParameterType;
                }
            }

            public override int Position
            {
                get
                {
                    return this._parameter.Position;
                }
            }

            public override object RawDefaultValue
            {
                get
                {
                    return this._parameter.RawDefaultValue;
                }
            }

            public ParameterInfo UnderlyingParameter
            {
                get
                {
                    return this._parameter;
                }
            }
        }

    }
}
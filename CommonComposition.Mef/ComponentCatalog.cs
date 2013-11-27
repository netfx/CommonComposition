namespace CommonComposition.Mef
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Context;

    public class ComponentCatalog : TypeCatalog
    {
        public ComponentCatalog(params Assembly[] assemblies)
            : this(assemblies.SelectMany(a => a.GetTypes()))
        {
        }

        public ComponentCatalog(params Type[] types)
            : this((IEnumerable<Type>)types)
        {
        }

        public ComponentCatalog(IEnumerable<Type> types)
            : base(types.Where(t => t.GetCustomAttribute<ComponentAttribute>(true) != null), CreateRegistration(types))
        {
        }

        private static ReflectionContext CreateRegistration(IEnumerable<Type> types)
        {
            return new ComponentsReflectionContext();

            // Alternative implementation using the new registration conventions.
            // Unfortunately, this does not allow non-public constructors.
            //var builder = new RegistrationBuilder();

            //foreach (var info in types
            //    .Select(t => new { Type = t, Component = t.GetCustomAttribute<ComponentAttribute>(true) })
            //    .Where(info => info.Component != null))
            //{
            //    var policy = info.Component.IsSingleton ? CreationPolicy.Shared : CreationPolicy.NonShared;
            //    var part = builder.ForType(info.Type)
            //        .Export()
            //        .ExportInterfaces()
            //        .SelectConstructor(ctors => ctors.OrderByDescending(ctor => ctor.GetParameters().Length).First())
            //        .SetCreationPolicy(policy);
            //}

            //return builder;
        }

        private class ComponentsReflectionContext : CustomReflectionContext
        {
            protected override IEnumerable<object> GetCustomAttributes(ParameterInfo parameter, IEnumerable<object> declaredAttributes)
            {
                var withKey = parameter.GetCustomAttribute<NamedAttribute>();
                if (withKey == null)
                    return base.GetCustomAttributes(parameter, declaredAttributes);

                // Inject the import attribute with the custom key.
                return base.GetCustomAttributes(parameter, declaredAttributes).Concat(new[] 
                {
                    new ImportAttribute(withKey.Name)
                });
            }

            protected override IEnumerable<object> GetCustomAttributes(MemberInfo member, IEnumerable<object> declaredAttributes)
            {
                var type = member as Type;
                var ctor = member as ConstructorInfo;

                // Automatically injects the [ImportingConstructor] attribute to the ctor
                // with the most parameters.
                if (ctor != null && 
                    ctor == ctor.DeclaringType.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First())
                {
                    return base.GetCustomAttributes(member, declaredAttributes).Concat(new[] { new ImportingConstructorAttribute() });
                }

                if (type == null)
                    return declaredAttributes;

                var component = type.GetCustomAttribute<ComponentAttribute>(true);
                if (component == null)
                    return base.GetCustomAttributes(member, declaredAttributes);

                var additionalAttributes = new List<object>();

                var policy = component.IsSingleton ? CreationPolicy.Shared : CreationPolicy.NonShared;
                additionalAttributes.Add(new PartCreationPolicyAttribute(policy));

                var named = type.GetCustomAttribute<NamedAttribute>(true);

                if (named == null)
                {
                    additionalAttributes.Add(new ExportAttribute(type));
                    additionalAttributes.AddRange(type.GetInterfaces().Select(i => new ExportAttribute(i)));
                }
                else
                {
                    additionalAttributes.Add(new ExportAttribute(named.Name, type));
                    additionalAttributes.AddRange(type.GetInterfaces().Select(i => new ExportAttribute(named.Name, i)));
                }

                return base.GetCustomAttributes(member, declaredAttributes).Concat(additionalAttributes);
            }
        }
    }
}
namespace CommonComposition.Mef
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
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
            : base(types.Where(t => t.IsDefined(typeof(ComponentAttribute), true) && !t.IsAbstract).Select(t => new ComponentType(t)))
        {
        }

        private class ComponentType : TypeDelegator
        {
            private Type type;
            private Lazy<ComponentAttribute> component;
            private Lazy<object[]> additionalAttributes;
            private Lazy<PartCreationPolicyAttribute[]> creationAttributes;
            private Lazy<ExportAttribute[]> exportAttributes;
            private Lazy<string> name;

            public ComponentType(Type type)
                : base(type)
            {
                this.type = type;
                this.component = new Lazy<ComponentAttribute>(() =>
                    type.GetCustomAttributes(typeof(ComponentAttribute), true).OfType<ComponentAttribute>().First());

                this.exportAttributes = new Lazy<ExportAttribute[]>(() =>
                {
                    var exports = new List<ExportAttribute>();
                    exports.Add(new ExportAttribute(name.Value, type));
                    exports.AddRange(type
                        .GetInterfaces()
                        .Where(i => i != typeof(IDisposable))
                        .Select(i => new ExportAttribute(name.Value, i)));

                    return exports.ToArray();
                });

                this.creationAttributes = new Lazy<PartCreationPolicyAttribute[]>(() =>
                    new PartCreationPolicyAttribute[] { new PartCreationPolicyAttribute(component.Value.IsSingleton ? CreationPolicy.Shared : CreationPolicy.NonShared) });

                this.additionalAttributes = new Lazy<object[]>(() => new object[0].Concat(exportAttributes.Value).Concat(creationAttributes.Value).ToArray());

                this.name = new Lazy<string>(() => type
                    .GetCustomAttributes(typeof(NamedAttribute), true)
                    .OfType<NamedAttribute>()
                    .Select(x => x.Name)
                    .FirstOrDefault());
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                if (attributeType == typeof(ExportAttribute) ||
                    attributeType == typeof(InheritedExportAttribute) ||
                    attributeType == typeof(PartCreationPolicyAttribute))
                    return true;

                return base.IsDefined(attributeType, inherit);
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                return base.GetCustomAttributes(inherit).Concat(additionalAttributes.Value).ToArray();
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                if (attributeType == typeof(ExportAttribute))
                    return exportAttributes.Value;
                else if (attributeType == typeof(PartCreationPolicyAttribute))
                    return creationAttributes.Value;

                return base.GetCustomAttributes(attributeType, inherit);
            }

            public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
            {
                var ctor = base.GetConstructors(bindingAttr).OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
                if (ctor != null)
                    return new ConstructorInfo[] { new ImportingConstructorInfo(ctor) };

                return new ConstructorInfo[0];
            }

            private class ImportingConstructorInfo : DelegatingConstructorInfo
            {
                private Lazy<ParameterInfo[]> parameters;

                public ImportingConstructorInfo(ConstructorInfo constructor)
                    : base(constructor)
                {
                    this.parameters = new Lazy<ParameterInfo[]>(() => constructor.GetParameters().Select(x => new ImportedParameterInfo(x)).ToArray());
                }

                public override bool IsDefined(Type attributeType, bool inherit)
                {
                    if (attributeType == typeof(ImportingConstructorAttribute))
                        return true;

                    return base.IsDefined(attributeType, inherit);
                }

                public override object[] GetCustomAttributes(Type attributeType, bool inherit)
                {
                    if (attributeType == typeof(ImportingConstructorAttribute))
                        return new ImportingConstructorAttribute[] { new ImportingConstructorAttribute() };

                    return base.GetCustomAttributes(attributeType, inherit);
                }

                public override object[] GetCustomAttributes(bool inherit)
                {
                    return base.GetCustomAttributes(inherit)
                        .Concat(new ImportingConstructorAttribute[] { new ImportingConstructorAttribute() })
                        .ToArray();
                }

                public override ParameterInfo[] GetParameters()
                {
                    return parameters.Value;
                }
            }

            private class ImportedParameterInfo : DelegatingParameterInfo
            {
                private ImportAttribute import;

                public ImportedParameterInfo(ParameterInfo parameter)
                    : base(parameter)
                {
                    var name = parameter.GetCustomAttributes(typeof(NamedAttribute), true)
                        .OfType<NamedAttribute>()
                        .Select(x => x.Name)
                        .FirstOrDefault();

                    this.import = new ImportAttribute(name);
                }

                public override bool IsDefined(Type attributeType, bool inherit)
                {
                    return base.IsDefined(attributeType, inherit);
                }

                public override object[] GetCustomAttributes(Type attributeType, bool inherit)
                {
                    var attrs = base.GetCustomAttributes(attributeType, inherit);
                    var result = (object[])Array.CreateInstance(attributeType, attrs.Length + 1);
                    attrs.CopyTo(result, 0);
                    result[result.Length - 1] = import;
                    attrs = result;

                    return result;
                }
            }
        }
    }
}
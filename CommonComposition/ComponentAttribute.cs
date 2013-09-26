namespace CommonComposition
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// Marks the decorated class as a component that will be registered 
    /// for composition
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ComponentAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentAttribute"/> class, 
        /// marking the decorated class as a component that will be registered 
        /// for composition.
        /// </summary>
        public ComponentAttribute()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether this component should be treated as a singleton 
        /// or single instance within the container. Defaults to <see langword="false"/>.
        /// </summary>
        [DefaultValue(false)]
        public bool IsSingleton { get; set; }
    }
}
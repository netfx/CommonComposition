![Icon](https://raw.github.com/clariuslabs/CommonComposition/master/noun_project_3427.png) Common Composition
=================

Portable component composition annotations, dependency injection framework agnostic.

## Why
Typically, application logic doesn't require deep knowledge of specific dependency injection framework features, APIs and extensibility points. It can even be considered a best practice to keep your logic code devoid of specific DI frameworks types and quirks. 

Most frameworks today allow for convention-based registration of components (like "register all concrete types with their implemented interfaces"). A typical annotation that's usually needed is whether the registered component should be instantiated only once in a container and reused (like a container-scoped singleton) or if a new instance should be created every time the component is requested. 

It would be clearly desirable to be able to express that type of annotation (as well as whether a type should be registered for composition at all) in a framework agnostic way. That's precisely what Common Composition is for. 

The Common Composition provides basic annotation attributes you need to specify how components should be composed in a dependency injection container. It allows your application logic to remain DI framework agnostic while still leveraging the benefits of convention-based configuration.

In addition, Common Composition will provide a set of default configurations for various containers so that the behave consistently with regards to some common relationship types, such as dependencies on Func<T> factories, IEnumerable<T>, etc.

### What about Common Service Locator?
The [Common Service Locator](http://commonservicelocator.codeplex.com/) project also provides an abstraction over IoC containers, but it's intended consumers are other frameworks, not application code. So much that a Service Locator is considered by most to be an [anti-pattern](https://www.google.com.ar/search?q=service+locator+anti+pattern). 

Your application code should clearly NOT depend on that abstraction.


## What
The goal is to keep the annotations you make on your application logic to a minimum. Currently, you can annotate your types that should be registered for composition with the `[ComponentAttribute]`:

    [Component(IsSingleton = false)]
    public class Foo : IFoo
    {
        public Foo(IBar bar)
        {
            this.Bar = bar;
        }

        public IBar Bar { get; private set; }
    }

    [Component(IsSingleton = true)]
    internal class Bar : IBar { } 

    public interface IComponent { }
    public interface IFoo : IComponent { }
    public interface IBar : IComponent { }


Under Common Composition conventions, all major dependency injection frameworks (Autofac, Ninject, Windsor, Unity and MEF) will make both Foo and Bar available (note Bar is an internal class), and resolving all IComponent instances will shield both Foo and Bar. Also, resolving twice the Bar/IBar component will return the same instance, since it's configured to be a singleton.

A common set of unit tests that run and are required to pass on all supported containers, ensures this common set of behaviors. 

Here's how to leverage Common Composition on each of the supported frameworks:

1. Autofac:

            var builder = new ContainerBuilder();
            builder.RegisterComponents(typeof(IFoo).Assembly);

            var container = builder.Build();

2. Ninject:

            var kernel = new Ninject.StandardKernel();

            kernel.RegisterComponents(typeof(IFoo).Assembly);

3. Windsor:

            var container = new WindsorContainer();

            container.RegisterComponents(typeof(IFoo).Assembly);

4. Unity:

            var container = new UnityContainer();

            container.RegisterComponents(typeof(IFoo).Assembly);

5. MEF:

            var catalog = new ComponentCatalog(typeof(IFoo).Assembly);
            var container = new CompositionContainer(catalog);


## Install

On your application logic projects:

            install-package CommonComposition

On your application bootstrapping code (i.e. your Main(), or web application startup procedure, etc.), depending on the chosen container implementation:

            install-package CommonComposition.Autofac
            install-package CommonComposition.Ninject
            install-package CommonComposition.Windsor
            install-package CommonComposition.Unity
            install-package CommonComposition.Mef


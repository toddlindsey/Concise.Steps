using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Concise.Steps.IoC
{
    internal class ConciseContainer : IContainer
    {
        private class Registration
        {
            public Type ServiceType;
            public Type ImplementationType;
            public bool IsSingleton;
            public object SingletonInstance;
        }

        IDictionary<Type, Registration> registrations = new Dictionary<Type, Registration>();
        Resolver resolver;

        public ConciseContainer()
        {
            this.resolver = new Resolver(this);
        }

        public void RegisterTransient(Type serviceType, Type implementationType)
        {
            var registration = new Registration
            {
                ServiceType = serviceType,
                ImplementationType = implementationType,
                IsSingleton = false
            };
            registrations[serviceType] = registration;
        }

        public void RegisterSingleton(Type serviceType, Type implementationType)
        {
            var registration = new Registration
            {
                ServiceType = serviceType,
                ImplementationType = implementationType,
                IsSingleton = true
            };
            registrations[serviceType] = registration;
        }

        private object Resolve(Type serviceType)
        {
            Registration registration;
            if (!this.registrations.TryGetValue(serviceType, out registration))
                throw new InvalidOperationException($"No registration exists for {serviceType.FullName}");

            if (registration.IsSingleton)
            {
                if (registration.SingletonInstance != null)
                    return registration.SingletonInstance;
                else
                {
                    // Ensure thread safety on the singleton create
                    lock (registration)
                    {
                        if (registration.SingletonInstance != null)
                            return registration.SingletonInstance;

                        registration.SingletonInstance = this.CreateUsingConstructorInjection(registration.ImplementationType);
                        return registration.SingletonInstance;
                    }
                }
            }
            else
                return this.CreateUsingConstructorInjection(registration.ImplementationType);
        }

        private object CreateUsingConstructorInjection(Type objectType)
        {
            ConstructorInfo[] constructors = objectType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            if (constructors.Length > 1)
                throw new InvalidOperationException($"Multiple constructors are not supported on type {objectType.FullName}");
            if (constructors.Length == 0)
                throw new InvalidOperationException($"No public instance constructor exists for type {objectType.FullName}");

            Type[] argumentTypes = constructors.First().GetParameters().Select(pi => pi.ParameterType).ToArray();
            object[] args = argumentTypes.Select(argType => this.Resolve(argType)).ToArray();
            return Activator.CreateInstance(objectType, args);
        }

        private class Resolver : IServiceProvider
        {
            private ConciseContainer container;

            public Resolver(ConciseContainer container)
            {
                this.container = container;
            }

            public object GetService(Type serviceType)
            {
                return container.Resolve(serviceType);
            }
        }

        public IServiceProvider Provider
        {
            get { return this.resolver; }
        }
    }
}

using Concise.Steps.TestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

#if NETSTANDARD1_6
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.PlatformAbstractions;
#endif

namespace Concise.Steps.IoC
{
    internal class Bootstrapper
    {
        static Bootstrapper()
        {
            Bootstrapper.InitializeWithConciseContainer();
        }

        public static IContainer InitializeWithConciseContainer()
        {
            var container = new ConciseContainer();
            InitializeWithContainer(container);
            return container;
        }

        private static void InitializeWithContainer(IContainer container)
        {
            // Find and register the loaded ITestFrameworkAdapter by looking through assemblies that directly reference this one
            {
                string currentAssemblyFullName = typeof(Bootstrapper).GetTypeInfo().Assembly.FullName;

                // Libraries directly referencing this one
                IList<Assembly> assembliesReferencingThisAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .Where((Assembly x) => 
                        x.GetName().FullName != currentAssemblyFullName && 
                        x.GetReferencedAssemblies().Any((AssemblyName y) => y.FullName == currentAssemblyFullName))
                    .ToList();

                // All types in those libraries implementing ITestFrameworkAdapter
                IList<Type> typesImplementingInterface = assembliesReferencingThisAssembly
                    .SelectMany(assembly => assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(ITestFrameworkAdapter))))
                    .ToList();

                if (!typesImplementingInterface.Any())
                    throw new InvalidOperationException("Unable to find a registered test adapter.  Did you forget to add a NuGet reference to Concise.Steps.MSTest or Concise.Steps.NUnit?");

                if( typesImplementingInterface.Count > 1 )
                {
                    throw new InvalidOperationException(
                        $"Multiple registered test framework adapters located - please remove a referenced assembly so that only one adapter is present." +
                        $" ({string.Join(",", typesImplementingInterface.Select(x => x.FullName))})");
                }

                container.RegisterSingleton(typeof(ITestFrameworkAdapter), typesImplementingInterface.First());
            }

            Bootstrapper.Locator = container.Provider;
        }

        /// <summary>
        /// Internal property for service locator use
        /// </summary>
        internal static IServiceProvider Locator { get; private set; }
    }
}

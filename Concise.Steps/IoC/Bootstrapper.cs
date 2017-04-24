using Concise.Steps.TestFramework;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
                // .NET Standard 1.6 will allow us to find and load the right assembly.  For now we have to use .NET Standard 1.5
#if NETSTANDARD1_6
                string currentAssemblyFullName = typeof(Bootstrapper).GetTypeInfo().Assembly.FullName;

                // Libraries directly referencing this one
                IList<RuntimeLibrary> referencingLibraries = DependencyContext.Default.RuntimeLibraries
                    .Where(x => x.Name != currentAssemblyFullName && x.Dependencies.Any(y => y.Name == currentAssemblyFullName))
                    .ToList();

                // All types in those libraries implementing ITestFrameworkAdapter
                IList<Type> typesImplementingInterface = referencingLibraries
                    .Select(library => Assembly.Load(new AssemblyName(library.Name)))
                    .SelectMany(assembly => assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(ITestFrameworkAdapter))))
                    .ToList();

                if (!typesImplementingInterface.Any())
                    throw new InvalidOperationException("Unable to find a registered test adapter.  Did you forget to add a NuGet reference to Concise.Steps.MSTest or Concise.Steps.XUnit?");

                if( typesImplementingInterface.Count > 1 )
                {
                    throw new InvalidOperationException(
                        $"Multiple registered test framework adapters located - please remove a referenced assembly so that only one adapter is present." +
                        $" ({string.Join(",", typesImplementingInterface.Select(x => x.FullName))})");
                }

                container.RegisterSingleton(typeof(ITestFrameworkAdapter), typesImplementingInterface.First());
#else
                // Until we switch over to .NET Standard 1.6, we are just going to directly require Concise.Steps.MSTest:

                string msTestName = "Concise.Steps.MSTest";
                Assembly msTestAssembly;
                try
                {
                    msTestAssembly = Assembly.Load(new AssemblyName(msTestName));
                }
                catch(Exception ex)
                {
                    throw new InvalidOperationException("You must add a NuGet reference to Concise.Steps.MSTest", ex);
                }

                Type msTestAdapterType = msTestAssembly.GetTypes().Single(x => x.FullName == "Concise.Steps.MSTest.TestFramework.MSTestAdapter");
                container.RegisterSingleton(typeof(ITestFrameworkAdapter), msTestAdapterType);
#endif
            }

            Bootstrapper.Locator = container.Provider;
        }

        /// <summary>
        /// Internal property for service locator use
        /// </summary>
        internal static IServiceProvider Locator { get; private set; }
    }
}

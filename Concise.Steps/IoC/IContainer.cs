using System;
using System.Collections.Generic;
using System.Text;

namespace Concise.Steps.IoC
{
    internal interface IContainer
    {
        IServiceProvider Provider { get; }

        void RegisterSingleton(Type serviceType, Type implementationType);

        void RegisterTransient(Type serviceType, Type implementationType);
    }
}

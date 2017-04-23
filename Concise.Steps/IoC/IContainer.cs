using System;
using System.Collections.Generic;
using System.Text;

namespace Concise.Steps.IoC
{
    public interface IContainer
    {
        IServiceProvider Provider { get; }

        void RegisterSingleton<TService, TImplementation>();

        void RegisterTransient<TService, TImplementation>();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concise.Steps.DemoTests.TestSubject
{
    public interface IObjectUnderTest
    {
        int Add(int a, int b);

        int AddPoorly(int a, int b);

        int DelegatedAdd(int a, int b);

        Task<int> AsyncAdd(int a, int b);
    }
}

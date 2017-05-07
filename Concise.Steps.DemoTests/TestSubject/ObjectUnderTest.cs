using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concise.Steps.DemoTests.TestSubject
{
    public class ObjectUnderTest : IObjectUnderTest
    {
        public int Add(int a, int b)
        {
            return a + b;
        }

        public int AddPoorly(int a, int b)
        {
            return a + b + 1;
        }

        public Task<int> AsyncAdd(int a, int b)
        {
            throw new NotImplementedException();
        }

        public int DelegatedAdd(int a, int b)
        {
            throw new NotImplementedException();
        }
    }
}

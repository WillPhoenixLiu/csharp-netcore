using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AopTest;

namespace CustomerApi.Aop
{
    [OpenTracingLoggingAspect]
    public class TestClass
    {
        string _a;
        string _b;
        public TestClass(string a, string b)
        {
            _a = a;
            _b = b;
        }
        public string Add()
        {
            return _a + _b;
        }
    }
}

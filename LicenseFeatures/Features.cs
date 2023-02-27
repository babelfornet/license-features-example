using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LicenseFeatures
{
    [Obfuscation(Feature = "msil encryption:internal=true;source=feature1;cache=false;password=aR&tj4i7u@", Exclude = false)]
    internal class Feature1
    {
        public Feature1() { }

        public string DoTask()
        {
            return "Feature1: " + Add(7, 8);
        }

        public int Add(int a, int b) => a + b;
    }

    [Obfuscation(Feature = "msil encryption:internal=true;source=feature2;cache=false;password=q4oZ7Y%pk0", Exclude = false)]
    internal class Feature2
    {
        public Feature2() { }

        public string DoTask()
        {
            return "Feature2: " + Multiply(7, 8);
        }

        public int Multiply(int a, int b) => a * b;
    }

    [Obfuscation(Feature = "msil encryption:internal=true;source=feature3;cache=false;password=U1#3ln2$vx", Exclude = false)]
    internal class Feature3
    {
        public Feature3() { }

        public string DoTask()
        {
            return "Feature3: " + Factorial(7);
        }

        public int Factorial(int a) => (a <= 1) ? 1 : a* Factorial(a - 1);
    }
}

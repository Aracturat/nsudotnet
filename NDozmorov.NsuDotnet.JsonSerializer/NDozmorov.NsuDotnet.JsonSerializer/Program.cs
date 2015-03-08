using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NDozmorov.NsuDotnet.JsonSerializer
{
    [Serializable]
    class TestClass
    {
        public int Int { get; set; }
        public string Str;

        [NonSerialized]
        public string Ignore; 

        public int[] IntArray;
        
        public AnotherTestClass[] AnotherTestClassArray;

        public AnotherTestClass AnotherTest;
    }

    [Serializable]
    class AnotherTestClass
    {
        public int Int;
        public string Str;
        public bool Flag;
        public string NullStr = null;
    }

    class Program
    {
        static void Main(string[] args)
        {
            TestClass test = new TestClass
            {
                Int = 20,
                Str = "string",
                Ignore = "ignStr",
                IntArray = new int[] {2, 3, 4},
                AnotherTest = new AnotherTestClass() {Int = 20, Str = "string", Flag = true},
                AnotherTestClassArray = new[]
                {
                    new AnotherTestClass() {Int = 21, Str = "string2", Flag = true},
                    new AnotherTestClass() {Int = 24, Str = "string3", Flag = false}
                }
            };


            using (var fileStream = new FileStream("log.txt", FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    JsonSerializer.Serialize(test, streamWriter);
                }
            }
        }
    }
}

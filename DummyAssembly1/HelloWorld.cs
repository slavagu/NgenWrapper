using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DummyAssembly1
{
    public class HelloWorld
    {
        public static void Print()
        {
            Console.WriteLine("Hello world from DummyAssembly1");
            DummyAssembly2.HelloWorld.Print();
        }
    }
}

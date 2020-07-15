using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main()
        {
            string name = "Door Name [ATMO] (room 1) (room 2)";
            int a = name.LastIndexOf("(") + 1;
            int b = name.LastIndexOf(")");
            Console.WriteLine(name.Substring(a, b - a));

            name = name.Substring(0, a-1);
            a = name.LastIndexOf("(") + 1;
            b = name.LastIndexOf(")");
            Console.WriteLine(name.Substring(a, b - a));

        }
    }
}

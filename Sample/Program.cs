using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    class Program
    {

        interface Ia
        {
            int a { get; set; }
        }

        interface Ib : Ia
        {
            int b { get; set; }
        }

  
        static void Main(string[] args)
        {



        }

        static void AAA(Ia item)
        {
            Console.Write(item.a);
        }
    }
}

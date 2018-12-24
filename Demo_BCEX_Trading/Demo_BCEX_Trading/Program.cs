using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo_BCEX_Trading
{
    class Program
    {
       /// <summary>
       /// main function
       /// </summary>
       /// <param name="args"></param>
        static void Main(string[] args)
        {
           
            CSettings.Init();

            CSimulator simulator = new CSimulator();

            simulator.Run();

            Console.ReadKey();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Connector.Tests;

namespace for_profiler
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = new LoadTest();
            x.Setup();
            try
            {
                x.SetsPerSecondWith50Threads();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            x.TearDown();
        }
    }
}

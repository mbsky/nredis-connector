using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Connector;
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
                //using(var conn = RedisConnection.Connect("localhost", 6379))
                //{
                //    var f = new CommandFactory(new NormalCommandExecutor(conn));
                //    f.Set("foo", new byte[515<<10]).Exec();
                    
                //}
                x.SetsPerSecondOn512kbSet();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            x.TearDown();
        }
    }
}

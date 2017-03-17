using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QlikConnect
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("QlikConnector");

                var path = @"C:\Users\MBerthold\Documents\Entwicklung\Projects\QlikConnector\ScriptSigner\bin\Debug\SignScripts\Demo.ps1";
                var script = File.ReadAllText(path);
                var qlikConnector = new QlikConnector(script);
                var table = qlikConnector.GetTable();

                Console.WriteLine("\nFertig...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var filePath = string.Empty;
            if (args.Length > 0)
                filePath = args[0];
            else
                filePath = @"C:\work\Fun stuff\ConsoleApp\ConsoleApp\bin\Debug\ConsoleApp.exe";

            if (!string.IsNullOrEmpty(filePath))
            {
                var r = new ReflectionReader();
                if (r.CheckIfFileExists(filePath))
                {
                    var assembly = Assembly.LoadFrom(filePath);
                    r.WriteAssemlyInfo(assembly);
                }
                Console.WriteLine("File path noth found!");
            }
            else
                Console.WriteLine("File argument not set!");

            Console.WriteLine("Press any key to exit.");
            Console.Read();
        }
    }    
}

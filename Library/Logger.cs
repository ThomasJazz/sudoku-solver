using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using Newtonsoft.Json;
using Spectre.Console;

namespace sudoku.solver
{
    public class Logger
    {
        public string? FunctionName { get; set; }

        /// <summary>
        /// When Logger is initialized from PlaySudoku.cs...
        /// Environment.StackTrace.Split("\r\n")[2].Split(":line")[0].Split("\\")[^1];
        /// will yield "PlaySudoku.cs"
        /// </summary>
        public Logger()
        {
            var methodFrame = new StackTrace()!.GetFrame(1)!;
            string methodName = methodFrame!
                .GetMethod()!
                .DeclaringType!
                .FullName!
                .Split(".")![^1];

            Console.WriteLine(methodName);
        }

        public string GetStackTrace()
        {
            return Environment.StackTrace;
        }

        public void PrintJson(object o, bool indent=true)
        {
            if (indent)
                Console.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
            else
                Console.WriteLine(JsonConvert.SerializeObject(o));
        }
    }
}
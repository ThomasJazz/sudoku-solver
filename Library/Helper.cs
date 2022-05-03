using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace sudoku.solver
{
    public static class Helper
    {
        public static List<T> Flatten2dList<T>(List<List<T>> dataset)
        {
            List<T> flat = new List<T>();

            foreach (List<T> list in dataset)
            {
                flat.AddRange(list);
            }

            return flat;
        }

        public static void PrintJson(object o, bool indent=true)
        {
            if (indent)
                Console.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
            else
                Console.WriteLine(JsonConvert.SerializeObject(o));
        }
    }
}
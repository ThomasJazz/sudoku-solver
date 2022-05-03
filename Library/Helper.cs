using System;
using System.Collections.Generic;

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
    }
}
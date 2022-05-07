using System;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace sudoku.solver
{
    public class XWing
    {
        public int Value { get; set; } = 0;
        public Dictionary<int, List<Tile>> RowMatches { get; set; } = new Dictionary<int, List<Tile>>();
        public Dictionary<int, List<Tile>> ColumnMatches { get; set; } = new Dictionary<int, List<Tile>>();

        public XWing(int Value)
        {
            this.Value = Value;
        }
    }
}
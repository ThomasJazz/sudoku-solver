using System;
using System.Collections.Generic;

namespace sudoku.solver
{
    class Tile
    {
        public int Value { get; set; } = 0;
        public int Row { get; }
        public int Column { get; }
        public int GroupNumber { get; }
        public bool IsFinal { get; } = false;

        public Tile(int Value, int Row, int Column)
        {
            this.Value = Value;
            this.Row = Row;
            this.Column = Column;

            int groupLevel = 2 * (Row/3);
            this.GroupNumber = groupLevel + ((Row / 3) + (Column / 3));
        }

        public Tile(int Value, int Row, int Column, bool IsFinal)
        {
            this.Value = Value;
            this.Row = Row;
            this.Column = Column;
            this.IsFinal = IsFinal;

            int groupLevel = 2 * (Row / 3);
            this.GroupNumber = groupLevel + ((Row / 3) + (Column / 3));
        }

        public override string ToString()
        {
            if (Value == 0)
                return " : ";
            else
                return $" {this.Value.ToString()} ";
        }
    }
}
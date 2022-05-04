using System;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace sudoku.solver
{
    class TileParent
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public int GroupNumber { get; set; }

        // public TileParent(int Row, int Column) 
        // {
        //     this.Row = Row;
        //     this.Column = Column;
        // }

        public void SetGroupNumber()
        {
            int groupLevel = 2 * (this.Row/3);
            this.GroupNumber = groupLevel + ((this.Row / 3) + (this.Column / 3));
        }
    }

    class Tile : TileParent
    {
        public int Value { get; set; } = 0;
        
        [JsonIgnore]
        public bool IsFinal { get; } = false;

        public Tile(int Value, int Row, int Column)
        {
            this.Value = Value;
            this.Row = Row;
            this.Column = Column;

            this.SetGroupNumber();
        }

        public Tile(int Value, int Row, int Column, bool IsFinal)
        {
            this.Value = Value;
            this.Row = Row;
            this.Column = Column;
            this.IsFinal = IsFinal;

            this.SetGroupNumber();
        }

        public override string ToString()
        {
            if (Value == 0)
                return " : ";
            else
                return $" {this.Value.ToString()} ";
        }

        public string ToCoordsString()
        {
            return $"{this.Row}-{this.Value}";
        }
    }

    class TileCandidate : TileParent
    {
        public List<int> PossibleValues { get; set; } = new List<int>();

        public TileCandidate(int Row, int Column)
        {
            this.Row = Row;
            this.Column = Column;
            this.SetGroupNumber();
        }
    }
}
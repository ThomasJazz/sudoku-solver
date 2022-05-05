using System;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace sudoku.solver
{
    public class TileParent
    {
        [JsonProperty("row")]
        public int Row { get; set; }

        [JsonProperty("col")]
        public int Column { get; set; }

        [JsonProperty("group")]
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

    public class Tile : TileParent
    {
        [JsonProperty("val")]
        public int Value { get; set; } = 0;

        public HashSet<Tile> Candidates { get; set; }
        
        [JsonIgnore]
        public bool IsFinal { get; } = false;

        public Tile(int Value, int Row, int Column)
        {
            this.Value = Value;
            this.Row = Row;
            this.Column = Column;

            this.SetGroupNumber();
            this.Candidates = new HashSet<Tile>();
        }

        public Tile(int Value, int Row, int Column, bool IsFinal)
        {
            this.Value = Value;
            this.Row = Row;
            this.Column = Column;
            this.IsFinal = IsFinal;

            this.SetGroupNumber();
            this.Candidates = new HashSet<Tile>();
        }

        /********** TOSTRING VARIATIONS **********/
        public override string ToString()
        {
            if (Value == 0)
                return " : ";
            else
                return $" {this.Value.ToString()} ";
        }

        public string ToShortString()
        {
            return $"[{this.Row},{this.Column}]: {this.Value}";
        }

        public string ToCoordsString()
        {
            return $"[{this.Row},{this.Value}]";
        }
    }

    // public class TileCandidate : TileParent
    // {
    //     public HashSet<int> PossibleValues { get; set; } = new HashSet<int>();

    //     public TileCandidate(int Row, int Column)
    //     {
    //         this.Row = Row;
    //         this.Column = Column;
    //         this.SetGroupNumber();
    //     }

    //     public TileCandidate(int Row, int Column, HashSet<int> )
    //     {
    //         this.Row = Row;
    //         this.Column = Column;
    //         this.SetGroupNumber();
    //     }
    // }
}
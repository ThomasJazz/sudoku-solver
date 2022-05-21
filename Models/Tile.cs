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

        //public HashSet<Candidate> Candidates { get; set; }
        //public HashSet<Tile> Candidates { get; set; }
        [JsonIgnore]
        public HashSet<int> Candidates { get; set; }
        
        [JsonIgnore]
        public bool IsFinal { get; } = false;

        /****** CONSTRUCTORS ******/
        public Tile(int Value, int Row, int Column)
        {
            this.Value = Value;
            this.Row = Row;
            this.Column = Column;

            this.SetGroupNumber();
            //this.Candidates = new HashSet<Tile>();
            this.Candidates = new HashSet<int>();
        }

        public Tile(int Value, int Row, int Column, bool IsFinal)
        {
            this.Value = Value;
            this.Row = Row;
            this.Column = Column;
            this.IsFinal = IsFinal;

            this.SetGroupNumber();
            //this.Candidates = new HashSet<Tile>();
            this.Candidates = new HashSet<int>();
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

        public string ToCandidateString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 9; i++)
            {

            }

            return sb.ToString();
        }

        /********** ACCESSORS/MUTATORS **********/
        public void AddCandidate(int num)
        {
            //this.Candidates.Add(new Tile(num, this.Row, this.Value));
            this.Candidates.Add(num);
        }

        public void AddCandidates(HashSet<int> numbers)
        {
            foreach (int num in numbers)
            {
                //this.Candidates.Add(new Tile(num, this.Row, this.Value));
                this.Candidates.Add(num);
            }
        }

        // public void SetCandidates(List<Tile> tiles)
        // {
        //     this.Candidates = tiles.ToHashSet();
        // }

        public void SetCandidates(List<int> candidates)
        {
            this.Candidates = candidates.ToHashSet();
        }

        public bool HasCandidate(int num)
        {
            // HashSet<int> candidateNums = this.Candidates.Select(cand => cand.Value).ToHashSet();
            // return candidateNums.Contains(num);
            return this.Candidates.Contains(num);
        }
    }

    public class Candidate : TileParent
    {
        public int Value { get; set; }

        public Candidate(int Row, int Column)
        {
            this.Row = Row;
            this.Column = Column;
            this.SetGroupNumber();
        }

        public Candidate(int Value, int Row, int Column)
        {
            this.Row = Row;
            this.Column = Column;
            this.Value = Value;
        }

        public Candidate(Tile Tile, int Value)
        {
            this.Row = Tile.Row;
            this.Column = Tile.Column;
            this.Value = Value;
        }

        public Tile ToTile()
        {
            return new Tile(this.Value, this.Row, this.Column);
        }
    }
}
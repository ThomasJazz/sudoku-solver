using System;
using System.Collections.Generic;

namespace sudoku.solver
{
    class TileGroup
    {
        public List<Tile> Tiles = new List<Tile>();

        public TileGroup(){}
    }

    public class RowLock
    {
        public int GroupNumber { get; set; }
        public int LockedRow { get; set; }
        public HashSet<int> LockedNumbers { get; set; } = new HashSet<int>();

        public RowLock(int GroupNumber)
        {
            this.GroupNumber = GroupNumber;
        }

        public RowLock(int GroupNumber, int LockedRow, HashSet<int> LockedNumbers)
        {
            this.GroupNumber = GroupNumber;
            this.LockedRow = LockedRow;
            this.LockedNumbers = LockedNumbers;
        }
    }

    public class ColumnLock
    {
        public int GroupNumber { get; set; }
        public int LockedColumn { get; set; }
        public HashSet<int> LockedNumbers { get; set; } = new HashSet<int>();

        public ColumnLock(int GroupNumber)
        {
            this.GroupNumber = GroupNumber;
        }
        
        public ColumnLock(int GroupNumber, int LockedColumn, HashSet<int> LockedNumbers)
        {
            this.GroupNumber = GroupNumber;
            this.LockedColumn = LockedColumn;
            this.LockedNumbers = LockedNumbers;
        }
    }
}
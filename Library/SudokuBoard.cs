using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace sudoku.solver
{
    class SudokuBoard
    {
        public static int NumRows = 9;
        public static int NumColumns = 9;

        public static HashSet<int> NumberSet = new HashSet<int>(){0,1,2,3,4,5,6,7,8,9};

        // This tracks the squares on the sudoku board
        private List<List<Tile>> Board = new List<List<Tile>>();

        // This groups the tiles and is updated every time Board is updated
        public Dictionary<int, List<Tile>> TileBoxes = new Dictionary<int, List<Tile>>();

        /**************************************/
        /************ CONSTRUCTORS ************/
        /**************************************/
        public SudokuBoard()
        {
            this.InitializeEmptyBoard();
        }

        public SudokuBoard(List<Tile> tiles)
        {
            this.InitializeEmptyBoard();

            foreach (Tile tile in tiles)
            {
                this.PlayTile(tile);
            }
        }

        /***************************************/
        /************ TILE MUTATORS ************/
        /***************************************/
        public void PlayTile(Tile tile)
        {
            UpdateTile(tile);
            UpdateTileBoxes();
        }

        public void PlayTile(int value, int row, int column)
        {
            UpdateTile(new Tile(value, row, column));
            UpdateTileBoxes();
        }

        public void RemoveTile(int row, int column)
        {
            UpdateTile(new Tile(0, row, column));
            UpdateTileBoxes();
        }

        /*****************************************/
        /************ BOARD ACCESSORS ************/
        /*****************************************/

        /************ TILE INFO ************/
        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public List<Tile> GetTilesWithNumber(int number)
        {
            List<Tile> tiles = new List<Tile>();

            foreach (List<Tile> row in this.Board)
            {
                tiles.AddRange(row.Where(tile => tile.Value == number));
            }

            return tiles;
        }

        public List<Tile> GetPlays(int number)
        {
            List<Tile> returnTiles = new List<Tile>();
            Dictionary<int, List<Tile>> potentialTiles = GetPotentialMoves(number);

            List<List<Tile>> definitePlays = potentialTiles.Where(dict => dict.Value.Count == 1)
                .ToList()
                .Select(dict => dict.Value)
                .ToList();

            returnTiles.AddRange(Helper.Flatten2dList<Tile>(definitePlays));
            return returnTiles;
        }

        public Dictionary<int, List<Tile>> GetPotentialMoves(int number)
        {
            Dictionary<int, List<Tile>> potentialTiles = new Dictionary<int, List<Tile>>();
            HashSet<int> includeRows = GetRowsWithoutNumber(number);
            HashSet<int> includeCols = GetColumnsWithoutNumber(number);
            HashSet<int> includeGroups = GetGroupsWithoutNumber(number);

            foreach (Tile tile in this.GetFlattenedBoard())
            {
                if (tile.Value != 0)
                {
                    continue;
                }
                // Check if the tile matches ALL of the inclusion criteria
                else if (includeRows.Contains(tile.Row) 
                    && includeCols.Contains(tile.Column) 
                    && includeGroups.Contains(tile.GroupNumber))
                {
                    if (!potentialTiles.ContainsKey(tile.GroupNumber))
                        potentialTiles[tile.GroupNumber] = new List<Tile>();
                    
                    potentialTiles[tile.GroupNumber].Add(new Tile(number, tile.Row, tile.Column));
                }
            }

            return potentialTiles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Tile> GetSingleOptionTiles()
        {
            List<Tile> emptyTiles = GetTilesWithNumber(0);
            List<Tile> playTiles = new List<Tile>();

            // Loop through all the empty tiles and see if they have only one possible value 
            foreach (Tile tile in emptyTiles)
            {
                HashSet<int> availableNumbers = this.GetPossibleTileValues(tile);

                // If there is only one possible value, we add it to the list of tiles to return and be played
                if (availableNumbers.Count == 1)
                {
                    int setTileValue = availableNumbers.First();

                    if (setTileValue != 0)
                        playTiles.Add(new Tile(setTileValue, tile.Row, tile.Column));
                } 
            }
            
            return playTiles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Tile> GetAllTileOptions()
        {
            List<Tile> emptyTiles = GetTilesWithNumber(0);
            List<Tile> tileOptions = new List<Tile>();

            // Loop through all the empty tiles and see if they have only one possible value 
            foreach (Tile tile in emptyTiles)
            {
                HashSet<int> availableNumbers = this.GetPossibleTileValues(tile);

                foreach (int num in availableNumbers)
                {
                    tileOptions.Add(new Tile(num, tile.Row, tile.Column));
                }
            }
            
            return tileOptions;
        }

        public HashSet<int> GetPossibleTileValues(Tile tile)
        {
            // Get all the values that intersect/collide with this tile
            HashSet<int> rowValues = this.Board[tile.Row].Select(tile => tile.Value).ToHashSet();
            HashSet<int> colValues = GetTilesInColumn(tile.Column).Select(tile => tile.Value).ToHashSet();
            HashSet<int> groupValues = GetGroup(tile.GroupNumber).Select(tile => tile.Value).ToHashSet();

            HashSet<int> remaining = NumberSet;
            remaining = remaining.Except(rowValues).ToHashSet();
            remaining = remaining.Except(colValues).ToHashSet();
            remaining = remaining.Except(groupValues).ToHashSet();

            return remaining;
        }

        /************ GROUP INFO ************/
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, List<Tile>> GetTileBoxes()
        {
            Dictionary<int, List<Tile>> TileBoxes = new Dictionary<int, List<Tile>>();
            List<Tile> flatBoard = this.GetFlattenedBoard();
            
            for (int i = 0; i < this.Board.Count; i++)
            {
                TileBoxes[i] = new List<Tile>();
                TileBoxes[i] = flatBoard.Where(tile => tile.GroupNumber == i).ToList();
            }

            return TileBoxes;
        }

        public List<Tile> GetGroup(int groupNumber)
        {
            return this.GetTileBoxes()[groupNumber];
        }

        public List<Tile> GetVerticalGroupsWithOffset(int offset = 0)
        {
            List<Tile> tiles = new List<Tile>();
            tiles.AddRange(this.TileBoxes[0 + offset]);
            tiles.AddRange(this.TileBoxes[3 + offset]);
            tiles.AddRange(this.TileBoxes[6 + offset]);

            return tiles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public HashSet<int> GetGroupsWithoutNumber(int number)
        {
            HashSet<int> groupsWithoutNumber = new HashSet<int>();
            var groups = this.GetTileBoxes();

            foreach (KeyValuePair<int, List<Tile>> kvp in groups)
            {
                bool hasTileNumber = kvp.Value.Any(tile => tile.Value == number);

                if (!hasTileNumber)
                    groupsWithoutNumber.Add(kvp.Key);
            }

            return groupsWithoutNumber;
        }

        public HashSet<int> GetGroupsWithNumber(int number)
        {
            HashSet<int> groupsWithNumber = new HashSet<int>();
            var groups = this.GetTileBoxes();

            foreach (KeyValuePair<int, List<Tile>> kvp in groups)
            {
                bool hasTileNumber = kvp.Value.Any(tile => tile.Value == number);

                if (hasTileNumber)
                    groupsWithNumber.Add(kvp.Key);
            }

            return groupsWithNumber;
        }

        public List<RowLock> GetGroupRowLocks()
        {
            List<RowLock> locks = new List<RowLock>();
            List<Tile> allOptions = this.GetAllTileOptions();
            int[] groupNums = this.TileBoxes.Keys.ToArray();
            
            // Loop through all tiles in all groups
            foreach (int groupNum in groupNums)
            {
                Dictionary<int, RowLock> mappedLocks = new Dictionary<int, RowLock>();
                RowLock lockedRow = new RowLock(groupNum);

                List<Tile> groupTileOptions = allOptions.Where(tile => tile.GroupNumber == groupNum).ToList();
                
                foreach (Tile currTile in groupTileOptions)
                {
                    // Check if the tile's value/number could be in any other row of the group
                    List<int> alternateRows = groupTileOptions
                        .Where(tile => tile.Value == currTile.Value && tile.Row != currTile.Row)
                        .Select(tile => tile.Row)
                        .ToList();

                    // If the value from the tile can only go in this row, that means it is a row lock
                    if (alternateRows.Count == 0)
                    {
                        if (!mappedLocks.ContainsKey(currTile.Row))
                            mappedLocks[currTile.Row] = new RowLock(groupNum);
                        
                        mappedLocks[currTile.Row].LockedNumbers.Add(currTile.Value);
                    }
                        
                }

                locks.AddRange(mappedLocks.Values);
            }

            return locks;
        }

        public List<ColumnLock> GetGroupColumnLocks()
        {
            List<ColumnLock> locks = new List<ColumnLock>();
            List<Tile> allOptions = this.GetAllTileOptions();
            int[] groupNums = this.TileBoxes.Keys.ToArray();
            
            // Loop through all tiles in all groups
            foreach (int groupNum in groupNums)
            {
                Dictionary<int, ColumnLock> mappedLocks = new Dictionary<int, ColumnLock>();
                ColumnLock lockedColumn = new ColumnLock(groupNum);

                List<Tile> groupTileOptions = allOptions.Where(tile => tile.GroupNumber == groupNum).ToList();
                
                foreach (Tile currTile in groupTileOptions)
                {
                    // Check if the tile's value/number could be in any other column of the group
                    List<int> alternateColumns = groupTileOptions
                        .Where(tile => tile.Value == currTile.Value && tile.Column != currTile.Column)
                        .Select(tile => tile.Column)
                        .ToList();

                    // If the value from the tile can only go in this column, that means it is a column lock
                    if (alternateColumns.Count == 0)
                    {
                        if (!mappedLocks.ContainsKey(currTile.Column))
                            mappedLocks[currTile.Column] = new ColumnLock(groupNum);
                        
                        mappedLocks[currTile.Column].LockedNumbers.Add(currTile.Value);
                    }
                        
                }

                locks.AddRange(mappedLocks.Values);
            }

            return locks;
        }

        // public Dictionary<int, HashSet<int>> GetGroupColumnLocks()
        // {
        //     Dictionary<int, HashSet<int>> columnLocks = new Dictionary<int, HashSet<int>>();
        //     List<Tile> allOptions = this.GetAllTileOptions();
        //     int[] groupNums = this.TileBoxes.Keys.ToArray();
            
        //     // Loop through all tiles in all groups
        //     foreach (int groupNum in groupNums)
        //     {
        //         List<Tile> groupTileOptions = allOptions.Where(tile => tile.GroupNumber == groupNum).ToList();
                
        //         foreach (Tile currTile in groupTileOptions)
        //         {
        //             // Check if the tile's value/number could be in any other column of the group
        //             List<int> alternateColumns = groupTileOptions
        //                 .Where(tile => tile.Value == currTile.Value && tile.Column != currTile.Column)
        //                 .Select(tile => tile.Column)
        //                 .ToList();

        //             // If the value from the tile can only go in this column, that means it is a column lock
        //             if (alternateColumns.Count == 0)
        //             {
        //                 if (!columnLocks.ContainsKey(currTile.Column))
        //                     columnLocks[currTile.Column] = new HashSet<int>();
                        
        //                 columnLocks[currTile.Column].Add(currTile.Value);
        //             }
                        
        //         }
        //     }

        //     return columnLocks;
        // }

        /************ ROW INFO ************/
        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public HashSet<int> GetRowsWithNumber(int number)
        {
            HashSet<int> filteredRowNumbers = new HashSet<int>();

            for (int i = 0; i < NumRows; i++)
            {
                List<Tile> row = this.Board[i];
                bool hasTileNumber = row.Any(tile => tile.Value == number);

                if (hasTileNumber)
                    filteredRowNumbers.Add(i);
            }

            return filteredRowNumbers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public HashSet<int> GetRowsWithoutNumber(int number)
        {
            HashSet<int> filteredRowNumbers = new HashSet<int>();
            HashSet<int> rowsWithNumber = GetRowsWithNumber(number);

            for (int i = 0; i < NumRows; i++)
            {
                if (!rowsWithNumber.Contains(i))
                    filteredRowNumbers.Add(i);
            }

            return filteredRowNumbers;
        }

        public List<Tile> GetRow(int rowNumber)
        {
            return this.Board[rowNumber];
        }

        // public List<Tile> GetMissingNumbersFromRow(int rowNumber)
        // {
        //     HashSet<int> missingNumbers = new HashSet<int>(){1,2,3,4,5,6,7,8,9};

        //     List<Tile> row = this.Board[rowNumber];
            
        //     foreach (Tile tile in row)
        //         missingNumbers.Remove(tile.Value);
        // }

        /************ COLUMN INFO ************/

        public HashSet<int> GetColumnsWithNumber(int number)
        {
            HashSet<int> columnsWithNumber = new HashSet<int>();

            for (int i = 0; i < NumColumns; i++)
            {
                List<Tile> col = this.GetTilesInColumn(i);

                bool hasTileNumber = col.Any(tile => tile.Value == number);

                if (hasTileNumber)
                    columnsWithNumber.Add(i);
            }

            return columnsWithNumber;
        }

        public HashSet<int> GetColumnsWithoutNumber(int number)
        {
            HashSet<int> columnsWithNumber = GetColumnsWithNumber(number);
            HashSet<int> columnsWithoutNumber = new HashSet<int>();

            for (int i = 0; i < NumColumns; i++)
            {
                if (!columnsWithNumber.Contains(i))
                    columnsWithoutNumber.Add(i);
            }

            return columnsWithoutNumber;
        }

        public List<Tile> GetTilesInColumn(int colNumber)
        {
            List<Tile> tiles = new List<Tile>();
            for (int i = 0; i < NumRows; i++)
            {
                tiles.Add(this.Board[i][colNumber]);
            }

            return tiles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        public bool CanPlayTile(Tile tile)
        {
            bool canPlay = true;
            int row = tile.Row;
            int col = tile.Column;
            int val = tile.Value;
            int group = tile.GroupNumber;

            // Can't play if there is already a number in the tile
            if (this.Board[row][col].Value != 0)
                return false;
            else if (this.Board[row][col].IsFinal)
                return false;

            // Check if the number already exists in the same column or row
            for (int i = 0; i < NumColumns; i++)
            {
                if (this.Board[row][i].Value == val)
                    return false;
                
                if (this.Board[col][i].Value == val)
                    return false;
            }

            // Check if there is already a tile in the same group with the same number
            if (this.TileBoxes[group].Any(tile => tile.Value == val))
                return false;
            

            return canPlay;
        }

        public bool HasEmptyTile()
        {
            foreach (List<Tile> row in this.Board)
            {
                foreach (Tile tile in row)
                    if (tile.Value == 0)
                        return true;
            }

            return false;
        }
        /***************************************/
        /************ BOARD VISUALS ************/
        /***************************************/
        public void PrintBoard()
        {
            Console.WriteLine(GetBoardStringBuilder().ToString());
        }

        public StringBuilder GetBoardStringBuilder()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < this.Board.Count; i++)
            {
                List<Tile> row = this.Board[i];
                int iMod3 = (i + 1) % 3;
                int iDiv3 = (i + 1) / 3;

                for (int k = 0; k < row.Count; k++)
                {
                    Tile t = row[k];
                    int kMod3 = (k + 1) % 3; 

                    sb.Append(t.ToString());

                    // Add some spacing to visually separate tile groups
                    if (kMod3 == 0 && k + 1 != 9)
                        sb.Append("|");
                }

                sb.Append("\n");

                if (iMod3 == 0)
                    sb.Append("-----------------------------\n");
            }

            return sb;
        }
        /*******************************************/
        /************ PRIVATE FUNCTIONS ************/
        /*******************************************/
        private void InitializeEmptyBoard()
        {
            // Initialize the board
            for (int i = 0; i < NumRows; i++)
            {
                List<Tile> row = new List<Tile>();

                for (int k = 0; k < NumColumns; k++)
                {
                    row.Add(new Tile(0, i, k));
                }

                this.Board.Add(row);
            }

            UpdateTileBoxes();
        }

        private void UpdateTile(Tile tile)
        {
            Helper.PrintJson(tile, false);
            this.Board[tile.Row][tile.Column] = tile;
        }

        private void UpdateTileBoxes()
        {
            this.TileBoxes = GetTileBoxes();
        }

        private List<Tile> GetFlattenedBoard()
        {
            List<Tile> flattened = new List<Tile>();

            foreach (List<Tile> row in this.Board)
            {
                flattened.AddRange(row);
            }

            return flattened;
        }
    }
}
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace sudoku.solver
{
    /// <summary>
    /// FUNCTION NAME CONVENTIONS: 
    /// - "Candidate" means *potentially* accurate tiles 
    /// - "Certain"/"Certainties" means the tiles are 100% correct
    /// NOTE: Largely basing these on what I've learned here: 
    /// https://www.conceptispuzzles.com/index.aspx?uri=puzzle/sudoku/techniques
    /// </summary>
    public class SudokuBoard
    {
        // For logging tile choices
        public Logger? Log { get; set; }
        public List<Tile> ActionLog = new List<Tile>();
        public static int NumRows = 9;
        public static int NumColumns = 9;
        public static HashSet<int> NumberSet = new HashSet<int>(){0,1,2,3,4,5,6,7,8,9};

        // This tracks the squares on the sudoku board
        private List<List<Tile>> Board = new List<List<Tile>>();

        // This groups the tiles and is updated every time Board is updated
        public Dictionary<int, List<Tile>> TileGroups = new Dictionary<int, List<Tile>>();


        /**************************************/
        /************ CONSTRUCTORS ************/
        /**************************************/
        public SudokuBoard()
        {
            this.InitializeEmptyBoard();
        }

        public SudokuBoard(Logger Log)
        {
            this.Log = Log;
            this.InitializeEmptyBoard();
        }

        public SudokuBoard(List<Tile> tiles, Logger Log=null)
        {
            this.Log = Log;
            this.InitializeEmptyBoard();

            foreach (Tile tile in tiles)
            {
                this.PlayTile(tile, false);
            }
        }

        /***************************************/
        /************ TILE MUTATORS ************/
        /***************************************/
        public void PlayTile(Tile tile, bool logAction=true)
        {
            UpdateTile(tile, false);
            UpdateTileBoxes();
        }

        public void PlayTile(int value, int row, int column)
        {
            UpdateTile(new Tile(value, row, column));
            UpdateTileBoxes();
        }

        public List<Tile> PlayTiles(List<Tile> tiles)
        {
            foreach (Tile tile in tiles)
            {
                PlayTile(tile);
            }

            return tiles;
        }

        public void RemoveTile(int row, int column)
        {
            UpdateTile(new Tile(0, row, column));
            UpdateTileBoxes();
        }

        /*****************************************/
        /************ BOARD ACCESSORS ************/
        /*****************************************/

        /************ TILE SEARCH FUNCTIONS ************/
        
        public List<Tile> GetBidirectionalCertainties()
        {
            List<Tile> returnTiles = new List<Tile>();

            for (int i = 1; i <= NumRows; i++)
            {
                Dictionary<int, List<Tile>> potentialTiles = GetGroupedCandidatesForValue(i);

                List<List<Tile>> definitePlays = potentialTiles.Where(dict => dict.Value.Count == 1)
                    .ToList()
                    .Select(dict => dict.Value)
                    .ToList();

                returnTiles.AddRange(Helper.Flatten2dList<Tile>(definitePlays));
            }
            
            return returnTiles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Tile> FindSingleCertainties()
        {
            List<Tile> emptyTiles = GetTilesContainingValue(0);
            List<Tile> playTiles = new List<Tile>();

            // Loop through all the empty tiles and see if they have only one possible value 
            foreach (Tile tile in emptyTiles)
            {
                HashSet<int> availableNumbers = this.GetTileCandidates(tile);

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
        public List<Tile> GetRowCertainties()
        {
            List<Tile> playableTiles = new List<Tile>();

            for (int rowNumber = 0; rowNumber < NumRows; rowNumber++)
            {
                List<Tile> emptyTiles = GetEmptyTilesFromRow(rowNumber);
                Dictionary<int, List<Tile>> tileOptions = new Dictionary<int, List<Tile>>();

                foreach (Tile tile in emptyTiles)
                {
                    var possibleNumbersInTile = GetTileCandidates(tile);

                    foreach (int number in possibleNumbersInTile)
                    {
                        if (!tileOptions.ContainsKey(number))
                            tileOptions[number] = new List<Tile>();
                        
                        tileOptions[number].Add(tile);
                    }
                }

                // If a number is only playable in one tile, we should play it
                foreach (KeyValuePair<int, List<Tile>> kvp in tileOptions)
                {
                    if (kvp.Value.Count == 1)
                    {
                        Tile temp = kvp.Value.First();
                        playableTiles.Add(new Tile(kvp.Key, temp.Row, temp.Column));
                    }
                }
            }
            
            return playableTiles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Tile> GetAllTileCertainties()
        {
            List<Tile> emptyTiles = GetTilesContainingValue(0);
            List<Tile> tileOptions = new List<Tile>();

            // Loop through all the empty tiles and see if they have only one possible value 
            foreach (Tile tile in emptyTiles)
            {
                HashSet<int> availableNumbers = this.GetTileCandidates(tile);

                foreach (int num in availableNumbers)
                {
                    tileOptions.Add(new Tile(num, tile.Row, tile.Column));
                }
            }
            
            return tileOptions;
        }

        /************ BASIC CANDIDATE SEARCHING ************/
        public Dictionary<int, List<Tile>> GetGroupedCandidatesForValue(int number)
        {
            Dictionary<int, List<Tile>> potentialTiles = new Dictionary<int, List<Tile>>();

            foreach (Tile tile in GetCandidatesForValue(number))
            {
                if (!potentialTiles.ContainsKey(tile.GroupNumber))
                    potentialTiles[tile.GroupNumber] = new List<Tile>();
                
                potentialTiles[tile.GroupNumber].Add(new Tile(number, tile.Row, tile.Column));
            }

            return potentialTiles;
        }

        public List<Tile> GetCandidatesForValue(int number)
        {
            List<Tile> potentialTiles = new List<Tile>();
            HashSet<int> includeRows = GetRowsWithoutValue(number);
            HashSet<int> includeCols = GetColumnsWithoutValue(number);
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
                    potentialTiles.Add(new Tile(number, tile.Row, tile.Column));
                }
            }

            return potentialTiles;
        }

        public HashSet<int> GetTileCandidates(Tile tile)
        {
            // If the tile already has a value, don't waste time looking for candidates
            if (tile.Value != 0)
                return new HashSet<int>();

            // Get all the values that intersect/collide with this tile
            HashSet<int> rowValues = GetRow(tile.Row).Select(tile => tile.Value).ToHashSet();
            HashSet<int> colValues = GetColumn(tile.Column).Select(tile => tile.Value).ToHashSet();
            HashSet<int> groupValues = GetGroup(tile.GroupNumber).Select(tile => tile.Value).ToHashSet();

            HashSet<int> remaining = NumberSet;
            remaining = remaining.Except(rowValues).ToHashSet();
            remaining = remaining.Except(colValues).ToHashSet();
            remaining = remaining.Except(groupValues).ToHashSet();

            return remaining;
        }

        // public List<List<Tile>> GetCandidatesBoard()
        // {
        //     List<List<Tile>> allCandidates = new List<List<Tile>>();

        //     foreach (List<Tile> row in this.Board)
        //     {
        //         List<Tile> rowCandidates = new List<Tile>();

        //         foreach (Tile tile in row)
        //         {
        //             var tempCand = this.GetTileCandidates(tile);
        //             rowCandidates.AddRange(tempCand.Select(tileCand => new Tile(tileCand.Row, tileCand.)))
        //         }
        //         var temp = this.Get
        //         foreach
        //     }
        // }

        /****** ADVANCED SEARCHES ******/
        // FIXME:
        public List<Tile> GetGroupNakedPairs()
        {
            List<Tile> Candidates = new List<Tile>();

            foreach (KeyValuePair<int, List<Tile>> group in this.TileGroups)
            {
                int groupNum = group.Key;

                List<Tile> emptyTiles = group.Value.Where(tile => tile.Value == 0).ToList();

                foreach (Tile tile in emptyTiles)
                {
                    //Tile candidate = new Tile(tile.Row, tile.Column);
                    //candidate.PossibleValues.AddRange(this.GetTileCandidates(tile).ToList());
                }
            }

            return Candidates;
        }

        /// <summary>
        /// Resources:
        /// - https://www.sudokuonline.io/tips/sudoku-x-wing
        /// - https://www.sudokuwiki.org/X_Wing_Strategy
        /// </summary>
        /// <returns></returns>
        public List<Tile> FindXWings(int number)
        {
            //this.SetAllTileCandidates();

            List<Tile> xwingCandidates = new List<Tile>();
            List<Tile> allCandidates = this.GetCandidatesForValue(number);
            XWing xwing = new XWing(number);
            
            foreach (Tile tile in allCandidates)
            {
                // Group up row matches
                if (!xwing.RowMatches.ContainsKey(tile.Row))
                    xwing.RowMatches[tile.Row] = new List<Tile>();
                
                xwing.RowMatches[tile.Row].Add(tile);

                // Group up column matches
                if (!xwing.ColumnMatches.ContainsKey(tile.Column))
                    xwing.ColumnMatches[tile.Column] = new List<Tile>();
                
                xwing.ColumnMatches[tile.Column].Add(tile);
            }

            foreach (List<Tile> tiles in xwing.RowMatches.Values)
            {
                // if (tiles.Count)
                // foreach (Tile tile in tiles)
                // int beginSearchAtRow = tile. % 3
                // int rowDifference = 
            }
            
            //List<Tile> all = this.GetFlattenedTiles(valCandidates);

            Helper.PrintJson(xwing);
            return xwingCandidates;
        }

        // TODO: Write function to get a hint for a single square so we can use it when actually playing
        
        /************ TILE INFO ************/
        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public List<Tile> GetTilesContainingValue(int number)
        {
            List<Tile> tiles = new List<Tile>();

            foreach (List<Tile> row in this.Board)
            {
                tiles.AddRange(row.Where(tile => tile.Value == number));
            }

            return tiles;
        }

        

        /************ GROUP INFO ************/
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, List<Tile>> GetTileGroups()
        {
            Dictionary<int, List<Tile>> TileGroups = new Dictionary<int, List<Tile>>();
            List<Tile> flatBoard = this.GetFlattenedBoard();
            
            for (int i = 0; i < this.Board.Count; i++)
            {
                TileGroups[i] = new List<Tile>();
                TileGroups[i] = flatBoard.Where(tile => tile.GroupNumber == i).ToList();
            }

            return TileGroups;
        }

        public List<Tile> GetGroup(int groupNumber)
        {
            return this.GetTileGroups()[groupNumber];
        }

        public List<Tile> GetVerticalGroupsWithOffset(int offset = 0)
        {
            List<Tile> tiles = new List<Tile>();
            tiles.AddRange(this.TileGroups[0 + offset]);
            tiles.AddRange(this.TileGroups[3 + offset]);
            tiles.AddRange(this.TileGroups[6 + offset]);

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
            var groups = this.GetTileGroups();

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
            var groups = this.GetTileGroups();

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
            List<Tile> allOptions = this.GetAllTileCertainties();
            int[] groupNums = this.TileGroups.Keys.ToArray();
            
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
            List<Tile> allOptions = this.GetAllTileCertainties();
            int[] groupNums = this.TileGroups.Keys.ToArray();
            
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
        public HashSet<int> GetRowsWithoutValue(int number)
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

        public List<Tile> GetTilesFromRow(int rowNumber, int value)
        {
            return GetRow(rowNumber).Where(tile => tile.Value == value).ToList();
        }

        public List<Tile> GetEmptyTilesFromRow(int rowNumber)
        {
            return GetRow(rowNumber).Where(tile => tile.Value == 0).ToList();
        }

        // public List<Tile> GetMissingNumbersFromRow(int rowNumber)
        // {
        //     HashSet<int> missingNumbers = new HashSet<int>(){1,2,3,4,5,6,7,8,9};

        //     List<Tile> row = this.Board[rowNumber];
            
        //     foreach (Tile tile in row)
        //         missingNumbers.Remove(tile.Value);
        // }

        /************ COLUMN INFO ************/

        public HashSet<int> GetColumnsContainingValue(int number)
        {
            HashSet<int> columnsWithNumber = new HashSet<int>();

            for (int i = 0; i < NumColumns; i++)
            {
                List<Tile> col = this.GetColumn(i);

                bool hasTileNumber = col.Any(tile => tile.Value == number);

                if (hasTileNumber)
                    columnsWithNumber.Add(i);
            }

            return columnsWithNumber;
        }

        public HashSet<int> GetColumnsWithoutValue(int number)
        {
            HashSet<int> columnsWithNumber = GetColumnsContainingValue(number);
            HashSet<int> columnsWithoutNumber = new HashSet<int>();

            for (int i = 0; i < NumColumns; i++)
            {
                if (!columnsWithNumber.Contains(i))
                    columnsWithoutNumber.Add(i);
            }

            return columnsWithoutNumber;
        }

        public List<Tile> GetColumn(int colNumber)
        {
            List<Tile> tiles = new List<Tile>();
            for (int i = 0; i < NumRows; i++)
            {
                tiles.Add(this.Board[i][colNumber]);
            }

            return tiles;
        }

        public List<Tile> GetTilesInColumn(int colNumber, int tileValue)
        {
            List<Tile> tiles = new List<Tile>();
            for (int i = 0; i < NumRows; i++)
            {
                if (this.Board[i][colNumber].Value == tileValue)
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
            if (this.TileGroups[group].Any(tile => tile.Value == val))
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
            StringBuilder sb = new StringBuilder("    0  1  2 | 3  4  5 | 6  7  8\n");
            sb.AppendLine("   ----------------------------");
            
            for (int i = 0; i < this.Board.Count; i++)
            {
                sb.Append($"{i} |");
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

                sb.AppendLine();

                if (iMod3 == 0)
                    sb.AppendLine("   ----------------------------");
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

        private void UpdateTile(Tile tile, bool logAction=true)
        {
            //Helper.PrintJson(tile, false);
            this.Board[tile.Row][tile.Column] = tile;
            this.UpdateTileBoxes();
            this.SetAllTileCandidates();
        }

        private void UpdateTileBoxes()
        {
            this.TileGroups = GetTileGroups();
        }

        public void SetAllTileCandidates()
        {
            foreach (List<Tile> row in this.Board)
            {
                foreach (Tile tile in row)
                {
                    HashSet<int> candidateNums = this.GetTileCandidates(tile);
                    tile.Candidates = new HashSet<Tile>();

                    foreach (int num in candidateNums)
                        tile.AddCandidate(num);// Candidates.Add(new Tile(tile, num));
                }
            }
        }
        // public void SetAllTileCandidates()
        // {
        //     foreach (List<Tile> row in this.Board)
        //     {
        //         foreach (Tile tile in row)
        //         {
        //             HashSet<int> candidateNums = this.GetTileCandidates(tile);
        //             tile.Candidates = new HashSet<Candidate>();

        //             foreach (int num in candidateNums)
        //                 tile.Candidates.Add(new Candidate(tile, num));
        //         }
        //     }
        // }

        public void SetTileCandidates(ref Tile tile)
        {
            HashSet<int> candidateNumbers = GetTileCandidates(tile);
            tile.AddCandidates(candidateNumbers);
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

        private List<Tile> GetFlattenedTiles(Dictionary<int, List<Tile>> dict)
        {
            List<Tile> flattened = new List<Tile>();

            foreach (List<Tile> list in dict.Values)
            {
                flattened.AddRange(list);
            }

            return flattened;
        }
    }
}
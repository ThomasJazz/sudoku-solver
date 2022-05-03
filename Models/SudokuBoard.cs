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
            this.Board[tile.Row][tile.Column] = tile;
            UpdateTileGroups();
        }

        public void PlayTile(int value, int row, int column)
        {
            this.Board[row][column] = new Tile(value, row, column);
            UpdateTileGroups();
        }

        public void RemoveTile(int row, int column)
        {
            this.Board[row][column] = new Tile(0, row, column);
            UpdateTileGroups();
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
            Dictionary<int, List<Tile>> potentialTiles = GetPotentialMoves(number);

            List<List<Tile>> some = potentialTiles.Where(dict => dict.Value.Count == 1)
                .ToList()
                .Select(dict => dict.Value)
                .ToList();

            return Helper.Flatten2dList<Tile>(some);
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
                // Check if the tile matches any of the exclusion criteria
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

        /************ GROUP INFO ************/
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, List<Tile>> GetTileGroups()
        {
            Dictionary<int, List<Tile>> tileGroups = new Dictionary<int, List<Tile>>();
            List<Tile> flatBoard = this.GetFlattenedBoard();
            
            for (int i = 0; i < this.Board.Count; i++)
            {
                tileGroups[i] = new List<Tile>();
                tileGroups[i] = flatBoard.Where(tile => tile.GroupNumber == i).ToList();
            }

            return tileGroups;
        }

        public List<Tile> GetGroup(int groupNumber)
        {
            return this.GetTileGroups()[groupNumber];
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

            UpdateTileGroups();
        }

        private void UpdateTileGroups()
        {
            this.TileGroups = GetTileGroups();
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
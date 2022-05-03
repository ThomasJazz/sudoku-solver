//     0  1  2   3  4  5   6  7  8
//     --TG0-------TG1-------TG2---
// 0 | :  :  : | :  3  1 | :  ~  ~ 
// 1 | ~  :  ~ | :  :  : | #  #  3 
// 2 | 3  :  1 | :  7  : | :  8  2 
//   | --TG3-------TG4-------TG5---
// 3 | ~  :  ~ | 1  4  : | 3  :  % 
// 4 | 1  3  5 | 7  9  6 | 8  2  4 
// 5 | 7  :  : | 3  %  % | 9  :  % 
//   | --TG6-------TG7-------TG8---
// 6 | :  :  : | 8  1  3 | 2  5  ~ 
// 7 | 2  1  3 | ~  6  ~ | 4  :  8 
// 8 | 5  9  8 | 4  2  7 | #  3  # 
//     ----------------------------

// ~ is a potential 9
// # is a potential 1

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;

// See https://aka.ms/new-console-template for more information

namespace sudoku.solver
{
    public class Program
    {
        /****** CONFIG ******/
        private static string BoardName = "hard-sudoku-1.sudoku";
        private static string RelativeConfigPath = Path.Combine(Directory.GetCurrentDirectory(), $"SudokuBoards/{BoardName}");
        private static string WriteNoMovesPath = Path.Combine(Directory.GetCurrentDirectory(), $"SudokuBoards/NoMovesFound/{BoardName}");
        private static string WriteSolutionToPath = Path.Combine(Directory.GetCurrentDirectory(), $"SudokuBoards/Solved/{BoardName}");

        /****** BODY ******/
        static void Main(string[] args)
        {
            SudokuBoard game = new SudokuBoard();
            game = Sudoku.ReadBoard(RelativeConfigPath);

            // Debugging
            //var multiPlays = game.GetMultiPlays(6);
            //var groupsMissing1 = game.GetGroupsWithoutNumber(6);
            //var playTiles = game.GetSingleOptionTiles();

            // Playing the game
            var solved = PlayGame(game);
            
            Sudoku.ExportBoard(solved, WriteSolutionToPath);
        }

        static SudokuBoard PlayGame(SudokuBoard game)
        {
            SudokuBoard playBoard = game;
            int iter = 0;

            while (playBoard.HasEmptyTile())
            {   
                List<Tile> tilesPlayedThisIter = new List<Tile>();

                // Search for specific numbers to play
                for (int searchVal = 1; searchVal <= SudokuBoard.NumRows; searchVal++)
                {
                    Console.WriteLine($"Finding play options for {searchVal}...");
                    var options = game.GetPlays(searchVal);

                    // Make any available moves and sleep for a second
                    foreach (Tile tile in options)
                    {
                        tilesPlayedThisIter.Add(tile);

                        playBoard.PlayTile(tile);
                        Thread.Sleep(250);
                    }
                }

                // Find squares that can only have one specific number placed there
                Console.WriteLine($"Finding single option tiles...");
                var singleOptions = game.GetSingleOptionTiles();

                foreach (Tile tile in singleOptions)
                {
                    playBoard.PlayTile(tile);
                    tilesPlayedThisIter.Add(tile);

                    Thread.Sleep(250);
                }

                iter++;

                Console.WriteLine($"Completed iteration {iter}");
                playBoard.PrintBoard();

                if (tilesPlayedThisIter.Count == 0)
                {
                    //Console.WriteLine($"All current tile options:");
                    //Helper.PrintJson(playBoard.GetAllTileOptions());

                    FindPattern(game);

                    Sudoku.ExportBoard(playBoard, WriteNoMovesPath);
                    throw new NoMovesFoundException($"Can't find a move for any number. Exiting...");
                }
            }

            return playBoard;
        }

        static void FindPattern(SudokuBoard game)
        {
            // List<Tile> allOptions = game.GetAllTileOptions();
            // int group = 6;

            // var groupOptions = allOptions.Where(tile => tile.GroupNumber == group).ToList();
            // Dictionary<int, HashSet<int>> rowLocks = new Dictionary<int, HashSet<int>>();
            // Dictionary<int, HashSet<int>> colLocks = new Dictionary<int, HashSet<int>>();
            
            // foreach (Tile option in groupOptions)
            // {
            //     List<int> alternateRows = groupOptions
            //         .Where(tile => tile.Value == option.Value && tile.Row != option.Row)
            //         .Select(tile => tile.Row)
            //         .ToList();

            //     List<int> alternateColumns = groupOptions
            //         .Where(tile => tile.Value == option.Value && tile.Column != option.Column)
            //         .Select(tile => tile.Column)
            //         .ToList();
                
            //     if (!rowLocks.ContainsKey(option.Row))
            //         rowLocks[option.Row] = new HashSet<int>();
            //     if (!colLocks.ContainsKey(option.Column))
            //         colLocks[option.Column] = new HashSet<int>();
                
            //     if (alternateRows.Count == 0)
            //         rowLocks[option.Row].Add(option.Value);
            //     else if (alternateColumns.Count == 0)
            //         colLocks[option.Column].Add(option.Value);


            // }
            
            // Helper.PrintJson(rowLocks);
            // Helper.PrintJson(colLocks);
            Helper.PrintJson(game.GetGroupRowLocks());
            Helper.PrintJson(game.GetGroupColumnLocks());
            if (true)
                Thread.Sleep(1);
        }
    }
}
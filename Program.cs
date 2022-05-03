using System;
using System.IO;
using System.Threading;
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
                    Sudoku.ExportBoard(playBoard, WriteNoMovesPath);
                    throw new NoMovesFoundException($"Can't find a move for any number. Exiting...");
                }
            }

            return playBoard;
        }
    }
}
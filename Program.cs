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
            SudokuBoard board = new SudokuBoard();
            board = Sudoku.ReadBoard(RelativeConfigPath);

            var solved = PlayGame(board);
            Sudoku.ExportBoard(solved, WriteSolutionToPath);
        }

        static SudokuBoard PlayGame(SudokuBoard board)
        {
            SudokuBoard playBoard = board;
            int iter = 0;

            while (playBoard.HasEmptyTile())
            {   
                List<Tile> tilesPlayedThisIter = new List<Tile>();
                for (int searchVal = 1; searchVal <= SudokuBoard.NumRows; searchVal++)
                {
                    Console.WriteLine($"Finding play options for {searchVal}...");
                    var options = playBoard.GetPlays(searchVal);

                    // Make any available moves and sleep for a second
                    foreach (Tile tile in options)
                    {
                        tilesPlayedThisIter.Add(tile);

                        Console.WriteLine($"\t{JsonConvert.SerializeObject(tile)}");
                        playBoard.PlayTile(tile);
                        Thread.Sleep(500);
                    }
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

        public static void PrintObj(object o, bool indent=true)
        {
            if (indent)
                Console.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
            else
                Console.WriteLine(JsonConvert.SerializeObject(o));
        }
    }
}
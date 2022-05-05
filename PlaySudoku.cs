//     0  1  2   3  4  5   6  7  8
//     --TG0-------TG1-------TG2--
// 0 | :  :  : | :  3  1 | :  ~  ~ 
// 1 | ~  :  ~ | :  :  : | #  #  3 
// 2 | 3  :  1 | :  7  : | :  8  2 
//   | --TG3-------TG4-------TG5--
// 3 | ~  :  ~ | 1  4  : | 3  :  % 
// 4 | 1  3  5 | 7  9  6 | 8  2  4 
// 5 | 7  :  : | 3  %  % | 9  :  % 
//   | --TG6-------TG7-------TG8--
// 6 | :  :  : | 8  1  3 | 2  5  ~ 
// 7 | 2  1  3 | ~  6  ~ | 4  :  8 
// 8 | 5  9  8 | 4  2  7 | #  3  # 
//     ---------------------------
// NOTE: 
// Fails on:
// - hard-sudoku-4
// - hard-sudoku-5
// - hard-sudoku-6
// - curr-sudoku-2 (easy)

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

// See https://aka.ms/new-console-template for more information

namespace sudoku.solver
{
    public class PlaySudoku
    {
        /****** CONFIG ******/
        private static string BoardName = "curr-sudoku-2.sud";
        private static string RelativeConfigPath = Path.Combine(Directory.GetCurrentDirectory(), $"SudokuBoards/{BoardName}");
        private static string WriteNoMovesPath = Path.Combine(Directory.GetCurrentDirectory(), $"SudokuBoards/NoMovesFound/{BoardName}");
        private static string WriteSolutionToPath = Path.Combine(Directory.GetCurrentDirectory(), $"SudokuBoards/Solved/{BoardName}");

        /****** BODY ******/
        static void Main(string[] args)
        {
            Logger Log = new Logger();

            //Log.TestStackTrace();
            
            
            
            
            
            // SudokuBoard game = new SudokuBoard();
            // game = Sudoku.ReadBoard(RelativeConfigPath);

            // // Playing the game
            // try
            // {
            //     var solved = PlayGame(game);
            //     Sudoku.ExportBoard(solved, WriteSolutionToPath);
            // }
            // catch (NoMovesFoundException e)
            // {
            //     var mappedOptions = Helper.MapModelsToListByKey<Tile>(e.FailedBoard.GetAllTileCandidates(), "GroupNumber");
            //     Helper.PrintJson(mappedOptions);

            //     Sudoku.ExportBoards(e.OriginalBoard, e.FailedBoard, WriteNoMovesPath);
            // }
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
                    var options = playBoard.FindBidirectionalCandidates(searchVal);

                    // Make any available moves
                    tilesPlayedThisIter.AddRange(options);
                    playBoard.PlayTiles(options);
                }
                
                // Console.WriteLine($"Finding single option tiles...");
                // Find squares that can only have one specific number placed there
                var singleOptions = playBoard.FindSingleCandidates();
                playBoard.PlayTiles(singleOptions);
                tilesPlayedThisIter.AddRange(singleOptions);

                // Find squares by row where their neighbors cannot play it
                var rowOptions = playBoard.FindRowCandidates();
                playBoard.PlayTiles(rowOptions);
                tilesPlayedThisIter.AddRange(rowOptions);

                iter++;

                Console.WriteLine($"Completed iteration {iter}");
                playBoard.PrintBoard();

                if (tilesPlayedThisIter.Count == 0)
                {   
                    throw new NoMovesFoundException($"Can't find a move for any number. Exiting...", 
                        game, 
                        playBoard);
                }
            }

            return playBoard;
        }
    }
}
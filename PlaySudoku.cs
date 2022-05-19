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
    public static class PlaySudoku
    {
        /****** CONFIG ******/
        // NOTE: Don't use \\ in file paths because it won't work on non-windows systems
        private static string BoardName = "easy-sudoku-1.sud";
        private static string RelativeConfigPath = Path.Combine(Directory.GetCurrentDirectory(), $"SudokuBoards/{BoardName}");
        private static string WriteNoMovesPath = Path.Combine(Directory.GetCurrentDirectory(), $"SudokuBoards/NoMovesFound/{BoardName}");
        private static string WriteSolutionToPath = Path.Combine(Directory.GetCurrentDirectory(), $"SudokuBoards/Solved/{BoardName}");

        //public static Logger Log = new Logger(true);

        /****** BODY ******/
        public static void Main(string[] args)
        {
            Thread.Sleep(4000);
            SudokuBoard game = new SudokuBoard();
            game = Sudoku.ReadBoard(RelativeConfigPath);

            game.PrintBoard();
            game.SetAllTileCandidates();
            //game.FindXWingsForNumber(5);
            //Console.WriteLine();
            // Playing the game
            try
            {
                var solved = PlayGame(game);
                Sudoku.ExportBoard(solved, WriteSolutionToPath);
            }
            catch (NoMovesFoundException e)
            {
                var mappedOptions = Helper.MapModelsToListByKey<Tile>(e.FailedBoard.GetAllTileCertainties(), "GroupNumber");

                Sudoku.ExportBoards(e.OriginalBoard, e.FailedBoard, WriteNoMovesPath);
                //Log.LogError(e.Message, e);
            }
        }

        public static SudokuBoard PlayGame(SudokuBoard originalBoard)
        {
            SudokuBoard playBoard = originalBoard;
            int iter = 0;

            // FYI: Important to play tiles after each group of certainties is found so we don't play duplicate moves
            while (playBoard.HasEmptyTile())
            {   
                //Log.LogInfo($"Scanning board for tile certainties");
                List<Tile> tilesPlayed = new List<Tile>();

                // Perform bidirectional search and make any available moves
                var options = playBoard.GetBidirectionalCertainties();
                tilesPlayed.AddRange(playBoard.PlayTiles(options));
                
                // Find squares that can only have one specific number placed there
                var singleOptions = playBoard.FindSingleCertainties();
                tilesPlayed.AddRange(playBoard.PlayTiles(singleOptions));

                // Find squares by row where their neighbors cannot play it
                var rowOptions = playBoard.GetRowCertainties();
                tilesPlayed.AddRange(playBoard.PlayTiles(rowOptions));

                iter++;

                // Show some information
                Console.WriteLine($"Tiles found in iteration #{iter} ({tilesPlayed.Count}): {JsonConvert.SerializeObject(tilesPlayed)}");

                playBoard.PrintBoard();
                Thread.Sleep(2000);
                if (tilesPlayed.Count == 0)
                {   
                    throw new NoMovesFoundException($"Can't find a move for any number. Exiting...", 
                        originalBoard, 
                        playBoard);
                }
            }

            return playBoard;
        }
    }
}
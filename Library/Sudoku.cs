using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace sudoku.solver
{
    class NoMovesFoundException : Exception
    {
        public SudokuBoard OriginalBoard { get; set; }
        public SudokuBoard FailedBoard { get; set; }
        public NoMovesFoundException()
        {
            FailedBoard = new SudokuBoard();
            OriginalBoard = new SudokuBoard();
        }

        public NoMovesFoundException(string message, SudokuBoard origBoard, SudokuBoard newBoard)
            : base(message)
        {
            OriginalBoard = origBoard;
            FailedBoard = newBoard;
        }

        public NoMovesFoundException(string message, Exception inner)
            : base(message, inner)
        {
            OriginalBoard = new SudokuBoard();
            FailedBoard = new SudokuBoard();
        }
    }

    class Sudoku
    {
        public Sudoku() {}

        public static SudokuBoard ReadBoard(string filePath)
        {
            Console.WriteLine($"Loading board {Path.GetFileName(filePath)}...");

            List<Tile> tiles = new List<Tile>();
            List<string> lines = File.ReadAllLines(filePath).ToList();

            int currRow = 0;
            foreach (string line in lines)
            {
                // If it's a dashed row, we ignore it and don't try to parse anything
                if (line.Contains("-"))
                    continue;

                // Clean special characters and spaces out of the list
                string cleaned = line.Replace("|", "");
                cleaned = cleaned.Replace(":", "0");
                cleaned = cleaned.Replace(" ", "");

                // Parse out the numbers from the row and create new tiles for them
                for (int k = 0; k < cleaned.Length; k++)
                {
                    int tileValue;
                    bool success = Int32.TryParse(cleaned[k].ToString(), out tileValue);

                    if (success)
                        tiles.Add(new Tile(tileValue, currRow, k));
                }

                currRow++;
            }

            var tempBoard = new SudokuBoard(tiles);
            tempBoard.PrintBoard();
            return tempBoard;
        }
    
        public static void ExportBoard(SudokuBoard board, string filePath)
        {
            StringBuilder sb = board.GetBoardStringBuilder();

            using (StreamWriter file = new StreamWriter($"{filePath}"))
            {
                file.WriteLine(sb.ToString());
            }
        }

        public static void ExportBoards(SudokuBoard original, SudokuBoard modified, string filePath)
        {
            StringBuilder sb = new StringBuilder("Original:\n");
            sb.Append(original.GetBoardStringBuilder());
            sb.Append("\nModified:\n");
            sb.Append(modified.GetBoardStringBuilder());
            
            using (StreamWriter file = new StreamWriter($"{filePath}"))
            {
                file.WriteLine(sb.ToString());
            }
        }
    }
}
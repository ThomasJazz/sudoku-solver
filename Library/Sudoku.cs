using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace sudoku.solver
{
    class NoMovesFoundException : Exception
    {
        public NoMovesFoundException(){}

        public NoMovesFoundException(string message)
            : base(message){}

        public NoMovesFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    class Sudoku
    {
        
        public Sudoku(){}

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
                file.WriteLine(sb.ToString()); // "sb" is the StringBuilder
            }
        }
    }
}
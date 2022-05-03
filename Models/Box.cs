using System;
using System.Collections.Generic;

namespace sudoku.solver
{
    class TileBox
    {
        public Dictionary<string, Tile> Tiles = new Dictionary<string, Tile>();

        public TileBox(){}
    }
}
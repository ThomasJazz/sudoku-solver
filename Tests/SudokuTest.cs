using System;
using Xunit;

namespace sudoku.solver.tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Console.WriteLine("test");
            Assert.True(false);
        }

        public void TestTileCandidateHashing()
        {
            Tile tile = new Tile(0, 5, 5);
            tile.AddCandidate(4);
            tile.AddCandidate(5);

            //Console.WriteLine(tile.Candidates.Contains())
        }
    }
}
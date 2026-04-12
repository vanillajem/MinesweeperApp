namespace MinesweeperApp.Models
{
    public class Cell
    {
        // Properties for the Cell class
        public int Row { get; set; }
        public int Col { get; set; }
        // State properties
        public bool IsMine { get; set; }
        public bool IsRevealed { get; set; }
        public bool IsFlagged { get; set; }
        // Property to track the number of adjacent mines
        public int AdjacentMines { get; set; }
        /// <summary>
        /// Parameterized constructor for the Cell class
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        public Cell(int row, int col)
        {
            // Initialize properties
            Row = row;
            Col = col;
            IsMine = false;
            IsRevealed = false;
            IsFlagged = false;
            AdjacentMines = 0;
        }
    }
}

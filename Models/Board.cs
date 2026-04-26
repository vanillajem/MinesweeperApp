using Microsoft.IdentityModel.Tokens;

namespace MinesweeperApp.Models
{
    public class Board
    {
        // Properties for the Board class
        public int Rows { get; set; }
        public int Cols { get; set; }
        public int TotalMines { get; set; }
        // 2D array to represent the grid of cells
        public Cell[][] Grid { get; set; }
        // Properties to track game state
        public bool GameOver { get; set; }
        public bool Win { get; set; }

        /// <summary>
        /// Default constructor for the Board class
        /// </summary>
        public Board()
        {
            // Initialize properties with default values
            Grid = Array.Empty<Cell[]>();
        }

        /// <summary>
        /// Parameterized constructor for Board class
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <param name="mines"></param>
        public Board(int rows, int cols, int mines)
        {
            // Initialize properties
            Rows = rows;
            Cols = cols;
            TotalMines = mines;
            // Initialize the grid 
            Grid = new Cell[rows][];
            for (int r = 0; r < rows; r++)
            {
                Grid[r] = new Cell[cols];
            }
            // Initialize game state
            InitializeBoard();
            PlaceMines();
            CalculateAdjacents();
        }

        /// <summary>
        /// Initializes the game board by creating Cell objects for each position in the grid.
        /// </summary>
        private void InitializeBoard()
        {
            // Loop through each row and column to create Cell objects
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    Grid[r][c] = new Cell(r, c);
                }
            }
        }

        /// <summary>
        /// Places the mines randomly on the board based on the number of mines specified.
        /// </summary>
        private void PlaceMines()
        {
            // Initialize and declare
            Random rand = new Random();
            int placed = 0;

            // Loop until the specified number of mines are placed
            while (placed < TotalMines)
            {
                // Generate random row and column indices
                int r = rand.Next(Rows);
                int c = rand.Next(Cols);
                // Check if the cell at the generated position is not already a mine
                if (!Grid[r][c].IsMine)
                {
                    // If it's not a mine, set it as a mine and increment the placed counter
                    Grid[r][c].IsMine = true;
                    placed++;
                }
            }
        }

        /// <summary>
        /// Calculates the number of adjacent mines for each cell that is not a mine.
        /// </summary>
        private void CalculateAdjacents()
        {
            // Loop through each row in the grid
            for (int r = 0; r < Rows; r++)
            {
                // Loop through each column in the grid
                for (int c = 0; c < Cols; c++)
                {
                    // If the current cell is a mine, skip the calculation for adjacent mines
                    if (Grid[r][c].IsMine) continue;
                    // Initialize a counter to keep track of the number of adjacent mines
                    int count = 0;
                    // Loop through the neighboring cells (including diagonals) to count the number of mines
                    for (int i = -1; i <= 1; i++)
                    {
                        // Loop through the neighboring columns
                        for (int j = -1; j <= 1; j++)
                        {
                            // Calculate the row and column indices of the neighboring cell
                            int nr = r + i;
                            int nc = c + j;
                            // Check if the neighboring cell is within the bounds of the grid
                            if (nr >= 0 && nr < Rows && nc >= 0 && nc < Cols)
                            {
                                // If the neighboring cell is a mine, increment the count
                                if (Grid[nr][nc].IsMine)
                                {
                                    // Increment the count of adjacent mines
                                    count++;
                                }
                            }
                        }
                    }
                    // Set the AdjacentMines property of the current cell to the count of adjacent mines
                    Grid[r][c].AdjacentMines = count;
                }
            }
        }

        /// <summary>
        /// Reveals the cell at the specified row and column in the game grid, updating the game state accordingly.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        public void RevealCell(int row, int col)
        {
            // Check if the specified row and column are within the bounds of the grid
            if (row < 0 || row >= Rows || col < 0 || col >= Cols)
                return;
            // Get the cell at the specified row and column
            Cell cell = Grid[row][col];
            // If the cell is already revealed or flagged, do nothing
            if (cell.IsRevealed || cell.IsFlagged)
                return;
            // Mark the cell as revealed
            cell.IsRevealed = true;
            // If the revealed cell is a mine, set GameOver to true
            if (cell.IsMine)
            {
                GameOver = true;
                return;
            }
            // If no adjacent mines, reveal neighbors
            if (cell.AdjacentMines == 0)
            {
                // Loop through the neighboring rows
                for (int i = -1; i <= 1; i++)
                {
                    // Loop through the neighboring columns
                    for (int j = -1; j <= 1; j++)
                    {
                        // Recursively reveal neighboring cells
                        RevealCell(row + i, col + j);
                    }
                }
            }
            // After revealing a cell, check if the player has won the game
            CheckWin();
        }

        /// <summary>
        /// Determines whether all non-mine cells have been revealed and updates the game state
        /// </summary>
        public void CheckWin()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    Cell cell = Grid[r][c];

                    if (!cell.IsMine && !cell.IsRevealed)
                    {
                        Win = false;
                        return;
                    }
                }
            }

            GameOver = true;
            Win = true;
        }

        /// <summary>
        /// Toggle the flag bool
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        public void ToggleFlag(int row, int col)
        {
            Cell cell = Grid[row][col];

            if (GameOver || cell.IsRevealed)
            {
                return;
            }

            cell.IsFlagged = !cell.IsFlagged;
        }

        /// <summary>
        /// Reveal all neighbors that do not have flags and aren't touching a bomb
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        private void RevealEmptyNeighbors(int row, int col)
        {
            for (int r = row - 1; r <= row + 1; r++)
            {
                for (int c = col - 1; c <= col + 1; c++)
                {
                    if (r < 0 || r >= Rows || c < 0 || c >= Cols)
                    {
                        continue;
                    }

                    Cell neighbor = Grid[r][c];

                    if (!neighbor.IsRevealed && !neighbor.IsMine && !neighbor.IsFlagged)
                    {
                        neighbor.IsRevealed = true;

                        if (neighbor.AdjacentMines == 0)
                        {
                            RevealEmptyNeighbors(r, c);
                        }
                    }
                }
            }
        }
    }
}

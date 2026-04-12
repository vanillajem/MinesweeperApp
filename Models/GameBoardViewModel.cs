namespace MinesweeperApp.Models
{
    // This ViewModel stores all data needed by the Game board view.
    // It keeps the board settings and board cell display data together
    // so the Razor page does not have to rely on multiple ViewBag values.
    public class GameBoardViewModel
    {
        // Size of the board (example: 5 means 5x5)
        public int BoardSize { get; set; }

        // Difficulty selected by the player
        public string Difficulty { get; set; }

        // Holds the display value for each cell on the board
        public List<string> Cells { get; set; }

        // Stores the current board state as a string so it can be posted back
        public string BoardState { get; set; }

        // Tracks which row was last clicked
        public int? ClickedRow { get; set; }

        // Tracks which column was last clicked
        public int? ClickedCol { get; set; }

        // The actual Board object that contains the game logic and state
        public Board Board { get; set; }
        
        // Default constructor
        public GameBoardViewModel()
        {
            Cells = new List<string>();
            Difficulty = string.Empty;
            BoardState = string.Empty;
        }
    }
}

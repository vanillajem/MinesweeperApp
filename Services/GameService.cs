using MinesweeperApp.Models;

namespace MinesweeperApp.Services
{
    public class GameService : IGameService
    {
        // Class level variable
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="context"></param>
        public GameService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get the number of mines to place on the board
        /// </summary>
        /// <param name="boardSize"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public int GetMineCount(int boardSize, string difficulty)
        {
            return difficulty.ToLower() switch
            {
                "easy" => (int)(boardSize * boardSize * 0.1),
                "medium" => (int)(boardSize * boardSize * 0.15),
                "hard" => (int)(boardSize * boardSize * 0.25),
                _ => (int)(boardSize * boardSize * 0.15)
            };
        }

        /// <summary>
        /// Builds a view model to construct the model of the game board
        /// </summary>
        /// <param name="board"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public GameBoardViewModel BuildViewModel(Board board, string difficulty)
        {
            List<string> cells = new List<string>();

            for (int r = 0; r < board.Rows; r++)
            {
                for (int c = 0; c < board.Cols; c++)
                {
                    Cell cell = board.Grid[r][c];

                    if (cell.IsFlagged)
                        cells.Add("🚩");
                    else if (!cell.IsRevealed)
                        cells.Add("■");
                    else if (cell.IsMine)
                        cells.Add("💣");
                    else if (cell.AdjacentMines > 0)
                        cells.Add(cell.AdjacentMines.ToString());
                    else
                        cells.Add("");
                }
            }

            return new GameBoardViewModel
            {
                BoardSize = board.Rows,
                Difficulty = difficulty,
                Cells = cells,
                Board = board
            };
        }

        /// <summary>
        /// Calculates the player's score
        /// </summary>
        /// <param name="board"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public int CalculateScore(Board board, string difficulty)
        {
            int difficultyMultiplier = difficulty.ToLower() switch
            {
                "easy" => 1,
                "medium" => 2,
                "hard" => 3,
                _ => 1
            };

            int revealedCells = 0;

            for (int r = 0; r < board.Rows; r++)
            {
                for (int c = 0; c < board.Cols; c++)
                {
                    Cell cell = board.Grid[r][c];

                    if (cell.IsRevealed && !cell.IsMine)
                    {
                        revealedCells++;
                    }
                }
            }

            return revealedCells * difficultyMultiplier * 10;
        }

        /// <summary>
        /// Saves the game score
        /// </summary>
        /// <param name="username"></param>
        /// <param name="board"></param>
        /// <param name="difficulty"></param>
        public void SaveGameScore(string username, Board board, string difficulty)
        {
            int score = CalculateScore(board, difficulty);

            GameScore gameScore = new GameScore
            {
                Username = username,
                BoardSize = board.Rows,
                Difficulty = difficulty ?? string.Empty,
                Score = score,
                DatePlayed = DateTime.Now
            };

            _context.GameScores.Add(gameScore);
            _context.SaveChanges();
        }
    }
}
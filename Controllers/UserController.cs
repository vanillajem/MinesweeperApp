using Microsoft.AspNetCore.Mvc;
using MinesweeperApp.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Linq;


namespace MinesweeperApp.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor - inject database context
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // REGISTER (GET)
        // =========================
        // Shows the registration form
        [HttpGet]
        public IActionResult Register()
        {
            // Just show the registration page
            return View();
        }

        // =========================
        // REGISTER (POST)
        // =========================
        // Handles registration form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            // Make sure all form rules pass
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if username already exists
            if (_context.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Username already exists.");
                return View(model);
            }

            // Check if email already exists
            if (_context.Users.Any(u => u.EmailAddress == model.EmailAddress))
            {
                ModelState.AddModelError("EmailAddress", "Email address already exists.");
                return View(model);
            }

            // Generate a salt for this new user
            string salt = PasswordHelper.GenerateSalt();

            // Hash the password using the salt
            string hashedPassword = PasswordHelper.HashPassword(model.Password, salt);

            // Convert the view model into a database user model
            UserModel user = new UserModel
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Sex = model.Sex,
                Age = model.Age,
                State = model.State,
                EmailAddress = model.EmailAddress,
                Username = model.Username,
                PasswordHash = hashedPassword,
                Salt = salt
            };

            // Save user to database
            _context.Users.Add(user);
            _context.SaveChanges();

            // Redirect to login page after successful registration
            return RedirectToAction("Login", new { registered = true });

        }

        // =========================
        // LOGIN (GET)
        // =========================
        // Shows login form
        [HttpGet]
        public IActionResult Login(bool registered = false)
        {
            // Show a one-time success message only after registration
            if (registered)
            {
                ViewBag.Message = "Registration successful! You can now log in.";
            }

            return View();
        }

            // =========================
            // LOGIN (POST)
            // =========================
            // Handles login form submission
            [HttpPost]
            public IActionResult Login(LoginViewModel model)
            {
                // Check whether the submitted form is valid
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Find the user by username first
                var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);

                // If no user found, login fails
                if (user == null)
                {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

                // Verify entered password against stored hash + salt
                bool isPasswordValid = PasswordHelper.VerifyPassword(
                    model.Password,
                    user.PasswordHash,
                    user.Salt);

                if (isPasswordValid)
                {
                    // Save logged in username in session
                    HttpContext.Session.SetString("Username", user.Username);

                    // Redirect to restricted page
                    return RedirectToAction("StartGame");
                }

            // Login failed
            ModelState.AddModelError("", "Invalid username or password");
            return View(model);
        }
        // =========================
        // START GAME (PROTECTED PAGE)
        // =========================
        // Only accessible if user is logged in
        public IActionResult StartGame()
        {
            // Check if session has username (means user is logged in)
            string username = HttpContext.Session.GetString("Username");

            // If NOT logged in → redirect to Login
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            // If logged in → allow access
            return View();
        }

        // =========================
        // START NEW GAME (POST)
        // =========================
        // Handles the StartGame form submission.
        // This follows the same MVC form-post idea used in the activities.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartNewGame(int boardSize, string difficulty)
        {
            // Make sure the user is logged in before allowing game setup
            string username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            //=============================================================
            // Added in order to remove old game board to start a new game (Jacob)
            //=============================================================
            HttpContext.Session.Remove("Board");

            // Redirect to the Game page and pass the chosen settings.
            // This is the next step in the Minesweeper flow.
            return RedirectToAction("Game", new { boardSize = boardSize, difficulty = difficulty });
        }

        // =========================
        // GAME PAGE (GET) Jacob
        // =========================

        /// <summary>
        /// Handles GET requests for the game page, intializing the game board based on the selected settings and any existing session data.
        /// </summary>
        /// <param name="boardSize"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Game(int boardSize, string difficulty)
        {
            // Make sure the user is logged in before allowing access to the game
            string username = HttpContext.Session.GetString("Username");
            // If not logged in, redirect to Login page
            if (string.IsNullOrEmpty(username))
            {
                // This is a safety check. In practice, the user should never see this if the flow is followed correctly.
                return RedirectToAction("Login");
            }

            // Try to get existing board from session
            Board board = GetBoardFromSession();

            // If no board exists, create one (first load)
            if (board == null)
            {
                // TODO: Jacob - The GetMineCount method is a placeholder. The actual number of mines should be determined by the difficulty level and board size.
                int mines = GetMineCount(boardSize, difficulty);
                board = new Board(boardSize, boardSize, mines);

                SaveBoardToSession(board);
            }

            GameBoardViewModel model = BuildViewModel(board, difficulty);

            return View(model);
        }

        // =========================
        // HANDLE CELL CLICK (POST) Jacob
        // =========================

        /// <summary>
        /// Processes a cell click event in the game board, updating the game state accordingly and checking for win/loss conditions. This is the core interaction handler for the Minesweeper game.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="boardSize"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HandleCellClick(int row, int col, int boardSize, string difficulty)
        {
            // Make sure the user is logged in before allowing game interactions
            string username = HttpContext.Session.GetString("Username");
            // If not logged in, redirect to Login page
            if (string.IsNullOrEmpty(username))
            {
                // This is a safety check. In practice, the user should never see this if the flow is followed correctly.
                return RedirectToAction("Login");
            }
            // Get the current game board from session
            Board board = GetBoardFromSession();
            // If no board exists in session, redirect back to Game page to initialize it
            if (board == null)
            {
                // This is a safety check. In practice, the user should never see this if the flow is followed correctly.
                return RedirectToAction("Game", new { boardSize, difficulty });
            }
            // Process the cell click on the board
            if (!board.GameOver)
            {
                // The RevealCell method will handle the game logic for revealing the cell, including checking for mines, updating the board state, and determining if the game is over.
                board.RevealCell(row, col);
            }
            // Save the updated board back to session
            SaveBoardToSession(board);

            // WIN / LOSS DETECTION
            if (board.GameOver)
            {
                // Check if the game state is a win
                if (board.Win)
                {
                    // Get the username of the user
                    username = HttpContext.Session.GetString("Username") ?? string.Empty;
                    // Set a score variable to the calculated score
                    int score = CalculateScore(board, difficulty);
                    // Create a new GameScore object with the relevant information and save it to the database.
                    GameScore gameScore = new GameScore
                    {
                        Username = username,
                        BoardSize = board.Rows,
                        Difficulty = difficulty ?? string.Empty,
                        Score = score,
                        DatePlayed = DateTime.Now
                    };
                    // Save the game score to the database
                    _context.GameScores.Add(gameScore);
                    _context.SaveChanges();
                    // Redirect to the Win page and pass the score as a parameter to display it on the win screen.
                    return RedirectToAction("Win", new { score = score });
                }
                else
                {
                    // If the game is over but not a win, it must be a loss. Redirect to the Loss page.
                    return RedirectToAction("Loss");
                }
            }
            // If the game is not over, just reload the game page with the updated board.
            GameBoardViewModel model = BuildViewModel(board, difficulty);
            // The Game view will use the model to render the current state of the board, including revealed cells, remaining mines, etc.
            return View("Game", model);
        }

        // =========================
        // LOGOUT
        // =========================
        public IActionResult Logout()
        {
            // Clear the current session
            HttpContext.Session.Clear();

            // Send the user back to login
            return RedirectToAction("Login");
        }

        // =========================
        // WIN PAGE
        // =========================
        // Displays the win screen.
        // Later, the score can be calculated by the game logic and passed here.
        [HttpGet]
        public IActionResult Win(int? score = null)
        {
            // Make sure the user is logged in
            string username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            ViewBag.Score = score;
            return View();
        }

        // =========================
        // LOSS PAGE
        // =========================
        // Displays the loss screen when the player hits a mine.
        [HttpGet]
        public IActionResult Loss()
        {
            // Make sure the user is logged in
            string username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        /// <summary>
        /// Calculates the number of mines to place on a minesweeper board based on the selected difficulty level and board size. This is a placeholder method and can be adjusted to use more specific rules for mine placement.
        /// </summary>
        /// <param name="boardSize"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        private int GetMineCount(int boardSize, string difficulty)
        {
            // Switch case to determine mine count based on difficulty.
            switch (difficulty.ToLower())
            {
                // In case of "easy", we place mines on 10% of the board.
                case "easy":
                    return (int)(boardSize * boardSize * 0.1);
                // In case of "medium", we place mines on 15% of the board.
                case "medium":
                    return (int)(boardSize * boardSize * 0.15);
                // In case of "hard", we place mines on 25% of the board.
                case "hard":
                    return (int)(boardSize * boardSize * 0.25);
                // As a default, we add mines to 15% of the board.
                default:
                    return (int)(boardSize * boardSize * 0.15);
            }
        }

        /// <summary>
        /// Build the view model for the game board based on the current state of the board and the selected difficulty.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        private GameBoardViewModel BuildViewModel(Board board, string difficulty)
        {
            // A list of cells to be rendered on the game board.
            List<string> cells = new List<string>();
            // Loop through each row in the board's grid
            for (int r = 0; r < board.Rows; r++)
            {
                // For each row, loop through the columns and check the state of each cell.
                for (int c = 0; c < board.Cols; c++)
                {
                    // Get the current cell from the board's grid
                    var cell = board.Grid[r][c];
                    // Determine what to display for this cell based on whether it's revealed, if it's a mine, and how many adjacent mines it has.
                    if (!cell.IsRevealed)
                    {
                        // Add a placeholder for unrevealed cells.
                        cells.Add("■");
                    }
                    else if (cell.IsMine)
                    {
                        // Add a bomb emoji for revealed mines.
                        cells.Add("💣");
                    }
                    else if (cell.AdjacentMines > 0)
                    {
                        // Add the number of adjacent mines for revealed cells that are not mines.
                        cells.Add(cell.AdjacentMines.ToString());
                    }
                    else
                    {
                        // Add an empty string for revealed cells with no adjacent mines.
                        cells.Add("");
                    }
                }
            }
            // Create and return the view model with the current board state and settings.
            return new GameBoardViewModel
            {
                BoardSize = board.Rows,
                Difficulty = difficulty,
                Cells = cells,
                Board = board
            };
        }

        /// <summary>
        /// Save the current game session
        /// </summary>
        /// <param name="board"></param>
        private void SaveBoardToSession(Board board)
        {
            // Serialize the board object to JSON and save it in the session under the key "Board".
            HttpContext.Session.SetString("Board",
                System.Text.Json.JsonSerializer.Serialize(board));
        }

        /// <summary>
        /// Get the board state from the current session.
        /// </summary>
        /// <returns></returns>
        private Board GetBoardFromSession()
        {
            // Retrieve the JSON string for the board from the session.
            var data = HttpContext.Session.GetString("Board");
            // If there is no board data in the session, return null.
            if (string.IsNullOrEmpty(data))
                return null;
            // Deserialize the JSON string back into a Board object and return it.
            return System.Text.Json.JsonSerializer.Deserialize<Board>(data);
        }

        /// <summary>
        /// Calculate the score for the game based on the number of revealed cells, the difficulty level, and the size of the board. This is a simple scoring algorithm that can be adjusted to better fit the desired gameplay experience.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        private int CalculateScore(Board board, string difficulty)
        {
            // Base score is determined by the total number of cells on the board.
            int baseScore = board.Rows * board.Cols;
            // Difficulty multiplier increases the score based on the selected difficulty level.
            int difficultyMultiplier = difficulty.ToLower() switch
            {
                "easy" => 1,
                "medium" => 2,
                "hard" => 3,
                _ => 1
            };
            // Count the number of cells that have been revealed and are not mines. This rewards players for successfully revealing safe cells.
            int revealedCells = 0;
            // Loop through each row
            for (int r = 0; r < board.Rows; r++)
            {
                // Loop through each column in the current row
                for (int c = 0; c < board.Cols; c++)
                {
                    // Get the current cell from the board's grid
                    Cell cell = board.Grid[r][c];
                    // If the cell is revealed and not a mine, increment the count of revealed cells.
                    if (cell.IsRevealed && !cell.IsMine)
                    {
                        revealedCells++;
                    }
                }
            }
            // Return the calculated final score.
            return revealedCells * difficultyMultiplier * 10;
        }
    }
}


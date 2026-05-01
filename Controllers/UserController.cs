using Microsoft.AspNetCore.Mvc;
using MinesweeperApp.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Linq;
using MinesweeperApp.Services;


namespace MinesweeperApp.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IGameService _gameService;

        // Constructor - inject database context
        public UserController(ApplicationDbContext context, IGameService gameService)
        {
            _context = context;
            _gameService = gameService;
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
                int mines = _gameService.GetMineCount(boardSize, difficulty);
                board = new Board(boardSize, boardSize, mines);

                SaveBoardToSession(board);
            }

            GameBoardViewModel model = _gameService.BuildViewModel(board, difficulty);

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
                    int score = _gameService.CalculateScore(board, difficulty);
                    _gameService.SaveGameScore(username, board, difficulty);

                    return RedirectToAction("Win", new { score = score });
                }
                else
                {
                    // If the game is over but not a win, it must be a loss. Redirect to the Loss page.
                    return RedirectToAction("Loss");
                }
            }
            // If the game is not over, just reload the game page with the updated board.
            GameBoardViewModel model = _gameService.BuildViewModel(board, difficulty);
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

        // =========================
        // SAVE GAME PLACEHOLDER (POST) Angela
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveGamePlaceholder(int boardSize, string difficulty)
        {
            string username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            // Added by Angela: this keeps the Save Game button working until Jacob connects the database save logic.
            // TODO Jacob: serialize the current Board from session and save it to the Games table.
            TempData["Milestone4Message"] = "Save Game button clicked. Jacob will connect this to the database/API.";

            return RedirectToAction("Game", new { boardSize, difficulty });
        }

        // =========================
        // SHOW SAVED GAMES PLACEHOLDER (GET) Angela
        // =========================
        [HttpGet]
        public IActionResult ShowSavedGames()
        {
            string username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            // Added by Angela: temporary sample row so the saved-games page layout can be viewed before Jacob adds the API/database.
            // TODO Jacob: replace this list with saved games loaded from the Games table.
            List<SavedGameViewModel> savedGames = new List<SavedGameViewModel>
            {
                new SavedGameViewModel
                {
                    Id = 1,
                    DateSaved = "Sample saved game",
                    BoardSize = "8 x 8",
                    Difficulty = "Medium"
                }
            };

            return View(savedGames);
        }

        // =========================
        // LOAD SAVED GAME PLACEHOLDER (POST) Angela
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LoadSavedGamePlaceholder(int id)
        {
            // Added by Angela: placeholder keeps the Load button from breaking before Jacob adds restore logic.
            // TODO Jacob: load the selected saved game, deserialize the board JSON, and put it back into session.
            TempData["Milestone4Message"] = "Load button clicked. Jacob will connect this to restore saved game data.";
            return RedirectToAction("ShowSavedGames");
        }

        // =========================
        // DELETE SAVED GAME PLACEHOLDER (POST) Angela
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSavedGamePlaceholder(int id)
        {
            // Added by Angela: placeholder keeps the Delete button from breaking before Jacob adds delete logic.
            // TODO Jacob: delete the selected saved game from the Games table or REST API endpoint.
            TempData["Milestone4Message"] = "Delete button clicked. Jacob will connect this to remove saved game data.";
            return RedirectToAction("ShowSavedGames");
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
        /// IActionResult method that toggles the flag
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="boardSize"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AjaxToggleFlag(int row, int col, int boardSize, string difficulty)
        {
            string username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return Json(new { redirectUrl = Url.Action("Login", "User") });
            }

            Board board = GetBoardFromSession();

            if (board == null)
            {
                return Json(new { redirectUrl = Url.Action("Game", "User", new { boardSize, difficulty }) });
            }

            board.ToggleFlag(row, col);

            SaveBoardToSession(board);

            GameBoardViewModel model = _gameService.BuildViewModel(board, difficulty);

            ViewData["Row"] = row;
            ViewData["Col"] = col;

            return PartialView("_CellPartial", model);
        }

        // ================================
        // AJAX CELL CLICK (POST) Angela
        // ================================

        // Added by Angela: handles AJAX left-click updates for one Minesweeper cell.
        // This keeps the page from doing a full reload.
        [HttpPost]
        public IActionResult AjaxCellClick(int row, int col, int boardSize, string difficulty)
        {
            // Make sure the user is logged in before allowing game actions.
            string username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                // Added by Angela: tells JavaScript the user needs to go back to login.
                return Json(new { redirectUrl = Url.Action("Login", "User") });
            }

            // Get the current game board from session.
            Board board = GetBoardFromSession();

            if (board == null)
            {
                // Added by Angela: if session is missing, reload the game page.
                return Json(new { redirectUrl = Url.Action("Game", "User", new { boardSize, difficulty }) });
            }

            // Jacob TODO: later this reveal logic should move into GameService for Milestone 3.
            if (!board.GameOver)
            {
                board.RevealCell(row, col);
            }

            // Save updated board back to session.
            SaveBoardToSession(board);

            // Handle game ending events (Jacob)
            if (board.GameOver)
            {
                if (board.Win)
                {
                    int score = _gameService.CalculateScore(board, difficulty);
                    _gameService.SaveGameScore(username, board, difficulty);

                    return Json(new
                    {
                        redirectUrl = Url.Action("Win", "User", new { score = score })
                    });
                }

                return Json(new
                {
                    redirectUrl = Url.Action("Loss", "User")
                });
            }

            // Build the updated ViewModel so the partial cell has the latest value.
            GameBoardViewModel model = _gameService.BuildViewModel(board, difficulty);

            // Updated by Jacob: return the _BoardPartial to allow for all adjacent empty cells to be revealed after click
            return PartialView("_BoardPartial", model);
        }
    }
}



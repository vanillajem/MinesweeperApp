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

            // Redirect to the Game page and pass the chosen settings.
            // This is the next step in the Minesweeper flow.
            return RedirectToAction("Game", new { boardSize = boardSize, difficulty = difficulty });
        }

        // =========================
        // GAME PAGE (GET) Jacob
        // =========================
        // TODO: Jacob - Replace this temporary board setup with the real Board/Cell logic.
        // This is where the initial Minesweeper board should be created based on
        // the selected board size and difficulty.

        // Displays the minesweeper board using a ViewModel.
        // ButtonGrid idea where the controller
        // sends board data to a Razor view for display.
        [HttpGet]
        public IActionResult Game(int boardSize, string difficulty)
        {
            // Make sure the user is logged in before showing the game board
            string username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            // Build a temporary board filled with X values
            List<string> cells = new List<string>();

            for (int i = 0; i < boardSize * boardSize; i++)
            {
                // A hidden cell is shown with a filled square symbol for now
                cells.Add("■");
            }

            // Create a ViewModel and send it to the view
            GameBoardViewModel model = new GameBoardViewModel
            {
                BoardSize = boardSize,
                Difficulty = difficulty,
                Cells = cells,
                BoardState = string.Join(",", cells)
            };

            return View(model);
        }

        // =========================
        // HANDLE CELL CLICK (POST) Jacob
        // =========================
        // TODO: Jacob - Replace this temporary click handling with real board logic.
        // The controller already receives row, col, boardSize, and difficulty.
        // This is where the clicked cell should be revealed using the actual Board class.

        // Receives the clicked row and column from the Game board.

        // posts back to the controller so the board can be updated.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HandleCellClick(int row, int col, int boardSize, string difficulty, string boardState)
        {
            // Make sure the user is still logged in
            string username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            // Rebuild the board from the posted boardState string
            List<string> cells;

            if (string.IsNullOrEmpty(boardState))
            {
                cells = new List<string>();

                for (int i = 0; i < boardSize * boardSize; i++)
                {
                    // A hidden cell is shown with a filled square symbol for now
                    cells.Add("■");
                }
            }
            else
            {
                cells = boardState.Split(',').ToList();
            }

            // Convert row/col into a list index
            int clickedIndex = (row * boardSize) + col;

            // Temporarily mark the clicked cell so it stays changed
            if (clickedIndex >= 0 && clickedIndex < cells.Count)
            {
                // A clicked cell is temporarily shown as blank to simulate a reveal.
                // Later, the Board and Cell classes will determine the real display value.
                cells[clickedIndex] = "";
            }

            // Create a ViewModel to send updated board data back to the view
            GameBoardViewModel model = new GameBoardViewModel
            {
                BoardSize = boardSize,
                Difficulty = difficulty,
                Cells = cells,
                BoardState = string.Join(",", cells),
                ClickedRow = row,
                ClickedCol = col
            };

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



        // TODO: Jacob - Add real WIN/LOSS  detection here.
        // If the player hits a mine, redirect to Loss().
        // If the player clears the board, redirect to Win(score).


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
    }
}

using Microsoft.AspNetCore.Mvc;
using MinesweeperApp.Models;
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
            ViewBag.Message = "Registration successful! You can now log in.";
            return View("Register");
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
            return RedirectToAction("Login");

        }

            // =========================
            // LOGIN (GET)
            // =========================
            // Shows login form
            [HttpGet]
        public IActionResult Login()
        {
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
        // LOGOUT
        // =========================
        public IActionResult Logout()
        {
            // Clear the current session
            HttpContext.Session.Clear();

            // Send the user back to login
            return RedirectToAction("Login");
        }
    }
}

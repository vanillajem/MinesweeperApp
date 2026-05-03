using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MinesweeperApp.Models;

namespace MinesweeperApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GamesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<IEnumerable<SavedGameModel>> GetAllGames()
        {
            return Ok(_context.Games.ToList());
        }

        [HttpGet("{id}")]
        public ActionResult<SavedGameModel> GetGameById(int id)
        {
            SavedGameModel game = _context.Games.FirstOrDefault(g => g.Id == id);

            if (game == null)
            {
                return NotFound();
            }

            return Ok(game);
        }

        [HttpPost]
        public ActionResult<SavedGameModel> CreateGame(SavedGameModel game)
        {
            game.DateSaved = DateTime.Now;

            _context.Games.Add(game);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetGameById), new { id = game.Id }, game);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteGame(int id)
        {
            SavedGameModel game = _context.Games.FirstOrDefault(g => g.Id == id);

            if (game == null)
            {
                return NotFound();
            }

            _context.Games.Remove(game);
            _context.SaveChanges();

            return NoContent();
        }
    }
}

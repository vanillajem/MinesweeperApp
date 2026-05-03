namespace MinesweeperApp.Models
{
    public class SavedGameModel
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public DateTime DateSaved { get; set; } = DateTime.Now;

        public int BoardSize { get; set; }

        public string Difficulty { get; set; } = string.Empty;

        public string BoardJson { get; set; } = string.Empty;
    }
}
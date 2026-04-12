namespace MinesweeperApp.Models
{
    public class GameScore
    {
        // Properties for the GameScore class
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public int BoardSize { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public int Score { get; set; }
        public DateTime DatePlayed { get; set; }
    }
}

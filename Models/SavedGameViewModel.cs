namespace MinesweeperApp.Models
{
    // Added by Angela: view model used to display the saved games screen for Milestone 4.
    // TODO Jacob: replace this sample display data with saved game records from the Games database table.
    public class SavedGameViewModel
    {
        public int Id { get; set; }
        public string DateSaved { get; set; } = string.Empty;
        public string BoardSize { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
    }
}

using MinesweeperApp.Models;

namespace MinesweeperApp.Services
{
    public interface IGameService
    {
        int GetMineCount(int boardSize, string difficulty);
        GameBoardViewModel BuildViewModel(Board board, string difficulty);
        int CalculateScore(Board board, string difficulty);
        void SaveGameScore(string username, Board board, string difficulty);
    }
}
 
namespace SEBIZ.Service
{
    public interface IPlaytimeService
    {
        Task StartPlaytimeAsync(string userId, string gameId);
        Task StopPlaytimeAsync(string userId, string gameId);
    }
}

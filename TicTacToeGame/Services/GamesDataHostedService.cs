using TicTacToeGame.Data;
using TicTacToeGame.Models;

namespace TicTacToeGame.Services
{
    public class GamesDataHostedService : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<GamesDataHostedService> logger;
        private Timer? timer = null!;

        public GamesDataHostedService(IServiceScopeFactory serviceScopeFactory, ILogger<GamesDataHostedService> logger)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Games data health checker started");

            timer = new Timer(CheckAndClearOldGamesData, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Games data health checker stopped");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        private void CheckAndClearOldGamesData(object? state)
        {
            logger.LogInformation("Clearing old games data...");

            using (var scope = serviceScopeFactory.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IGameSessionsRepository>();

                var oldGames = repository.GetOldGameSessions(TimeSpan.FromMinutes(10));
                if (oldGames.Any())
                {
                    logger.LogInformation($"Found {oldGames.Count()} old games. Clearing...");

                    foreach (GameSession game in oldGames)
                    {
                        repository.RemoveGameSession(game.GameSessionId);
                    }

                    repository.SaveChanges();

                    logger.LogInformation("Old games cleared");
                }
                else
                {
                    logger.LogInformation("No games older than 10 minutes found");
                }
            }
        }
    }
}
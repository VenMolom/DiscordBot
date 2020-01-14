using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    class LoggingService
    {
        private readonly ILogger _logger;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _cmdService;

        public LoggingService(ILogger<LoggingService> logger, DiscordSocketClient client, CommandService cmdService)
        {
            _logger = logger;
            _client = client;
            _cmdService = cmdService;

            _client.Ready += OnReadyAsync;
            _client.Log += OnLogAsync;
            _cmdService.Log += OnLogAsync;
        }

        public Task OnReadyAsync()
        {
            _logger.LogInformation($"Connected as -> [{_client.CurrentUser}]");
            _logger.LogInformation($"We are on [{_client.Guilds.Count}] servers");
            return Task.CompletedTask;
        }

        public Task OnLogAsync(LogMessage msg)
        {
            switch (msg.Severity.ToString())
            {
                case "Critical":
                    {
                        _logger.LogCritical(msg.ToString());
                        break;
                    }
                case "Warning":
                    {
                        _logger.LogWarning(msg.ToString());
                        break;
                    }
                case "Info":
                    {
                        _logger.LogInformation(msg.ToString());
                        break;
                    }
                case "Verbose":
                    {
                        _logger.LogInformation(msg.ToString());
                        break;
                    }
                case "Debug":
                    {
                        _logger.LogDebug(msg.ToString());
                        break;
                    }
                case "Error":
                    {
                        _logger.LogError(msg.ToString());
                        break;
                    }
            }

            return Task.CompletedTask;
        }
    }
}

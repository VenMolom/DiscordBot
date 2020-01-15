using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class LoggingService
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

        public Task Log(LogMessage msg)
        {
            OnLogAsync(msg);
            return Task.CompletedTask;
        }

        private Task OnReadyAsync()
        {
            OnLogAsync(new LogMessage(LogSeverity.Info, "Ready", $"Connected as {_client.CurrentUser}"));
            OnLogAsync(new LogMessage(LogSeverity.Info, "Ready", $"We are on {_client.Guilds.Count} servers"));
            return Task.CompletedTask;
        }

        private Task OnLogAsync(LogMessage msg)
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

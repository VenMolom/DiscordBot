using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Serilog;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using DiscordBot.Services;

namespace DiscordBot
{
    class DiscordBotClient
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _cmdService;
        private IServiceProvider _services;

        public DiscordBotClient()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                MessageCacheSize = 50,
                LogLevel = LogSeverity.Debug
            });

            _cmdService = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                CaseSensitiveCommands = false
            });



            Serilog.Log.Logger = new LoggerConfiguration()
                .WriteTo.File("Logs/DiscordBot.log", rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .CreateLogger();
        }

        public async Task InitializeAsync()
        {
            _services = ConfigureServices();
            _services.GetRequiredService<LoggingService>();
            await _services.GetRequiredService<CommandHandler>().InstallCommandsAsync();
            

            _client.Ready += OnReadyAsync;

            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DiscordToken"));
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        public async Task OnReadyAsync()
        {
            await _client.SetActivityAsync(new Game("3 Wiedźmin 3 najlepszy"));
            await _client.SetStatusAsync(UserStatus.AFK);
        }

        public IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_cmdService)
            .AddSingleton<AudioService>()
            .AddSingleton<CommandHandler>()
            .AddSingleton<LoggingService>()
            .AddLogging(configure => configure.AddSerilog());

            services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Debug);

            return services.BuildServiceProvider();
        }
    }
}

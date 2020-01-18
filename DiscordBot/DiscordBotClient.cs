using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Serilog;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using DiscordBot.Services;
using Victoria;
using DiscordBot.Entities;

namespace DiscordBot
{
    class DiscordBotClient
    {
        public static readonly Game defaultGame = new Game("3 Wiedźmin 3 najlepszy");

        private readonly DiscordSocketClient _client;
        private readonly CommandService _cmdService;
        private readonly LavaNode _lavaNode;
        private IServiceProvider _services;
        private readonly Config _config;

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
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async
            });

            _lavaNode = new LavaNode(_client, new LavaConfig
            {
                Port = 2334,
                LogSeverity = LogSeverity.Debug,
                SelfDeaf = false
            });

            _config = new ConfigService().GetConfig();

            Serilog.Log.Logger = new LoggerConfiguration()
                .WriteTo.File("Logs/DiscordBot.log", rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .CreateLogger();
        }

        public async Task InitializeAsync()
        {
            _services = ConfigureServices();
            await _services.GetRequiredService<CommandHandler>().InstallCommandsAsync();
            

            _client.Ready += OnReadyAsync;

            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        public async Task OnReadyAsync()
        {
            await _lavaNode.ConnectAsync();
            await _client.SetActivityAsync(defaultGame);
            await _client.SetStatusAsync(UserStatus.AFK);
        }

        public IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_cmdService)
            .AddSingleton(_lavaNode)
            .AddSingleton<AudioService>()
            .AddSingleton<CommandHandler>()
            .AddSingleton<LoggingService>()
            .AddLogging(configure => configure.AddSerilog());

            services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Debug);

            return services.BuildServiceProvider();
        }
    }
}

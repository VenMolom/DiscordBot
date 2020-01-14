using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.IO;

namespace DiscordBot
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();

            _client.Log += Log;

            CommandService commands = new CommandService();
            IServiceProvider service = new Initialize(commands, _client).BuildServiceProvider();
            CommandHandler handler = new CommandHandler(_client, commands, service);
            await handler.InstallCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DiscordToken"));
            await _client.StartAsync();

            await _client.SetActivityAsync(new Game("3 Wiedźmin 3 najlepszy"));
            await _client.SetStatusAsync(UserStatus.AFK);

            await Task.Delay(-1);
        }

        public Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}

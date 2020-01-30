using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Victoria;

namespace DiscordBot.Services
{
    public class AdminService
    {
        private readonly DiscordSocketClient _client;
        private readonly LavaNode _lavaNode;

        public AdminService(DiscordSocketClient client, LavaNode lavaNode)
        {
            _client = client;
            _lavaNode = lavaNode;
        }

        public async Task TestAsync(ISocketMessageChannel channel)
        {
            await channel.SendMessageAsync("Admin test message");
        }

        public async Task PingAsync(SocketUser user, ISocketMessageChannel channel, int iterations)
        {
            var handle = user.Mention;
            for(int i = 0; i < iterations; ++i)
                await channel.SendMessageAsync(handle);
        }

        public async Task QuitAsync()
        {
            await _lavaNode.DisconnectAsync();
            await _client.StopAsync();
            await _client.LogoutAsync();
            System.Environment.Exit(0);
        }
    }
}

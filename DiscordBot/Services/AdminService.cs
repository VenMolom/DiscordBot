using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DiscordBot.Services
{
    public class AdminService
    {
        private readonly DiscordSocketClient _client;

        public AdminService(DiscordSocketClient client)
        {
            _client = client;
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
    }
}

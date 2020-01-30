using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Services;

namespace DiscordBot.Modules
{
    [Group("admin")]
    [RequireOwner]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        private readonly AdminService _adminService;

        public AdminModule(AdminService adminService)
        {
            _adminService = adminService;
        }

        [Command("test")]
        public async Task TestAsync()
            => await _adminService.TestAsync(Context.Channel);

        [Command("ping")]
        public async Task PingAsync(SocketUser user, int iterations = 1)
            => await _adminService.PingAsync(user, Context.Channel, iterations);

        [Command("quit")]
        public async Task QuitAsync()
            => await _adminService.QuitAsync();
    }
}

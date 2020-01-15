using Discord;
using Discord.Commands;
using DiscordBot.Entities;
using DiscordBot.Services;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        private readonly AudioService _service;

        public AudioModule(AudioService service)
        {
            _service = service;
        }

        [Command("join", RunMode = RunMode.Async)]
        public async Task<RuntimeResult> JoinAsync()
            => await _service.JoinAsync(Context.User as IGuildUser, Context.Channel as ITextChannel);

        [Command("leave", RunMode = RunMode.Async)]
        public async Task<RuntimeResult> LeaveAsync()
           => await _service.LeaveAsync(Context.Guild, Context.User);
      

        [Command("play", RunMode = RunMode.Async)]
        public async Task<RuntimeResult> SendVoiceAsync()
        {
            return await _service.SendVoiceAsync(Context.Guild, Context.Channel);
        }
    }
}

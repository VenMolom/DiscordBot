using Discord;
using Discord.Commands;
using DiscordBot.Services;
using System.Threading.Tasks;

namespace DiscordBot
{
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        private readonly AudioService _service;

        public AudioModule(AudioService service)
        {
            _service = service;
        }

        [Command("join", RunMode = RunMode.Async)]
        public async Task<RuntimeResult> JoinVoiceAsync(IVoiceChannel channel = null)
        {
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                return MyCustomResult.FromError("User must be in a voice channel, or a voice channel must be passed as an argument.");
            }
            return await _service.JoinVoiceAsync(Context.Guild, channel);
        }

        [Command("leave", RunMode = RunMode.Async)]
        public async Task<RuntimeResult> LeaveVoiceAsync()
        {
            return await _service.LeaveVoiceAsync(Context.Guild);
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task<RuntimeResult> SendVoiceAsync()
        {
            return await _service.SendVoiceAsync(Context.Guild, Context.Channel);
        }
    }
}

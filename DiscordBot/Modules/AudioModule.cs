using Discord;
using Discord.Commands;
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

        [Command("join")]
        public async Task<RuntimeResult> JoinAsync()
            => await _service.JoinAsync(Context.User as IGuildUser, Context.Channel as ITextChannel);

        [Command("leave")]
        public async Task<RuntimeResult> LeaveAsync()
            => await _service.LeaveAsync(Context.Guild, Context.User as IGuildUser);
      

        [Command("play")]
        public async Task<RuntimeResult> SendVoiceAsync([Remainder] string query = null)
            => await _service.PlayAsync(Context.Guild, query);

        [Command("pause")]
        public async Task<RuntimeResult> PauseAsync()
            => await _service.PauseAsync(Context.Guild);

        [Command("resume")]
        public async Task<RuntimeResult> ResumeAsync()
            => await _service.ResumeAsync(Context.Guild);

        [Command("skip")]
        public async Task<RuntimeResult> SkipAsync()
            => await _service.SkipAsync(Context.Guild);

        [Command("audio")]
        public async Task<RuntimeResult> VolumeAsync(ushort volume)
            => await _service.VolumeAsync(Context.Guild, volume);
    }
}

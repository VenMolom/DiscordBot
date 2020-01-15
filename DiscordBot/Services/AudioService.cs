using Discord;
using Discord.Audio;
using Discord.Commands;
using DiscordBot.Entities;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Victoria;

namespace DiscordBot.Services
{
    public class AudioService
    {
        private readonly LavaNode _lavaNode;
        private readonly LoggingService _logger;

        public AudioService(LavaNode lavaNode, LoggingService logger)
        {
            _lavaNode = lavaNode;
            _logger = logger;
        }

        public async Task<RuntimeResult> JoinAsync(IGuildUser user, ITextChannel textChannel)
        {
            if (user.VoiceChannel == null)
            {
                return MyCustomResult.FromError("You need to connect to voice channel.");
            }

            if (_lavaNode.HasPlayer(user.Guild))
            {
                return MyCustomResult.FromError("Already connected to channel in this guild!");
            }

            try
            {
                await _lavaNode.JoinAsync(user.VoiceChannel, textChannel);
                return MyCustomResult.FromSuccess($"Joined {user.VoiceChannel.Name}.");
            }
            catch(Exception e)
            {
                return MyCustomResult.FromError(e.Message);
            }
        }

        public async Task<RuntimeResult> LeaveAsync(IGuild guild, IGuildUser user)
        {
            if(!_lavaNode.TryGetPlayer(guild, out var player))
            {
                return MyCustomResult.FromError("I'm not connected to any voice channel!");
            }

            var voiceChannel = user.VoiceChannel ?? player.VoiceChannel;
            if(voiceChannel == null)
            {
                return MyCustomResult.FromError("Not sure which voice channel to disconnect from.");
            }

            try
            {
                await _lavaNode.LeaveAsync(voiceChannel);
                return MyCustomResult.FromSuccess($"Left {voiceChannel.Name}.");
            }
            catch(Exception e)
            {
                return MyCustomResult.FromError(e.Message);
            }
        }

        public async Task<RuntimeResult> PlayAsync(IGuild guild, string query)
        {

        }

        //public async Task<RuntimeResult> SendVoiceAsync(IGuild guild, IMessageChannel channel)
        //{
        //    string path = "002_1_1_2[1]_ohayoo.mp3";

        //    if(!File.Exists(path))
        //    {
        //        return MyCustomResult.FromError("File does not exist");
        //    }

        //    IAudioClient audioClient;

        //    if(ConnectedChannels.TryGetValue(guild.Id, out audioClient))
        //    {
        //        using (var ffmpeg = CreateStream(path))
        //        using (var output = ffmpeg.StandardOutput.BaseStream)
        //        using (var discord = audioClient.CreatePCMStream(AudioApplication.Music))
        //        {
        //            await channel.SendMessageAsync($"Playing {path} in {guild.Name}");
        //            try { await output.CopyToAsync(discord); }
        //            finally { await discord.FlushAsync(); }
        //        }
        //        return MyCustomResult.FromSuccess($"Finished playing {path} in {guild.Name}");
        //    }

        //    return MyCustomResult.FromError("I'm not connected to any channel in this guild");
        //}

        //private Process CreateStream(string path)
        //{
        //    return Process.Start(new ProcessStartInfo
        //    {
        //        FileName = "ffmpeg",
        //        Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
        //        UseShellExecute = false,
        //        RedirectStandardOutput = true,
        //    });
        //}
    }
}
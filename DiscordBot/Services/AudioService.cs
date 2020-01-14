using Discord;
using Discord.Audio;
using Discord.Commands;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class AudioService
    {
        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

        public async Task<RuntimeResult> JoinVoiceAsync(IGuild guild, IVoiceChannel channel = null)
        {
            IAudioClient audioClient;

            if(ConnectedChannels.TryGetValue(guild.Id, out audioClient))
            {
                return MyCustomResult.FromError("Already connected to channel in this guild");
            }
            if(channel.Guild.Id != guild.Id)
            {
                return MyCustomResult.FromError("Channel doesn't belong to this guild");
            }

            try
            {
                audioClient = await channel.ConnectAsync();
            }
            catch(TimeoutException)
            {
                return MyCustomResult.FromError($"Cannot connect to {channel.Name}");
            }

            if(ConnectedChannels.TryAdd(guild.Id, audioClient))
            {
                return MyCustomResult.FromSuccess($"Connected to voice channel {channel.Name} on {guild.Name}");
            }

            return MyCustomResult.FromError($"Cannot connect to {channel.Name}");
        }

        public async Task<RuntimeResult> LeaveVoiceAsync(IGuild guild)
        {
            IAudioClient audioClient;

            if(ConnectedChannels.TryRemove(guild.Id, out audioClient))
            {
                await audioClient.StopAsync();
                return MyCustomResult.FromSuccess("Disconnected");
            }

            return MyCustomResult.FromError("I'm not connected to any channel in this guild");
        }

        public async Task<RuntimeResult> SendVoiceAsync(IGuild guild, IMessageChannel channel)
        {
            string path = "002_1_1_2[1]_ohayoo.mp3";

            if(!File.Exists(path))
            {
                return MyCustomResult.FromError("File does not exist");
            }

            IAudioClient audioClient;

            if(ConnectedChannels.TryGetValue(guild.Id, out audioClient))
            {
                using (var ffmpeg = CreateStream(path))
                using (var output = ffmpeg.StandardOutput.BaseStream)
                using (var discord = audioClient.CreatePCMStream(AudioApplication.Music))
                {
                    await channel.SendMessageAsync($"Playing {path} in {guild.Name}");
                    try { await output.CopyToAsync(discord); }
                    finally { await discord.FlushAsync(); }
                }
                return MyCustomResult.FromSuccess($"Finished playing {path} in {guild.Name}");
            }

            return MyCustomResult.FromError("I'm not connected to any channel in this guild");
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }
    }
}
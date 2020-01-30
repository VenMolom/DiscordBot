using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Entities;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace DiscordBot.Services
{
    public class AudioService
    {
        private readonly LavaNode _lavaNode;
        private readonly DiscordSocketClient _client;

        public AudioService(LavaNode lavaNode, DiscordSocketClient client)
        {
            _lavaNode = lavaNode;
            _client = client;

            _lavaNode.OnTrackEnded += OnTrackEndedAsync;
        }

        private async Task OnTrackEndedAsync(TrackEndedEventArgs arg)
        {
            var player = arg.Player;
            var users = await (player.VoiceChannel as IChannel).GetUsersAsync().FlattenAsync();
            if (users.Count() == 1)
            {
                await player.TextChannel.SendMessageAsync("No users listening, I'm leaving!");
                await LeaveAsync(player.VoiceChannel.Guild, null);
                return;
            }

            if(!arg.Reason.ShouldPlayNext())
            {
                return;
            }

            if(!player.Queue.TryDequeue(out var queueable))
            {
                await player.TextChannel.SendMessageAsync("No more songs to play.");
                return;
            }

            if(!(queueable is LavaTrack track))
            {
                await player.TextChannel.SendMessageAsync("Can't play next song!");
                return;
            }

            await player.PlayAsync(track);
            await player.TextChannel.SendMessageAsync($"{arg.Reason}: **{arg.Track.Title}**\nNow playing: **{track.Title}**.");
        }

        public async Task<RuntimeResult> JoinAsync(IGuildUser user, ITextChannel textChannel)
        {
            if (user?.VoiceChannel == null)
            {
                return CommandResult.FromError("You need to connect to voice channel.");
            }

            if (_lavaNode.HasPlayer(user.Guild))
            {
                return CommandResult.FromError("Already connected to channel in this guild!");
            }

            try
            {
                await _lavaNode.JoinAsync(user.VoiceChannel, textChannel);
                return CommandResult.FromSuccess($"Joined **{user.VoiceChannel.Name}**.");
            }
            catch (Exception e)
            {
                return CommandResult.FromError(e.Message);
            }
        }

        public async Task<RuntimeResult> LeaveAsync(IGuild guild, IGuildUser user)
        {
            if (!_lavaNode.TryGetPlayer(guild, out var player))
            {
                return CommandResult.FromError("I'm not connected to any voice channel!");
            }

            var voiceChannel = user?.VoiceChannel ?? player?.VoiceChannel;
            if (voiceChannel == null)
            {
                return CommandResult.FromError("Not sure which voice channel to disconnect from.");
            }

            try
            {
                await _lavaNode.LeaveAsync(voiceChannel);
                return CommandResult.FromSuccess($"Left **{voiceChannel.Name}**.");
            }
            catch (Exception e)
            {
                return CommandResult.FromError(e.Message);
            }
        }

        public async Task<RuntimeResult> PlayAsync(IGuild guild, string query)
        {
            if (!_lavaNode.HasPlayer(guild))
            {
                return CommandResult.FromError("I'm not connected to any voice channel!");
            }

            if (String.IsNullOrWhiteSpace(query))
            {
                return CommandResult.FromError("Please provide a search term.");
            }

            var searchResult = await _lavaNode.SearchAsync(query);

            if (searchResult.LoadStatus == LoadStatus.NoMatches ||
                searchResult.LoadStatus == LoadStatus.LoadFailed)
            {
                return CommandResult.FromError($"Can't find anything!");
            }

            var player = _lavaNode.GetPlayer(guild);

            if (player.PlayerState == PlayerState.Playing ||
                player.PlayerState == PlayerState.Paused)
            {
                if (!String.IsNullOrWhiteSpace(searchResult.Playlist.Name))
                {
                    foreach (var track in searchResult.Tracks)
                    {
                        player.Queue.Enqueue(track);
                    }
                    return CommandResult.FromSuccess($"Enqueued {searchResult.Tracks.Count} songs.");
                }
                else
                {
                    var track = searchResult.Tracks[0];
                    player.Queue.Enqueue(track);
                    return CommandResult.FromSuccess($"Enqueued {track.Title}.");
                }
            }
            else
            {
                var track = searchResult.Tracks[0];
                if (String.IsNullOrWhiteSpace(searchResult.Playlist.Name))
                {
                    await player.PlayAsync(track);
                    return CommandResult.FromSuccess($"Playing {track.Title}.");
                }
                else
                {
                    await player.PlayAsync(track);
                    for (int i = 1; i < searchResult.Tracks.Count; ++i)
                    {
                        player.Queue.Enqueue(searchResult.Tracks[i]);
                    }
                    return CommandResult.FromSuccess($"Enqueued {searchResult.Tracks.Count} songs.\nPlaying {track.Title}.");
                }
            }
        }

        public async Task<RuntimeResult> PauseAsync(IGuild guild)
        {
            if (!_lavaNode.TryGetPlayer(guild, out var player))
            {
                return CommandResult.FromError("I'm not connected to any voice channel!");
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                return CommandResult.FromError("I'm not playing anything!");
            }

            try
            {
                await player.PauseAsync();
                return CommandResult.FromSuccess($"Paused: **{player.Track.Title}**.");
            }
            catch (Exception e)
            {
                return CommandResult.FromError(e.Message);
            }
        }

        public async Task<RuntimeResult> ResumeAsync(IGuild guild)
        {
            if (!_lavaNode.TryGetPlayer(guild, out var player))
            {
                return CommandResult.FromError("I'm not connected to any voice channel!");
            }

            if (player.PlayerState != PlayerState.Paused)
            {
                return CommandResult.FromError("I'm not paused!");
            }

            try
            {
                await player.ResumeAsync();
                return CommandResult.FromSuccess($"Resumed: **{player.Track.Title}**.");
            }
            catch (Exception e)
            {
                return CommandResult.FromError(e.Message);
            }
        }

        public async Task<RuntimeResult> SkipAsync(IGuild guild)
        {
            if (!_lavaNode.TryGetPlayer(guild, out var player))
            {
                return CommandResult.FromError("I'm not connected to any voice channel!");
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                return CommandResult.FromError("I can't skip when I'm not playing!");
            }

            if (player.Queue.Count == 0)
            {
                await player.StopAsync();
                return CommandResult.FromSuccess("No more songs to play.");
            }

            try
            {
                var oldTrack = player.Track;
                await player.SkipAsync();
                return CommandResult.FromSuccess($"Skipped: **{oldTrack.Title}**.\nPlaying: **{player.Track.Title}**.");
            }
            catch (Exception e)
            {
                return CommandResult.FromError(e.Message);
            }
        }

        public async Task<RuntimeResult> VolumeAsync(IGuild guild, ushort volume)
        {
            if (!_lavaNode.TryGetPlayer(guild, out var player))
            {
                return CommandResult.FromError("I'm not connected to any voice channel!");
            }

            try
            {
                await player.UpdateVolumeAsync(volume);
                return CommandResult.FromSuccess($"Changed volume to **{volume}**.");
            }
            catch (Exception e)
            {
                return CommandResult.FromError(e.Message);
            }
        }

        public async Task<RuntimeResult> StopAsync(IGuild guild)
        {
            if (!_lavaNode.TryGetPlayer(guild, out var player))
            {
                return CommandResult.FromError("I'm not connected to any voice channel!");
            }

            if (player.PlayerState != PlayerState.Playing &&
                player.PlayerState != PlayerState.Paused)
            {
                return CommandResult.FromError("I can't stop when I'm not playing!");
            }

            try
            {
                await player.StopAsync();
                player.Queue.Clear();
                return CommandResult.FromSuccess($"Stopped playing.");
            }
            catch (Exception e)
            {
                return CommandResult.FromError(e.Message);
            }
        }
    }
}
using System;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace DiscordBot
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command("say")]
        [Summary("Echoes a message")]
        public Task SayAsync(
            [Remainder][Summary("The text to echo")] string echo)
            => ReplyAsync(echo);

        [Command("eat")]
        public async Task<RuntimeResult> ChooseAsync(string food)
        {
            if (food == "salad")
                return MyCustomResult.FromError("No, I don't want that!");
            return MyCustomResult.FromSuccess($"Give me the {food}!");
        }
    }

    [Group("sample")]
    public class SampleModule : ModuleBase<SocketCommandContext>
    {
        [Command("square")]
        [Summary("Squares a number.")]
        public async Task SquareAsync(
            [Summary("The number to square.")] int num)
        {
            await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
        }

        [Command("userinfo")]
        [Summary("Returns info about the current user, or the user parameter, if one passed.")]
        [Alias("user", "whois")]
        public async Task UserInfoAsync(
            [Summary("The (optional) user to get info from")] SocketUser user = null)
        {
            var userInfo = user ?? Context.Client.CurrentUser;
            await ReplyAsync(
                $"{userInfo.Username}#{userInfo.Discriminator}" +
                $"\nCreated: {userInfo.CreatedAt.DateTime.ToString()}" +
                ((userInfo.Activity != null) ? $"\n{userInfo.Activity.Type.ToString()}: {userInfo.Activity.Name}" : ""));
        }
    }
}

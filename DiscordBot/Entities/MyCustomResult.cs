using Discord.Commands;

namespace DiscordBot
{
    public class MyCustomResult : RuntimeResult
    {
        public MyCustomResult(CommandError? error, string reason) 
            :base(error, reason)
        {   }

        public static MyCustomResult FromError(string reason)
            => new MyCustomResult(CommandError.Unsuccessful, reason);

        public static MyCustomResult FromSuccess(string reason)
            => new MyCustomResult(null, reason);
    }
}

namespace DiscordBot
{
    class Program
    {
        static void Main(string[] args)
            => new DiscordBotClient().InitializeAsync().GetAwaiter().GetResult();
    }
}

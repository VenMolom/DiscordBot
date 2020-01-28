using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using DiscordBot.Services;
using DiscordBot.Entities;
using System.Threading;

namespace DiscordBot
{
    public class CommandHandler
    {
        private readonly int waitTime = 300000;

        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private readonly LoggingService _logger;

        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services, LoggingService logger)
        {
            _commands = commands;
            _client = client;
            _services = services;
            _logger = logger;
        }

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            _commands.CommandExecuted += OnCommandExecutedAsync;
            _commands.Log += OnLogAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task OnLogAsync(LogMessage logMessage)
        {
            if (logMessage.Exception is CommandException cmdException)
            {
                await cmdException.Context.Channel.SendMessageAsync("Something went catastrophically wrong!");
                var application = await _client.GetApplicationInfoAsync();
                await application.Owner.SendMessageAsync("```" + cmdException.ToString() + "```");
            }
        }

        private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            switch (result)
            {
                case CommandResult commandResult:
                    Emoji emoji = commandResult.IsSuccess ? new Emoji("✅") : new Emoji("❌");
                    var message = await context.Channel.SendMessageAsync(emoji + " " + commandResult.Reason);
                    SleepAndDelete(waitTime, message);
                    break;
                default:
                    if (!result.IsSuccess)
                    {
                        string cmd = command.IsSpecified ? command.Value?.Name : "";
                        await _logger.Log(new LogMessage(LogSeverity.Info, "Command", $"Failed to execute \"{cmd}\" for {context.User} in {context.Guild}/{context.Channel}."));
                        await _logger.Log(new LogMessage(LogSeverity.Info, "Command", result.ErrorReason));
                        await context.Channel.SendMessageAsync(new Emoji("❌") + " " + result.ErrorReason);
                    }
                    break;
            }
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;

            if (!(message.HasCharPrefix('!', ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            SocketCommandContext context = new SocketCommandContext(_client, message);

            await _commands.ExecuteAsync(context, argPos, _services);
        }

        private void SleepAndDelete(int milisecondsTimeout, IUserMessage message)
        {
            if(message == null) return;
            Task.Run(() => 
            {
                Thread.Sleep(5000);
                message.DeleteAsync();
            });
        }
    }
}

﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using DiscordBot.Services;
using DiscordBot.Entities;

namespace DiscordBot
{
    public class CommandHandler
    {
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
            }
        }

        private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            switch (result)
            {
                case MyCustomResult customResult:
                    await context.Channel.SendMessageAsync(customResult.Reason);
                    break;
                default:
                    if (!string.IsNullOrEmpty(result?.ErrorReason))
                    {
                        string cmd = command.IsSpecified ? command.Value?.Name : "";
                        await _logger.Log(new LogMessage(LogSeverity.Info, "Command", $"Failed to execute \"{cmd}\" for {context.User} in {context.Guild}/{context.Channel}."));
                        await _logger.Log(new LogMessage(LogSeverity.Info, "Command", result.ErrorReason));
                        await context.Channel.SendMessageAsync(result.ErrorReason);
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

            var result = await _commands.ExecuteAsync(context, argPos, _services);
        }
    }
}

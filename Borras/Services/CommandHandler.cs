using System.Reflection;
using Borras.Common;
using Borras.Init;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

namespace Borras.Services;

public class CommandHandler : ICommandHandler
{
    private readonly DiscordShardedClient _client;
    private readonly CommandService _commands;
    private readonly InteractionService _slashCommands;

    public CommandHandler(
        DiscordShardedClient client,
        CommandService commands,
        InteractionService slashCommands)
    {
        _client = client;
        _commands = commands;
        _slashCommands = slashCommands;
    }

    public async Task InitializeAsync()
    {
        // add the public modules that inherit InteractionModuleBase<T> to the InteractionService
        await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), Bootstrapper.ServiceProvider);
        await _slashCommands.AddModulesAsync(Assembly.GetExecutingAssembly(), Bootstrapper.ServiceProvider);
        //_client.ShardReady += async _client => {  };
        _client.InteractionCreated += HandleInteractionAsync;
        // Subscribe a handler to see if a message invokes a command.
        _client.MessageReceived += HandleCommandAsync;
        _commands.CommandExecuted += async (optional, context, result) =>
        {
            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            {
                // the command failed, let's notify the user that something happened.
                await context.Channel.SendMessageAsync($"error: {result}");
            }
        };

        foreach (var module in _commands.Modules)
        {
            await Logger.Log(LogSeverity.Info, $"{nameof(CommandHandler)} | Commands", $"Module '{module.Name}' initialized.");
        }
    }

    private async Task HandleCommandAsync(SocketMessage arg)
    {
        // Bail out if it's a System Message.
        if (arg is not SocketUserMessage msg)
            return;

        // We don't want the bot to respond to itself or other bots.
        if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot)
            return;

        // Create a Command Context.
        var context = new ShardedCommandContext(_client, msg);
        

        var markPos = 0;
        await Logger.Log(LogSeverity.Info, $"Trying to execute command", msg.ToString());

        if (msg.HasCharPrefix('!', ref markPos) /*|| msg.HasCharPrefix('/', ref markPos)*/)
        {
            var a = InteractionType.ApplicationCommand;
            var result = await _commands.ExecuteAsync(context, markPos, Bootstrapper.ServiceProvider);

            await Logger.Log(LogSeverity.Info, $"Command execution successful", msg.ToString());
        }
    }
    private async Task HandleInteractionAsync(SocketInteraction arg)
    {

        _client.MessageReceived -= HandleCommandAsync;
        // We don't want the bot to respond to itself or other bots.
        if (arg.User.Id == _client.CurrentUser.Id || arg.User.IsBot)
            return;

        // Create a Command Context.
        var context = new ShardedInteractionContext(_client, arg);

        await Logger.Log(LogSeverity.Info, "Slash command handler: ", $"{arg.User.Username} tried to execute a command");

        
            var a = InteractionType.ApplicationCommand;
            var result = await _slashCommands.ExecuteCommandAsync(context, Bootstrapper.ServiceProvider);

            await Logger.Log(LogSeverity.Info, $"Command execution successful", arg.Data.ToString());
        
    }
}
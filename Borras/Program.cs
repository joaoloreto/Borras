// See https://aka.ms/new-console-template for more information

using Discord.Commands;
using Discord;
using Discord.WebSocket;
using Borras.Common;
using Borras.Init;
using Borras.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Discord.Interactions;

string path = Directory.GetParent(Environment.CurrentDirectory).ToString();
var config = new ConfigurationBuilder()
    .AddJsonFile(path + "/appsettings.json")
    .AddEnvironmentVariables()
    .Build();
var client = new DiscordShardedClient(new DiscordSocketConfig { GatewayIntents = GatewayIntents.All });
var commands = new CommandService(new CommandServiceConfig
{
    // Again, log level:
    LogLevel = LogSeverity.Info,

    // There's a few more properties you can set,
    // for example, case-insensitive commands.
    CaseSensitiveCommands = false,
});
var sCommands = new InteractionService(client, new InteractionServiceConfig
{
    LogLevel = LogSeverity.Info
});
Bootstrapper.Init();
Bootstrapper.RegisterInstance(client);
Bootstrapper.RegisterInstance(commands);
Bootstrapper.RegisterInstance(sCommands);
Bootstrapper.RegisterType<ICommandHandler, CommandHandler>();
Bootstrapper.RegisterInstance(config);

await MainAsync();

async Task MainAsync()
{
    await Bootstrapper.ServiceProvider.GetRequiredService<ICommandHandler>().InitializeAsync();
    
    client.ShardReady += async shard =>
    { 
        await sCommands.RegisterCommandsToGuildAsync(676748495079604225);
        //await sCommands.RegisterCommandsToGuildAsync(722072405899477042);
        
        await Logger.Log(LogSeverity.Info, "ShardReady", "Slash Commands Registered");

        await Logger.Log(LogSeverity.Info, "ShardReady", $"Shard Number {shard.ShardId} is connected and ready!");
    };
    
    // Login and connect.
    var token = config.GetRequiredSection("Settings")["DiscordBotToken"];
    if (string.IsNullOrWhiteSpace(token))
    {
        await Logger.Log(LogSeverity.Error, $"{nameof(Program)} | {nameof(MainAsync)}", "Token is null or empty.");
        return;
    }

    await client.LoginAsync(TokenType.Bot, token);
    await client.StartAsync();
    //await sCommands.RegisterCommandsGloballyAsync();
    //Listing all commands in the logs
    var commandsList = commands.Commands;
    var groupedCommands = commandsList.GroupBy(command => command.Module.Name);
    var slashCommands = sCommands.SlashCommands.GroupBy(command => command.Module.Name);
    foreach (var group in groupedCommands)
    {
        await Logger.Log(LogSeverity.Info, $"Module: {group.Key}", "Successfully");

        foreach (var command in group)
        {
            await Logger.Log(LogSeverity.Info, $"  Command: {command.Name}, Summary: {command.Summary}", "command");
        }
    }
    foreach (var group in slashCommands)
    {
        await Logger.Log(LogSeverity.Info, $"Module: {group.Key}", "Successfully");

        foreach (var command in group)
        {
            await Logger.Log(LogSeverity.Info, $"  Command: {command.Name}, Summary: {command.Description}", "command");
        }
    }
    // Wait infinitely so your bot actually stays connected.
    await Task.Delay(Timeout.Infinite);
}

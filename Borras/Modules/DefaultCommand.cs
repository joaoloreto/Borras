using Borras.Common;
using Borras.Services;
using Discord;
using Discord.Commands;
using RunMode = Discord.Commands.RunMode;

namespace Borras.Modules;

public class DefaultCommand : ModuleBase<ShardedCommandContext>
{
    public CommandService CommandService { get; set; }

    [Command("hello", RunMode = RunMode.Async)]
    public async Task Hello(string args)
    {
        //await Logger.Log(LogSeverity.Info, $"{nameof(CommandHandler)} | Commands", $"Module '{module.Name}' initialized.");
        Console.WriteLine("The code got here");
        Console.WriteLine(args);
        await Context.Message.ReplyAsync($"Hello {Context.User.Username}. Nice to meet you!");
    }
}
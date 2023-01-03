using Borras.Common;
using Borras.Services;
using Discord;
using Discord.Commands;
using RunMode = Discord.Commands.RunMode;

namespace Borras.Modules;

public class MusicCommands : ModuleBase<ShardedCommandContext>
{
    public CommandService CommandService { get; set; }

    [Command("play", RunMode = RunMode.Async)]
    public async Task Play(string args)
    {
        Console.WriteLine("The code got here");
        Console.WriteLine(args);
        //TODO: Music player
        await Context.Message.ReplyAsync($"Hello {Context.User.Username}. Nice to meet you!");
    }
}
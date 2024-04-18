using Borras.Common;
using Discord;
using Discord.Interactions;
using RunMode = Discord.Interactions.RunMode;

namespace Borras.Modules
{
    public class TestModules : InteractionModuleBase<ShardedInteractionContext>
    {
        
        [SlashCommand("test", "it's a fucking test")]
        public async Task FuckingTest()
        {
            await Logger.Log(LogSeverity.Info, "Code got to trying the fucking test", "Test successful.");
            await RespondAsync("test");
            //await Context.Message.ReplyAsync($"{Context.User.Username} has tested this command successfully");
        }
        [SlashCommand("blyat", "it's a blyat", runMode: RunMode.Async)]
        public async Task Blyat(string arg)
        {
            await Logger.Log(LogSeverity.Info, "Code got to trying the fucking test", "Test successful.");
            await Context.Interaction.RespondAsync(arg);
            //await Context.Message.ReplyAsync($"{Context.User.Username} has tested this command successfully");
        }
    }
}

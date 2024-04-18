using Borras.Common;
using Discord;
using Discord.Commands;
using RunMode = Discord.Commands.RunMode;
using Discord.WebSocket;
using Discord.Interactions;

namespace Borras.Modules;

public class MainModule : InteractionModuleBase<ShardedInteractionContext>
{

    [SlashCommand("hello", "Says hello back at you")]
    public async Task Hello()
    {
        await Logger.Log(LogSeverity.Info, $"{Context.User.Username} tried to say hello", "Successfully");
        await RespondAsync($"Hello {Context.User.Username}. Nice to meet you!");
    }

    [SlashCommand("borra","Calls someone a borras")]
    public async Task Borra(string arg)
    {
        if (!arg.StartsWith("<@"))
        {
            await Logger.Log(LogSeverity.Info, $"{Context.User.Username} tried to call someone a borras", "Unsuccessfully");
            await RespondAsync($"{Context.User.Username}. Give me a user name!");
        }
        else
        {
            await Logger.Log(LogSeverity.Info, $"{Context.User.Username} tried to call someone a borras", "Successfully");
            await RespondAsync($"{arg} é um borra piças");
        }
    }
    /*
    [Command("nhlstandings", RunMode = RunMode.Async)]
        
        public async Task NHLStandings()
        {
        try
        {
            NhlApi nhlapi = new();
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            await Logger.Log(LogSeverity.Info, $"{Context.User.Username} tried to get nhl standings data", "Successfully");
            //List<TeamSeasonStatistics> table = nhlapi.GetLeagueStandingsSeasonInformationAsync().Result;
            List<Standing> table = nhlapi.GetLeagueStandingsByDateAsync(today).Result.Standings;
            string standings = "";
            //standings += "Metropolitan Division" + System.Environment.NewLine;
            foreach (Standing standing in table)
            {

                //if (standing.DivisionAbbrev.Equals("A"))
                //{
                    //standings += Environment.NewLine + standing.DivisionName + " division" + Environment.NewLine;
                    standings += standing.TeamName.Default + " | " + standing.Points + " pts" + Environment.NewLine;
                //}
            }
            await Context.Message.ReplyAsync($"{standings}");
        }
        catch (Exception ex)
        {
            await Logger.Log(LogSeverity.Info, $"{Context.User.Username} failed to get nhl standings data", ex.Message);
        }
        }
    */
    [Command("dropdown", RunMode = RunMode.Async)]
    public async Task dropDown()
    {
        
        var menuBuilder = new SelectMenuBuilder()
        .WithPlaceholder("Select an option")
        .WithCustomId("menu-1")
        .WithMinValues(1)
        .WithMaxValues(1)
        .AddOption("Option A", "opt-a", "Option B is lying!")
        .AddOption("Option B", "opt-b", "Option A is telling the truth!");

        var builder = new ComponentBuilder()
            .WithSelectMenu(menuBuilder);

        await ReplyAsync("Whos really lying?", components: builder.Build());
        Context.Client.SelectMenuExecuted += MyMenuHandler;


    }
    public async Task MyMenuHandler(SocketMessageComponent arg)
    {
        var text = string.Join(", ", arg.Data.Values);
        await arg.RespondAsync($"You have selected {text}");
    }
}
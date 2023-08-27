using Borras.Common;
using Borras.Services;
using Discord;
using Discord.Audio;
using Discord.Commands;
using System.Security.Cryptography.X509Certificates;
using Nhl.Api;
using RunMode = Discord.Commands.RunMode;
using Nhl.Api.Models.Standing;
using Nhl.Api.Models.Team;

namespace Borras.Modules;

public class MainModule : ModuleBase<ShardedCommandContext>
{
    public CommandService CommandService { get; set; }

    [Command("hello", RunMode = RunMode.Async)]
    public async Task Hello()
    {
        await Logger.Log(LogSeverity.Info, $"{Context.User.Username} tried to say hello", "Successfully");
        await Context.Message.ReplyAsync($"Hello {Context.User.Username}. Nice to meet you!");
    }

    [Command("borra", RunMode = RunMode.Async)]
    public async Task Borra(string arg)
    {
        if (!arg.StartsWith("<@"))
        {
            await Logger.Log(LogSeverity.Info, $"{Context.User.Username} tried to call someone a borras", "Unsuccessfully");
            await Context.Message.ReplyAsync($"{Context.User.Username}. Give me a user name!");
        }
        else
        {
            await Logger.Log(LogSeverity.Info, $"{Context.User.Username} tried to call someone a borras", "Successfully");
            await Context.Message.ReplyAsync($"{arg} é um borra piças");
        }
    }
    [Command("nhlstandings", RunMode = RunMode.Async)]
        
        public async Task NHLStandings()
        {
        try
        {
            NhlApi nhlapi = new();
            await Logger.Log(LogSeverity.Info, $"{Context.User.Username} tried to get nhl standings data", "Successfully");
            List<Records> table = nhlapi.GetLeagueStandingsAsync().Result;
            string standings = "";
            //standings += "Metropolitan Division" + System.Environment.NewLine;
            foreach (Records record in table)
            {
                foreach (TeamRecord teamRecord in record.TeamRecords) {
                    if (teamRecord.DivisionRank.Equals("1"))
                        standings += System.Environment.NewLine + record.Division.Name + " division" + System.Environment.NewLine;
                    standings += teamRecord.DivisionRank +  ". " + teamRecord.Team.Name + " | " + teamRecord.Points + " pts" + System.Environment.NewLine;
                }
                    
            }
            await Context.Message.ReplyAsync($"{standings}");
        }
        catch (Exception ex)
        {
            await Logger.Log(LogSeverity.Info, $"{Context.User.Username} failed to get nhl standings data", "Successfully");
        }
        }
    }
using Borras.Common;
using Borras.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Text.Json;

namespace Borras.Modules
{
    public class Aoe4Module : InteractionModuleBase<ShardedInteractionContext>
    {
        private string aoe4id = "";
        private string sc2id = "";
        private string regionid = "";
        private string realmid = "";

        [SlashCommand("sc2rank", "Gets your solo rank in starcraft 2")]
        public async Task Sc2Rank()
        {
            try
            {
                var mb = new ModalBuilder()
                .WithTitle("What account will you choose?")
                .WithCustomId("sc2id")
                .AddTextInput("What account do you want to look into", "blizzardid", placeholder: "Blizzard ID");


                await RespondWithModalAsync(mb.Build());
            }
            catch (Exception e)
            {
                await Logger.Log(LogSeverity.Error, "Aoe4Module", $"{Context.User.Username} picked invalid Blizzard id",e);

            }

            Context.Client.ModalSubmitted += Client_ModalSubmitted;

        }

        [SlashCommand("aoe4rank", "Shows whatever ranked data for the account you want")]
        public async Task Aoe4Rank()
        {
            
            try
            {
                var mb = new ModalBuilder()
                .WithTitle("What rank will you choose?")
                .WithCustomId("rank_choice")
                .AddTextInput("What account do you want to look into", "aoe4worldid", placeholder: "aoe4worldid");
                
                
                //await RespondAsync("Select a game mode", components: components.Build(), ephemeral: true);

                await RespondWithModalAsync(mb.Build());
            }
            catch (Exception e)
            {
                await Logger.Log(LogSeverity.Info, "Aoe4Module", $"{Context.User.Username} picked rank");

            }

            //await RespondWithModalAsync(mb.Build());
            Context.Client.ModalSubmitted += Client_ModalSubmitted;
        }

        private async Task Client_ModalSubmitted(SocketModal arg)
        {
            switch (arg.Data.CustomId.ToString())
            {
                


                case "rank_choice":
                    await Logger.Log(LogSeverity.Info, "Aoe4Module", $"{Context.User.Username} picked rank");
                    
                    try 
                    {
                        aoe4id = arg.Data.Components.ToList().First(x => x.CustomId == "aoe4worldid").Value.ToString();
                        var menuBuilder = new SelectMenuBuilder()
                            .WithPlaceholder("Select a game mode")
                            .WithCustomId("gameModeSelect")
                            .WithMinValues(1)
                            .WithMaxValues(1)
                            .AddOption("1v1 Ranked", "1v1Rank")
                            .AddOption("Teams Ranked", "teamsRank")
                            .AddOption("Solo Quick match", "qmsolo");
                        var components = new ComponentBuilder().WithSelectMenu(menuBuilder);
                        AllowedMentions mentions = new AllowedMentions();
                        mentions.AllowedTypes = AllowedMentionTypes.Users;
                        //await arg.RespondAsync(components:menuBuilder.Build());
                        await arg.RespondAsync("Select the game mode",allowedMentions:mentions,components:components.Build());
                        Context.Client.SelectMenuExecuted += Client_SelectMenuHandlerExecuted;
                        await Logger.Log(LogSeverity.Info, "Aoe4Module", "Data fetched successfully");

                    }
                    catch (Exception e) 
                    { 
                        await Logger.Log(LogSeverity.Error, "Aoe4Module", e.Message); 
                    }
                    break;
                case "sc2id":
                    {
                        sc2id = arg.Data.Components.ToList().First(x => x.CustomId == "blizzardid").Value.ToString();
                        var menuBuilder = new SelectMenuBuilder()
                            .WithPlaceholder("Select a region id")
                            .WithCustomId("regionid")
                            .WithMinValues(1)
                            .WithMaxValues(1)
                            .AddOption("US", "1")
                            .AddOption("EU", "2")
                            .AddOption("KO and TW", "3")
                            .AddOption("CN","4");
                        var components = new ComponentBuilder().WithSelectMenu(menuBuilder);
                        AllowedMentions mentions = new AllowedMentions();
                        mentions.AllowedTypes = AllowedMentionTypes.Users;
                        //await arg.RespondAsync(components:menuBuilder.Build());
                        await arg.RespondAsync("Select the region", allowedMentions: mentions, components: components.Build());
                        Context.Client.SelectMenuExecuted += Client_SelectMenuHandlerExecuted;
                        await Logger.Log(LogSeverity.Info, "Aoe4Module", "Data fetched successfully");

                    }
                    break;
                default:
                    break;
            }
            
        }
public async Task Client_SelectMenuHandlerExecuted(SocketMessageComponent arg)
{

            // a switch for all the possible selections from the menu
            // "0" is ranked solo, "1" is ranked team, "2" is quick match 1v1, "3" is quick match 2v2 and so on
            //switch (arg.Data.Values.First())
            switch (arg.Data.CustomId)
            {

                case "regionid":
                    
                        regionid = arg.Data.Values.First().ToString();
                    var menuBuilder = new SelectMenuBuilder()
                            .WithPlaceholder("Select a region id")
                            .WithCustomId("realmid")
                            .WithMinValues(1)
                            .WithMaxValues(1)
                            .AddOption("1", "1")
                            .AddOption("2", "2");
                    var components = new ComponentBuilder().WithSelectMenu(menuBuilder);
                    AllowedMentions mentions = new AllowedMentions();
                    mentions.AllowedTypes = AllowedMentionTypes.Users;
                    await arg.RespondAsync("Select the realm", allowedMentions: mentions, components: components.Build());
                    Context.Client.SelectMenuExecuted += Client_SelectMenuHandlerExecuted;
                    await Logger.Log(LogSeverity.Info, "Aoe4Module", "Data fetched successfully");


                    break;
                case "realmid":
                        realmid = arg.Data.Values.First().ToString();
                    JsonDocument r = APILinksService.GetSC2Data(sc2id, regionid, realmid).Result;
                    await arg.RespondAsync(r.ToString());
                    //await arg.RespondAsync( APILinksService.GetSC2Data(sc2id,regionid,realmid).Result.RootElement.GetProperty("snapshot").GetProperty("seasonSnapshot").GetProperty("1v1").GetProperty("leagueName").ToString());
                    break;
                case "gameModeSelect":
                    switch (arg.Data.Values.First())
                    {
                        case "1v1Rank":

                            await Logger.Log(LogSeverity.Info, $"{Context.User.Username} picked solo rank", "Successful");
                            try
                            {
                                // a standard response with the value of the rating, in this case ranked 1v1, for the actual breakdown of the json procedure see the comments on GetAoE4Data
                                await arg.RespondAsync(APILinksService.GetAoe4data(aoe4id.ToString()).Result.RootElement.GetProperty("modes").GetProperty("rm_solo").GetProperty("rating").ToString());
                            }
                            catch (Exception e)
                            {
                                await Logger.Log(LogSeverity.Error, "Aoe4Module", "No such rank exists");
                                await arg.RespondAsync("No such rank exists");

                            }
                            break;

                        case "teamsRank":
                            await Logger.Log(LogSeverity.Info, $"{Context.User.Username} picked team rank", "Successful");
                            // a standard response with the value of the rating, in this case ranked teams, for the actual breakdown of the json procedure see the comments on GetAoE4Data
                            try
                            {
                                await arg.RespondAsync(APILinksService.GetAoe4data(arg.Data.CustomId.ToString()).Result.RootElement.GetProperty("modes").GetProperty("rm_team").GetProperty("rating").ToString());
                            }
                            catch (Exception e)
                            {
                                await Logger.Log(LogSeverity.Error, "Aoe4Module", e.Message);
                                await arg.RespondAsync("No such rank exists");
                            }
                            break;

                        case "qmsolo":

                            await Logger.Log(LogSeverity.Info, $"{Context.User.Username} picked team rank", "Successful");
                            try
                            {
                                // a standard response with the value of the rating, in this case quick match 1v1, for the actual breakdown of the json procedure see the comments on GetAoE4Data
                                await arg.RespondAsync(APILinksService.GetAoe4data(aoe4id.ToString()).Result.RootElement.GetProperty("modes").GetProperty("qm_1v1").GetProperty("rating").ToString());
                            }
                            catch (Exception e)
                            {
                                await Logger.Log(LogSeverity.Error, "Aoe4Module", e.Message);
                                await arg.RespondAsync("No such rank exists");
                            }
                            break;
                        default:
                            // a logger error in case the selected menu answer doesn't get passed on correctly
                            // todo: correct error handling

                            await Logger.Log(LogSeverity.Info, $"{Context.User.Username} picked something else", "Successful");
                            throw new ArgumentException("Wrong Selection");
                            break;
                    }
                        break;
            }  
}

        
    }
    
}

using Borras.Common;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Text.Json;

namespace Borras.Modules
{
    public class Aoe4Module : ModuleBase<ShardedCommandContext>
    {
        // A command that gets your rating on all game modes
        // todo: all several qm ratings
        [Command("aoe4rank", RunMode = RunMode.Async)]
        public async Task Aoe4Rank(string arg)
        {
            // we build a select menu with all the options we need, we pass our player id as the custom id of the select menu
            var menuBuilder = new SelectMenuBuilder()
        .WithPlaceholder("Select solo or team rank")
        .WithCustomId(arg)
        .WithMinValues(1)
        .WithMaxValues(1)
        .AddOption("Ranked Solo", "0", "Your solo rank")
        .AddOption("Ranked Team", "1", "Your team rank")
        .AddOption("Quick Match 1v1", "2", "Your quick match 1v1 rating");
        // we create the builder of the select menu, copied code
            var builder = new ComponentBuilder()
            .WithSelectMenu(menuBuilder);

            // bot response, your rank is: as a tagged message followed by the actual number
            // we do the get using a menu handler
            await ReplyAsync("Your rank is: ", components: builder.Build());
            try
            {
                Context.Client.SelectMenuExecuted += MyMenuHandler;
            }catch (Exception ex) 
            {
                await Logger.Log(LogSeverity.Error, "Attempt to get AoE4 player data/n " + ex.Message, "Failed");
                await Context.Message.ReplyAsync("Error while selecting an option");
            }
            
}

        // this function handles menu selection, just a simple switch with all the selection cases and one defult to handle any errors
        public async Task MyMenuHandler(SocketMessageComponent arg)
        {
            // a switch for all the possible selections from the menu
            // "0" is ranked solo, "1" is ranked team, "2" is quick match 1v1, "3" is quick match 2v2 and so on
            switch (arg.Data.Values.First())
            {

                case "0":
                    
                    await Logger.Log(LogSeverity.Info, $"{Context.User.Username} picked solo rank", "Successful");
                    // a standard response with the value of the rating, in this case ranked 1v1, for the actual breakdown of the json procedure see the comments on GetAoE4Data
                    await arg.RespondAsync(GetAoE4Data(arg.Data.CustomId.ToString()).Result.RootElement.GetProperty("modes").GetProperty("rm_solo").GetProperty("rating").ToString());
                    break;
                
                case "1":
                    await Logger.Log(LogSeverity.Info, $"{Context.User.Username} picked team rank", "Successful");
                    // a standard response with the value of the rating, in this case ranked teams, for the actual breakdown of the json procedure see the comments on GetAoE4Data

                    await arg.RespondAsync(GetAoE4Data(arg.Data.CustomId.ToString()).Result.RootElement.GetProperty("modes").GetProperty("rm_team").GetProperty("rating").ToString());

                    break;

                case "2":
                    await Logger.Log(LogSeverity.Info, $"{Context.User.Username} picked team rank", "Successful");
                    // a standard response with the value of the rating, in this case quick match 1v1, for the actual breakdown of the json procedure see the comments on GetAoE4Data
                    await arg.RespondAsync(GetAoE4Data(arg.Data.CustomId.ToString()).Result.RootElement.GetProperty("modes").GetProperty("qm_1v1").GetProperty("rating").ToString());
                    break;

                default:
                    // a logger error in case the selected menu answer doesn't get passed on correctly
                    // todo: correct error handling

                    await Logger.Log(LogSeverity.Info, $"{Context.User.Username} picked something else", "Successful");
                    throw new Exception("Wrong Selection");
                    break;
            }  
        }

        // The function that gets the player's data using the AOE4WorldAPI (huge thanks to the AOE4World.com folks)
        // This gets a json with all the data from a specific player id passed on as arg
        private async Task<JsonDocument> GetAoE4Data(string arg)
        {
            string apiUrl = "https://aoe4world.com/api/v0/players/" + arg;
            string responseString;
            JsonDocument? responseJson = null;

            // Here we create an httpclient to handle the get method for accessing the api, fortunately this api is simple to use and we don't need special tokens
            // if you need special tokens you need to set the appropriate headers

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // A response is created, you still need to deserialize
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    await Logger.Log(LogSeverity.Info, "AoE4API GET Attempt", "Successful");
                    // The response is serialized as a string
                    responseString = await response.Content.ReadAsStringAsync();
                    // If the response is not empty and it has in fact returned a string we deserialize it into a json object
                    if (responseString != null)
                    {
                        responseJson = JsonSerializer.Deserialize<JsonDocument>(responseString);
                        await Logger.Log(LogSeverity.Info, "AoE4 response json deserialized", "Successful");
                    }
                    if (responseJson == null)
                        throw new Exception("Response Json is null here");
                }
                catch (Exception ex)
                {
                    await Logger.Log(LogSeverity.Error, "Attempt to get AoE4 player data/n " + ex.Message, "Failed");
                    return null;
                }
                return responseJson;
            }
        }
    }
    
}

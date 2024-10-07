using Discord;
using Borras.Common;
using System.Text.Json;
using System.Globalization;
using Google.Apis.Auth.OAuth2.Responses;
using ChatGPT.Net.DTO.ChatGPTUnofficial;

namespace Borras.Services
{
    public class APILinksService
    {
        private static string path = Directory.GetParent(Environment.CurrentDirectory).ToString();
        private static string GetAOE4APIURL()
        {
            return JsonSerializer.Deserialize<JsonDocument>(File.ReadAllText(path + "/appsettings.json")).RootElement.GetProperty("Aoe4").GetProperty("apiPlayerUrl").ToString();
        }
        private static JsonDocument GetSC2APIData()
        {
            return JsonSerializer.Deserialize<JsonDocument>(File.ReadAllText(path + "/appsettings.json"));
        }
        public static async Task<JsonDocument> GetAoe4data(string arg)
        {
            string apiUrl = GetAOE4APIURL() + arg;
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
                    throw new Exception("GET Method from aoe4world.com API returned a null reference", ex);

                }
                return responseJson;
            }
        }
        /*
        * 
        *  arg1: your profile id, go to https://starcraft2.blizzard.com, open your profile and get the number that is written last
        *  arg2: your region id, first number after the url
        *  arg3: your realmid, second number after the url
        */
        public static async Task<JsonDocument> GetSC2Data(string arg1, string arg2, string arg3)
        {
            //ClientId ClientSecret ClientName RedirectURL
            string clientId = GetSC2APIData().RootElement.GetProperty("Starcraft2").GetProperty("ClientId").ToString();
            string clientSecret = GetSC2APIData().RootElement.GetProperty("Starcraft2").GetProperty("ClientSecret").ToString();
            string tokenEndpoint = "https://eu.battle.net/oauth/token";
            string profileID = arg1;
            string regionID = arg2;
            string realmID = arg3;
            string responseString = "";
            string tokenResponse = "";
            JsonDocument? responseJson = null;
            ResponseModel? responseModel = null;
            string responseContent = "";
            using (HttpClient client = new HttpClient())
            {


                var content = new FormUrlEncodedContent(new Dictionary<string, string>
             {

             { "grant_type", "client_credentials" },
                 { "client_id", clientId },
                 { "client_secret", clientSecret }
             });
                await Logger.Log(LogSeverity.Info, "APILinksService", "Parsed config file: Successfully");

                /*              var content = new FormUrlEncodedContent(new Dictionary<string, string>
                       {
                           { "client_id", clientId },
                           { "client_secret", clientSecret },
                           { "code", authorizationCode },
                           { "redirect_uri", "https://localhost" },
                           { "grant_type", "authorization_code" }

                               });*/
                HttpResponseMessage response = await client.PostAsync(tokenEndpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    tokenResponse = await response.Content.ReadAsStringAsync();
                    responseModel = JsonSerializer.Deserialize<ResponseModel>(tokenResponse);
                    if (responseModel == null || responseModel.access_token.Equals(""))
                        throw new NullReferenceException();
                    await Logger.Log(LogSeverity.Info, "APILinksService", "Fetched client authorization token successfully");
                }
                else
                {
                    await Logger.Log(LogSeverity.Error, "APILinksService", "Failed to obtain a client authorization token. Status code: " + response.StatusCode);
                    throw new HttpRequestException();
                }
            }
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", responseModel.access_token);

                string profileApiUrl = $"https://eu.api.blizzard.com/sc2/profile/{regionID}/{realmID}/{profileID}";

                try
                {
                    HttpResponseMessage response = await client.GetAsync(profileApiUrl);

                    if (response.IsSuccessStatusCode)
                    {

                        responseContent = await response.Content.ReadAsStringAsync();
                        //Console.WriteLine("Profile API Response:", responseContent);

                        await Logger.Log(LogSeverity.Info, "API Profile data retrieved: ", "Successfully");
                        //Console.WriteLine(responseContent);
                        try
                        {
                            return JsonSerializer.Deserialize<JsonDocument>(responseContent);
                        }
                        catch (Exception e)
                        {
                            await Logger.Log(LogSeverity.Error, "Parsing the document", "Unsuccessful");
                        }
                    }
                    else
                    {
                        await Logger.Log(LogSeverity.Error, "APILinksService", "Request to Profile API failed. Status code: " + response.StatusCode);
                    }
                }

                catch (Exception ex)
                {
                    await Logger.Log(LogSeverity.Error, "APILinksService", ex.Message, ex);
                }
                return null;
            }
        }
    }
    class ResponseModel
    {
        public string access_token { get; set; }
    }
}

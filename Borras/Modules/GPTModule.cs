using Discord.Interactions;
using ChatGPT.Net;
using System.Text.Json;

namespace Borras.Modules
{
    public class GPTModule : InteractionModuleBase<ShardedInteractionContext>
    {
        string path = Directory.GetParent(Environment.CurrentDirectory).ToString();

        [SlashCommand("chatgpt", "Ask ChatGPT something")]
        public async Task AskChatGPT(string arg)
        {
            
            var bot = new ChatGpt(serializeChatGPTKey());
            string response = await bot.Ask(arg);
            await RespondAsync(response);
        }

        private string serializeChatGPTKey()
        {
            string fileContents = File.ReadAllText(path + "/appsettings.json");
            return JsonSerializer.Deserialize<JsonDocument>(fileContents).RootElement.GetProperty("Settings").GetProperty("GPTAPIKey").ToString();
        }
    }
}
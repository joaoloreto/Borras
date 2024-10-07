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
            
            ChatGpt bot = serializeChatGPTKey();
            string response = await bot.Ask(arg);
            await RespondAsync(response);
        }

        private ChatGpt serializeChatGPTKey()
        {
            string fileContents = File.ReadAllText(path + "/appsettings.json");
            return new ChatGpt(JsonSerializer.Deserialize<JsonDocument>(fileContents).RootElement.GetProperty("Settings").GetProperty("GPTAPIKey").ToString());
        }
    }
}
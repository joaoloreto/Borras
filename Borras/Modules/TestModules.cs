﻿using Borras.Common;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using RunMode = Discord.Interactions.RunMode;

namespace Borras.Modules
{
    public class TestModules : InteractionModuleBase<ShardedInteractionContext>
    {
        [SlashCommand("test-command", "it's a fucking test")]
        public async Task FuckingTest(SocketSlashCommand command)
        {
            await Logger.Log(LogSeverity.Info, "Code got to trying the fucking test", "Test successful.");
            //await Context.Interaction.ReplyAsync($"{Context.User.Username} has tested this command successfully");
            await Context.Interaction.RespondAsync($"{Context.User.Username} has tested this command successfully");
        }

    }
}
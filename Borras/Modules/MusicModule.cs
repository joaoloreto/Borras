﻿using Borras.Common;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System.Diagnostics;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Borras.Modules;
public class MusicModule : ModuleBase<ShardedCommandContext> //InteractionModuleBase<ShardedInteractionContext>
{

	[Command("join", RunMode = Discord.Commands.RunMode.Async)]
	public async Task Join()
	{
		//checking if we're in a voice channel
		var voiceChannel = (Context.User as IGuildUser)?.VoiceChannel;
		//if not abort
		if (voiceChannel == null)
		{
			await ReplyAsync("You must be in a voice channel to use this command.");
			await Logger.Log(LogSeverity.Info, $"Attempting to connect to voice channel", "Not in voice channel, aborting");
			return;
		}
		//if yes, join
		

		JoinVoice();
		await Logger.Log(LogSeverity.Info, $"Attempting to connect to voice channel", "Successful");
	}

	[Command("disconnect", RunMode = Discord.Commands.RunMode.Async)]
	public async Task Disconnect()
	{
		//just disconnect, no checks
		IVoiceChannel channel = (Context.User as IVoiceState).VoiceChannel;
		await channel.DisconnectAsync();
		await ReplyAsync("Disconnected");
	}


	[Command("play", RunMode = Discord.Commands.RunMode.Async)]
	public async Task PlayAsync(string videoUrl)
	{
		//checking to see if we're in a channel
		var voiceChannel = (Context.User as IGuildUser)?.VoiceChannel;
		//if not, abort
		if (voiceChannel == null)
		{
			await ReplyAsync("You must be in a voice channel to use this command.");
			await Logger.Log(LogSeverity.Info, $"Attempting to connect to voice channel", "Not in voice channel, aborting");
			return;
		}
		//if yes, connect
		JoinVoice();
		await Logger.Log(LogSeverity.Info, $"Attempting to connect to voice channel", "Successful");

		//create a youtubeclient object to handle youtube requests
		YoutubeClient youtubeClient = new YoutubeClient();
		await Logger.Log(LogSeverity.Info, $"get to create an object of youtube client " + youtubeClient.Videos.Streams.ToString(), "Successful");
		//get path to the audio file and convert from opus to pcm
		var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(videoUrl);
		var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
		var convertedAudioPath = ConvertWebMToPCM(audioStreamInfo.Url);
		await Logger.Log(LogSeverity.Info, convertedAudioPath, "Successful");
		
			//Play the audio in the Discord voice channel
			if (string.IsNullOrEmpty(convertedAudioPath))
			{
				await ReplyAsync("Failed to convert the audio file.");
				return;
			}
        try
        {
            // Play the PCM audio in the voice channel
            var audioClient = await voiceChannel.ConnectAsync();
        await Logger.Log(LogSeverity.Info, "Converted audio", "Successful");
        var audioOutStream = audioClient.CreatePCMStream(AudioApplication.Music);
        await Logger.Log(LogSeverity.Info, audioOutStream.CanRead.ToString(), "Successful");
        
			using (var audioFileStream = File.OpenRead(convertedAudioPath))
			{

				await audioFileStream.CopyToAsync(audioOutStream);
				await audioOutStream.FlushAsync();
			}
		}
		catch (Exception e)
		{
			await Logger.Log(LogSeverity.Error,e.Source,e.Message);
		}
	}

	private async void JoinVoice()
	{
		//try to connect
		await Logger.Log(LogSeverity.Info, $"Connecting to voice channel", "Attempting");
		IVoiceChannel channel = (Context.User as IVoiceState).VoiceChannel;
		IAudioClient client = await channel.ConnectAsync();

		await Logger.Log(LogSeverity.Info, $"Connecting to voice channel", "Successful");
		await ReplyAsync("Joined channel");
	}

	private string ConvertWebMToPCM(string webmFilePath)
	{
		var tempOutputPath = Path.GetTempFileName() + ".pcm";

		try
		{
			var ffmpegProcessStartInfo = new ProcessStartInfo
			{
				FileName = "ffmpeg",
				Arguments = $"-i \"{webmFilePath}\" -ac 2 -f s16le -ar 48000 \"{tempOutputPath}\"",
				UseShellExecute = false,
				CreateNoWindow = true
			};

			using (var ffmpegProcess = Process.Start(ffmpegProcessStartInfo))
			{
				ffmpegProcess.WaitForExit();
			}

			return tempOutputPath;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Failed to convert WebM to PCM: {ex.Message}");
			return null;
		}
	}
}

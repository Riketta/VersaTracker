using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace VersaTrackerBot
{
    class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private DiscordSocketClient _client;

        public static void Main(string[] args) => new Program().MainAsync(args).GetAwaiter().GetResult();

        public async Task MainAsync(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            LogManager.SetupLogger();

            string token = args[0];

            _client = new DiscordSocketClient();
            Bot.SetClient(_client);
            Bot.ConnectDB();

            _client.Log += Log;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += Bot.MessageReceivedAsync;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            //await _client.SetStatusAsync(UserStatus.Invisible);
            await _client.SetGameAsync("DEBUG", null, ActivityType.Playing);

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            logger.Info(msg.ToString(null, true, false));
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            logger.Info($"{_client.CurrentUser} is connected!");

            return Task.CompletedTask;
        }
    }
}

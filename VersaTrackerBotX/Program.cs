using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.WebSocket;

namespace VersaTrackerBotX
{
    class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static DiscordClient _client;

        public static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            LogManager.SetupLogger();

            logger.Info("Parsing arguments");
            string token = args[0];
            string database = "database.db";
            if (args.Length >= 2)
                database = args[1];

            logger.Info("Setting-up client configuration");
            DiscordConfiguration config = new DiscordConfiguration()
            {
                Token = token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = false,
            };
            _client = new DiscordClient(config);
            logger.Debug("Token: {0}", token);

            logger.Info("Setting-up bot");
            Bot.SetClient(_client);
            Bot.ConnectDB(database);

            logger.Info("Setting-up events");
            _client.DebugLogger.LogMessageReceived += Log;
            _client.Ready += Client_Ready;
            _client.ClientErrored += Client_ClientError;
            _client.MessageCreated += Bot.MessageReceivedAsync;

            logger.Info("Connecting");
            //_client.SetWebSocketClient<WebSocket4NetClient>();
            //_client.SetWebSocketClient<WebSocketSharpClient>();
            await _client.ConnectAsync();

            //await _client.SetStatusAsync(UserStatus.Invisible);
            //await _client("DEBUG", null, ActivityType.Playing);

            // Block this task until the program is closed.
            logger.Info("Initial jobs done");
            await Task.Delay(-1);
        }

        private static void Log(object sender, DebugLogMessageEventArgs e)
        {
            logger.Info(e.Message.ToString());
        }

        private static Task Client_Ready(ReadyEventArgs e)
        {
            logger.Info($"{_client.CurrentUser} is connected!");

            return Task.CompletedTask;
        }

        private static Task Client_ClientError(ClientErrorEventArgs e)
        {
            // let's log the details of the error that just 
            // occured in our client
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "ExampleBot", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }
    }
}

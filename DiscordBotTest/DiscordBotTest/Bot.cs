using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Discord.Commands;

namespace DiscordBotTest
{
    class Bot
    {
        public string Id { get; }

        private BotConfig _config;
        private DiscordSocketClient _discord;

        private CommandHandler _commandHandler;

        public Bot(string configFilePath)
        {
            using (StreamReader sr = new StreamReader(configFilePath))
            {
                _config = JsonConvert.DeserializeObject<BotConfig>(sr.ReadToEnd());

                Id = _config.Id;
                var discordConfig = new DiscordSocketConfig();
                discordConfig.MessageCacheSize = _config.MessageCacheSize;

                _discord = new DiscordSocketClient(discordConfig);
            }

            _commandHandler = new CommandHandler(_config, _discord);
        }

        public async void Start()
        {
            if (string.IsNullOrWhiteSpace(_config.Token))
            {
                throw new Exception("Token Is Required in config.json");
            }

            await _discord.LoginAsync(TokenType.Bot, _config.Token);
            await _discord.StartAsync();

            _discord.MessageReceived += OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            if (msg == null)
            {
                return;
            }
            if (msg.Author.Id == _discord.CurrentUser.Id)
            {
                return;
            }

            int argPos = 0;     // Check if the message has a valid command prefix
            if (msg.HasStringPrefix(_config.Prefix, ref argPos)/* || msg.HasMentionPrefix(_discord.CurrentUser, ref argPos)*/)
            {
                string response = _commandHandler.RunCommand(msg.Content);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    await msg.Channel.SendMessageAsync(response);
                }
            }
        }

        public void Stop()
        {
            _discord.LogoutAsync();
        }
    }
}

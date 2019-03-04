using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotTest
{
    public class BotConfig
    {
        public string Id { get; set; }
        public string Token { get; set; }
        public int MessageCacheSize { get; set; }
        public string Prefix { get; set; }
    }
}

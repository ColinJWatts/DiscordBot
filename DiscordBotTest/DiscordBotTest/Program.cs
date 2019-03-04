using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Bot bot = new Bot("config.json");
            bot.Start();
            while(Console.ReadLine() != "q") { }

            bot.Stop();
        }
    }
}

using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotTest
{
    public interface ICommand
    {
        void Init(BotConfig config, DiscordSocketClient discord, Dictionary<string, ICommand> commandDictionary);
        string Description { get; }
        string command(string[] args);
        void Finish();
    }

    public class Help : ICommand
    {
        private string _description = "!help: returns descriptions from all available commands";
        Dictionary<string, ICommand> _dict;

        public string Description { get { return _description; } }

        public string command(string[] args)
        {
            string response = "";

            foreach(KeyValuePair<string, ICommand> kvp in _dict)
            {
                response += (kvp.Value.Description + " \n");
            }
           
            return response;
        }

        public void Finish() {}

        public void Init(BotConfig config, DiscordSocketClient discord, Dictionary<string, ICommand> commandDictionary)
        {
            _dict = commandDictionary;
        }
    }

    class InsultConfig
    {
        public List<string> Descriptors;
        public List<string> Modifiers;
        public List<string> Nouns;
    }

    public class InsultGenerator : ICommand
    {
        private string _description = "!insult: generates a 3 word insult in the form of (Descriptor, Modifier, Noun) \n" +
            "    Example: Racist Fuck Goblin\n" +
            "!insult add [word type: Descriptor, Modifier, Noun] [wordToAdd]\n    this will add a word to the specfied list";
        public string Description { get { return _description; } }

        InsultConfig _config;
        DiscordSocketClient _discord;
        private readonly Random rnd = new Random();

        public string command(string[] args)
        {
            string response = "";

            if (args.Count() > 3 && args[1] == "add")
            {
                switch (args[2])
                {
                    case "Descriptor":
                        if (!_config.Descriptors.Contains(args[3]))
                        {
                            _config.Descriptors.Add(args[3]);
                        }
                        else
                        {
                            response = $"Can't add {args[3]} to the list you ";
                        }
                        break;
                    case "Modifier":
                        if (!_config.Modifiers.Contains(args[3]))
                        {
                            _config.Modifiers.Add(args[3]);
                        }
                        else
                        {
                            response = $"Can't add {args[3]} to the list you ";
                        }
                        break;
                    case "Noun":
                        if (!_config.Nouns.Contains(args[3]))
                        {
                            _config.Nouns.Add(args[3]);
                        }
                        else {
                            response = $"Can't add {args[3]} to the list you ";
                        }
                        break;
                    default:
                        response = $"Can't add {args[3]} to the list you ";
                        break;
                }
               
            }

            response += _config.Descriptors[rnd.Next(0,_config.Descriptors.Count())] + " ";
            response += _config.Modifiers[rnd.Next(0, _config.Modifiers.Count())] + " ";
            response += _config.Nouns[rnd.Next(0, _config.Nouns.Count())];

            return response;
        }

        public void Init(BotConfig config, DiscordSocketClient discord, Dictionary<string, ICommand> commandDictionary)
        {
            using (StreamReader sr = new StreamReader("insultConfig.json"))
            {
                _config = JsonConvert.DeserializeObject<InsultConfig>(sr.ReadToEnd());
                _discord = discord;
            }
        }

        public void Finish()
        {
            using (StreamWriter sw = new StreamWriter("insultConfig.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(sw, _config);
            }
        }
    }

    class CommandHandler
    {

        private Dictionary<string, ICommand> CommandDictionary = new Dictionary<string, ICommand>
        {
            {"help", new Help() },
            {"insult", new InsultGenerator() }
        };
        
        private BotConfig _config;
        private DiscordSocketClient _discord;

        public CommandHandler(BotConfig botConfig, DiscordSocketClient discord)
        {
            _config = botConfig;
            _discord = discord;

            foreach(ICommand c in CommandDictionary.Values.AsEnumerable())
            {
                c.Init(_config, _discord, CommandDictionary);
            }
        }

        ~CommandHandler()
        {
            foreach (ICommand c in CommandDictionary.Values.AsEnumerable())
            {
                c.Finish();
            }
        }

        public string RunCommand(string command)
        {

            command = command.Remove(0, _config.Prefix.Count()); //trim prefix

            string[] args = command.Split(' ');

            command = args[0];

            if (!CommandDictionary.ContainsKey(command))
            {
                return $"'{command}' is not a recognized operation, type '!help' for information on recognized commands"; 
            }

            return CommandDictionary[command].command(args);
        }
    }

}

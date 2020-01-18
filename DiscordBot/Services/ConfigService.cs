using DiscordBot.Entities;
using Newtonsoft.Json;
using System.IO;

namespace DiscordBot.Services
{
	class ConfigService
	{
		public Config GetConfig()
		{
			var file = "Config.json";
			var data = File.ReadAllText(file);
			return JsonConvert.DeserializeObject<Config>(data);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Client;

namespace DiscordBot.Services
{
    class TwitchService
    {
        private readonly TwitchClient _twitchClient;

        TwitchService()
        {
            _twitchClient = new TwitchClient();
            
        }
        
    }
}

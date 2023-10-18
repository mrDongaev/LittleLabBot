using littlelab_bot.MainEntities;
using littlelab_bot.DataIntegration;
using Telegram.Bot.Types.ReplyMarkups;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using littlelab_bot.ButtonDesigner;
using littlelab_bot.BagWithAnimations;

namespace littlelab_bot.Dictionaries
{
    public static class DictionariesInMemory
    {
        public static ConcurrentDictionary<string, LabUser> dictUsers = new ConcurrentDictionary<string, LabUser>();

        public static ConcurrentDictionary<string, Request> dictRequests = new ConcurrentDictionary<string, Request>();

        public static ConcurrentDictionary<string, Sample> dictSamples = new ConcurrentDictionary<string, Sample>();

        public static ConcurrentDictionary<string, DisplaySymbols> dictDisplaySymbols = new ConcurrentDictionary<string, DisplaySymbols>();
    }
}            
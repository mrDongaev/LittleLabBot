using System;
using System.Threading;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
//using Telegram.Bots.Http;
using littlelab_bot;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json;
using Telegram.Bot.Types.ReplyMarkups;
using littlelab_bot.ButtonDesigner;
using System.Runtime.CompilerServices;
using littlelab_bot.DataBase;
using littlelab_bot.MainEntities;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace littlelab_bot
{
    public class LaunchPadBot
    {
        private static readonly ITelegramBotClient bot = new TelegramBotClient("6347452150:AAEJ2yBMWPS5ZOPsNQyezMK-7YzHxjEGKsY");
        public static void StartBot()
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();

            var cancellationToken = cts.Token;

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };
            bot.StartReceiving(

                HandleUpdateAsync,

                HandleErrorAsync,

                receiverOptions,

                cancellationToken
            );
            Console.ReadLine();
        }
        /// <summary>
        /// Метод который обрабатывает асинхронное обновление бота, максимально упрощаю в нем логику, чтобы не запутать самого себя
        /// </summary>
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var Tgid = await ReturnFromIdAsync(update, cancellationToken);

            LabUser labUser = new();

            await labUser.UserInitAndUpdateAsync(Tgid, botClient, update);

            switch (update.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.Message:

                    await labUser.WordProcessAsync(botClient, update);

                    break;

                case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:

                    var callBQ = update.CallbackQuery;

                    await labUser.CallbackProcessAsync(botClient, update, callBQ);

                    break;

                default:
                    return;
            }
        }
        public static async Task<string?> ReturnFromIdAsync(Update update, CancellationToken cancellationToken)
        {
            switch (update.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.Message:

                    return update.Message?.From.Id.ToString();

                case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:

                    return update.CallbackQuery?.From.Id.ToString();

                default:
                    return null;
            }
        }
        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }
    }
}

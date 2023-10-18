using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Xml.Linq;
using static littlelab_bot.BagWithAnimations.StaticPhrase;

namespace littlelab_bot.ButtonDesigner
{
    public class CommonButtons
    {
        /// <summary>
        /// Строим клавиатуру с выбором должности, при первом старте
        /// </summary>
        public static async Task ConstructMenuJobChoice(ITelegramBotClient botClient, Update update)
        {
            InlineKeyboardMarkup inlineKeyboard = new(
               new[] {
                   new[] {
                       InlineKeyboardButton.WithCallbackData("Я заказчик", _client)
                   },
                   new[] {
                       InlineKeyboardButton.WithCallbackData("Я химик", _labworker)
                   }
                   });
            await botClient.SendTextMessageAsync(await ReturnChatTypeAsync(update), _roleQuestion , replyMarkup: inlineKeyboard);
        }
        public static async Task ConstructButtonAsync(ITelegramBotClient botClient, Update update, string command, string message)
        {
            ReplyKeyboardMarkup keyBoard = new(

                new KeyboardButton[] { command
                }
            )
            { ResizeKeyboard = true };

            await botClient.SendTextMessageAsync(await ReturnChatTypeAsync(update), message, replyMarkup: keyBoard);
        }
        public static async Task<Chat> ReturnChatTypeAsync(Update update) 
        {
            switch (update.Type) 
            {
                case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:

                    return update.CallbackQuery.Message.Chat;

                default:
                    return update.Message.Chat;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using static littlelab_bot.Dictionaries.DictionariesInMemory;
using static littlelab_bot.BagWithAnimations.StaticPhrase;
using System.Reflection;
using littlelab_bot.ButtonDesigner;
using littlelab_bot.BagWithAnimations;
using System.Windows.Forms;
using littlelab_bot.DataBase;
using littlelab_bot.MainEntities;

namespace littlelab_bot.DataIntegration
{
    public class Request
    {
        public enum ActivityItem { inactive, active }
        public enum ActivityRequest { saved, sent, atwork, completed }
        public ActivityItem StatusSelectIdentProb { get; set; }
        public ActivityItem StatusSelectMaterial { get; set; }
        public ActivityItem StatusSelectTests { get; set; }
        public int Id { get; set; }
        public ActivityRequest StatusRequest { get; set; }
        public DateTime Dtsendrequest { get; set; }
        public string Materialname { get; set; }
        public string Marking { get; set; }
        public string[] Testsnames { get; set; }
        public string Startinfo { get; set; }

        public async Task MaterialSelectAsync(string tgid, CallbackQuery callback, ITelegramBotClient botClient, Update update)
        {
            var message = callback.Data;

            if (message != "/сompletemateriaselect")
            {
                var index = int.Parse(message);

                dictDisplaySymbols[tgid].ClearItemsMaterialsAsync();

                if (dictRequests[tgid]?.Materialname == _arrMaterials[index])
                {
                    dictRequests[tgid].Materialname = null;
                }
                else
                {
                    dictRequests[tgid].Materialname = _arrMaterials[index];

                    dictDisplaySymbols[tgid].SetDisplayMaterialsAsync(index, "\u2705");
                }
                await dictDisplaySymbols[tgid].UpdateInlineMenuAsync();

                await botClient.EditMessageTextAsync(await CommonButtons.ReturnChatTypeAsync(update), messageId: callback.Message.MessageId, _choosematerial,
                    replyMarkup: dictDisplaySymbols[tgid].inlineKeyboardMarkups["choosemat"]);

                await botClient.AnswerCallbackQueryAsync(callback.Id, _addedToTemplate);
            }
            else 
            {
                dictRequests[tgid].StatusSelectMaterial = ActivityItem.inactive;

                await botClient.EditMessageTextAsync(await CommonButtons.ReturnChatTypeAsync(update), messageId: callback.Message.MessageId, _enterYourApplicationDetails,
                    replyMarkup: dictDisplaySymbols[tgid].inlineKeyboardMarkups["makerequest"]);
            }
        }
        public async Task TestsSelectAsync(string tgid, CallbackQuery callback, ITelegramBotClient botClient, Update update) 
        {
            var message = callback.Data;

                if (message != "/сompletetests")
                {
                    var index = int.Parse(message);

                    if (Testsnames[index] == _arrTests[index])
                    {
                        dictDisplaySymbols[tgid].ClearItemsTestsAsync(index);

                        Testsnames[index] = null;
                    }
                    else
                    {
                        dictRequests[tgid].Testsnames[index] = _arrTests[index];

                        dictDisplaySymbols[tgid].SetDisplayTestsAsync(index, "\u2705");
                    }
                await dictDisplaySymbols[tgid].UpdateInlineMenuAsync();

                await botClient.EditMessageTextAsync(await CommonButtons.ReturnChatTypeAsync(update), messageId: callback.Message.MessageId, _selectIndicators,
                        replyMarkup: dictDisplaySymbols[tgid].inlineKeyboardMarkups["choosetests"]);

                await botClient.AnswerCallbackQueryAsync(callback.Id, _addedToTemplate);
                }
                else
                {
                    dictRequests[tgid].StatusSelectTests = ActivityItem.inactive;

                await botClient.EditMessageTextAsync(await CommonButtons.ReturnChatTypeAsync(update), messageId: callback.Message.MessageId, _enterYourApplicationDetails,
                        replyMarkup: dictDisplaySymbols[tgid].inlineKeyboardMarkups["makerequest"]);
                }
        }
        public async Task InitEmptyRequestAsync(LabUser labUser, ITelegramBotClient botClient, Update update)
        {
            RequestDBOperations requestDB = new();
            Testsnames = new string[_arrTests.Length];

            await requestDB.RequestAddTableAsync(labUser, this);

            DisplaySymbols displaySymbols = new();

            await displaySymbols.UpdateInlineMenuAsync();

            await botClient.SendTextMessageAsync(await CommonButtons.ReturnChatTypeAsync(update), $"Вносите данные по заявке",
    replyMarkup: displaySymbols.inlineKeyboardMarkups["makerequest"]);
        }
    }
}

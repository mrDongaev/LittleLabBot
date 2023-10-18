using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using static littlelab_bot.Dictionaries.DictionariesInMemory;
using static littlelab_bot.BagWithAnimations.StaticPhrase;
using littlelab_bot.DataIntegration;
using littlelab_bot.ButtonDesigner;
using littlelab_bot.BagWithAnimations;
using System.Runtime.InteropServices;
using littlelab_bot.DataBase;

namespace littlelab_bot.MainEntities
{
    public class Customer
    {
        LabUser LabUser { get; set; }

        Request Request { get; set; }

        DisplaySymbols DisplayS { get; set; }

        readonly ITelegramBotClient _botClient;

        readonly Update _update;
        public Customer(LabUser labUser, ITelegramBotClient botClient, Update update)
        {
            //Проводим валидацию при инициализации полей, чтобы исключить возможный null
            _update = update ?? throw new ArgumentNullException(paramName: nameof(update));

            _botClient = botClient ?? throw new ArgumentNullException(paramName: nameof(_botClient));

            LabUser = labUser ?? throw new ArgumentNullException(paramName: nameof(labUser));

            if (dictRequests.ContainsKey(LabUser.Tgid)) { Request = dictRequests?[LabUser.Tgid]; }

            if (dictDisplaySymbols.ContainsKey(LabUser.Tgid)) { DisplayS = dictDisplaySymbols[LabUser.Tgid]; }
        }
        public async Task RunCustomerCommandAsync() 
        {
            switch (_update.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.Message:

                    await CustomerMessageCommandAsync();

                    break;
                case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:

                    await CustomerCallbackQueryAsync(_update.CallbackQuery);

                    break;
                default:
                    break;
            }
        }
        public async Task CustomerMessageCommandAsync() 
        {
            var message = _update?.Message.Text;

            var messageId = _update.Message.MessageId;

            var chat = _update?.Message.Chat;

            switch (message)
            {
                case "/start":

                    await _botClient.SendTextMessageAsync(_update.Message.Chat, $"Привет {LabUser.Tgname}, начинаем работу!");

                    await CommonButtons.ConstructButtonAsync(_botClient, _update, "/makerequest", _startFillingOutApp);

                    break;

                case "/makerequest":

                    dictRequests.AddOrUpdate(LabUser.Tgid, (key) => new(), (key, oldValue) => new());

                    dictDisplaySymbols.AddOrUpdate(LabUser.Tgid, (key) => new(), (key, oldValue) => new());

                    Request = dictRequests[LabUser.Tgid];

                    await Request.InitEmptyRequestAsync(LabUser, _botClient, _update);

                    break;

                case "/finishenteringid":

                    await CommonButtons.ConstructButtonAsync(_botClient, _update, "/makerequest", _idAdded);

                    break;

                default:

                        if (Request?.StatusSelectIdentProb == Request.ActivityItem.active)
                        {
                            Request.Marking = message;
                        }
                    break;
            }
        }
        public async Task CustomerCallbackQueryAsync(CallbackQuery callback) 
        {
            var chat = _update.CallbackQuery.Message.Chat;

            if (dictRequests.ContainsKey(LabUser.Tgid))
            {
                NotifGenerator notif = new(LabUser, _botClient);

                switch (callback.Data)
                {
                    case "/enteridentprob":

                        Request.StatusSelectIdentProb = Request.ActivityItem.active;

                        await CommonButtons.ConstructButtonAsync(_botClient, _update, "/finishenteringid", _enterId);

                        break;
                    case "/choosemat":

                        Request.StatusSelectMaterial = Request.ActivityItem.active;

                        await DisplayS.UpdateInlineMenuAsync();

                        await _botClient.EditMessageTextAsync(chat, messageId: callback.Message.MessageId, $"Выберите материал",
                            replyMarkup: DisplayS.inlineKeyboardMarkups["choosemat"]);
                        break;

                    case "/choosetests":

                        Request.StatusSelectTests = Request.ActivityItem.active;

                        await DisplayS.UpdateInlineMenuAsync();

                        await _botClient.EditMessageTextAsync(chat, messageId: callback.Message.MessageId, $"Выберите показатели",
                            replyMarkup: DisplayS.inlineKeyboardMarkups["choosetests"]);

                        break;

                    case "/saverequest":

                        await notif.ChangeSendingStatusAsync(callback, Request.ActivityRequest.saved, _saveRequest);

                        break;
                    case "/sendrequest":

                        await notif.ChangeSendingStatusAsync(callback, Request.ActivityRequest.sent, _sentToTheLaboratory);

                        await notif.IntroInfoForLab(callback);


                        break;

                    default:

                        if (Request.StatusSelectMaterial == Request.ActivityItem.active)
                        {
                            await Request.MaterialSelectAsync(LabUser.Tgid, callback, _botClient, _update);
                        }
                        if (Request.StatusSelectTests == Request.ActivityItem.active)
                        {
                            await Request.TestsSelectAsync(LabUser.Tgid, callback, _botClient, _update);
                        }
                        break;
                }
            }
            else 
            {
                await _botClient.SendTextMessageAsync(chat, _noRequestsToFill);
            }
        }
    }
}

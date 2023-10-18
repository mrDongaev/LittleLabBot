using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using static littlelab_bot.Dictionaries.DictionariesInMemory;
using static littlelab_bot.BagWithAnimations.StaticPhrase;
using static littlelab_bot.ButtonDesigner.CommonButtons;
using littlelab_bot.ButtonDesigner;
using littlelab_bot.BagWithAnimations;
using System.Runtime.InteropServices;
using littlelab_bot.DataBase;
using littlelab_bot.DataIntegration;

namespace littlelab_bot.MainEntities
{
    public class Chemist
    {
        LabUser LabUser { get; set; }

        Sample SampleInfo { get; set; }
        
        readonly ITelegramBotClient _botClient;

        readonly Update _update;
        public Chemist(LabUser labUser, ITelegramBotClient botClient, Update update)
        {
            //Проводим валидацию при инициализации полей, чтобы исключить возможный null
            _update = update ?? throw new ArgumentNullException(paramName: nameof(update));

            _botClient = botClient ?? throw new ArgumentNullException(paramName: nameof(_botClient));

            LabUser = labUser ?? throw new ArgumentNullException(paramName: nameof(labUser));

            if (dictSamples.ContainsKey(LabUser.Tgid)) { SampleInfo = dictSamples[LabUser.Tgid]; }
        }
        public async Task RunChemistCommandAsync() 
        {
            switch (_update.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.Message:

                    await ChemistMessageCommandAsync();

                    break;
                case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:

                    await ChemistCallbackQueryAsync(_update.CallbackQuery);

                    break;
                default:
                    break;
            }
        }
        public async Task ChemistMessageCommandAsync() 
        {
            var message = _update?.Message.Text;

            var messageId = _update.Message.MessageId;

            var chat = _update?.Message.Chat;

            switch (message)
            {
                case "/start":

                    await _botClient.SendTextMessageAsync(await ReturnChatTypeAsync(_update), $"Привет {LabUser.Tgname}, начинаем работу!");

                    await CommonButtons.ConstructButtonAsync(_botClient, _update, "/checkactiverequest", _expectApplications);

                    break;

                default:

                    break;
            }
        }
        public async Task ChemistCallbackQueryAsync(CallbackQuery callback) 
        {
            var chat = _update.CallbackQuery.Message.Chat;

            await Console.Out.WriteLineAsync(callback.InlineMessageId);

            dictDisplaySymbols.AddOrUpdate(LabUser.Tgid, (key) => new(), (key, oldValue) => new());

            await dictDisplaySymbols[LabUser.Tgid].UpdateInlineMenuAsync();

            if (callback.Data.Contains("/acceptrequest"))
            {
                  try
                  {
                    dictSamples.AddOrUpdate(LabUser.Tgid, (key) => new(), (key, oldValue) => new());

                    SampleInfo = dictSamples[LabUser.Tgid];

                    await SampleInfo.SampleAcceptanceAsync(LabUser, _botClient, _update, callback);

                    SampleInfo.ValuesInd = new string[SampleInfo.NamesInd.Length];

                    SampleInfo.Status = Sample.ActivitySample.inputind;

                    await _botClient.EditMessageTextAsync(await ReturnChatTypeAsync(_update), callback.Message.MessageId, SampleInfo.AllTextRequest,
                          replyMarkup: dictDisplaySymbols[LabUser.Tgid].inlineKeyboardMarkups["selectindicator"]);
                  }
                  catch (NullReferenceException ex)
                  {
                    await _botClient.SendTextMessageAsync(await ReturnChatTypeAsync(_update), _errorForOutputSample + "\n" + ex.ToString());
                      throw ex;
                  }
            }
            else
            {
                if (SampleInfo != null)
                {
                    switch (callback.Data)
                    {
                        case "/checkactiverequest":

                            //dictRequests[TgId].StatusSelectIdentProb = Request.ActivityItem.active;

                            //await CommonButtons.ConstructButtonAsync(_botClient, _update, "/finishenteringid", StaticPhrase._enterId);

                            break;
                        case "/endinputandsend":

                            SampleDBOperations sampleDB = new();

                            NotifGenerator notif = new(LabUser, _botClient);

                            await notif.TextSampleGenerationAndSendingAsync(SampleInfo, _update);

                            await sampleDB.UpdateSampleIndValuesInDbAsync(SampleInfo);

                            await _botClient.EditMessageTextAsync(await ReturnChatTypeAsync(_update), callback.Message.MessageId, SampleInfo.AllTextRequest + _sampleAndApplicationAreClosed);

                            break;

                        default:

                            if (SampleInfo.Status == Sample.ActivitySample.inputind)
                            {
                                await IndicatorSelection(callback);
                            }
                            if (SampleInfo.Status == Sample.ActivitySample.inputvalue)
                            {
                                await EnteringValue(callback);
                            }
                            break;
                    }
                }
                else 
                {
                    await _botClient.SendTextMessageAsync(await CommonButtons.ReturnChatTypeAsync(_update), _errorForOutputSample);
                }
            }
        }
        public async Task IndicatorSelection(CallbackQuery callback) 
        {
            var displaySymb = dictDisplaySymbols[LabUser.Tgid];

            var nameFromTemplate = displaySymb.ArrDisplayTestsForChemist[int.Parse(callback.Data)];

            var list = SampleInfo.NamesInd.ToList();

            if (list.Contains(nameFromTemplate))
            {
                var index = list.IndexOf(nameFromTemplate);

                if (SampleInfo.NamesInd[index] == nameFromTemplate) 
                {
                    SampleInfo.Status = Sample.ActivitySample.inputvalue;

                    SampleInfo.CurrentInputIndex = index;

                    await _botClient.EditMessageTextAsync(await CommonButtons.ReturnChatTypeAsync(_update), callback.Message.MessageId, SampleInfo.AllTextRequest,
                        replyMarkup: dictDisplaySymbols[LabUser.Tgid].inlineKeyboardMarkups["enternumbers"]);
                }
            }
            else
            {
                await _botClient.AnswerCallbackQueryAsync(callback.Id, _notRequired);
            }
        }
        public async Task EnteringValue(CallbackQuery callback) 
        {
            var displayS = dictDisplaySymbols[LabUser.Tgid];

            var list = displayS.ArrDisplayTestsForChemist;

            if (!list.Contains(callback.Data)) {

                if (!SampleInfo.FirstIterationWithNumbers)
                {
                    SampleInfo.FirstIterationWithNumbers = true;
                }
                else
                {
                    var index = SampleInfo.CurrentInputIndex;

                    switch (callback.Data)
                    {
                        case "/savenumber":

                            SampleInfo.Status = Sample.ActivitySample.inputind;

                            SampleInfo.FirstIterationWithNumbers = false;

                            await _botClient.EditMessageTextAsync(await ReturnChatTypeAsync(_update), callback.Message.MessageId, SampleInfo.AllTextRequest,
                                replyMarkup: dictDisplaySymbols[LabUser.Tgid].inlineKeyboardMarkups["selectindicator"]);

                            break;
                        case "/erase":

                            var len = SampleInfo.ValuesInd[index].Length;

                            if (len != 0)
                            {
                                var eraseStr = SampleInfo.ValuesInd[index].Substring(0, SampleInfo.ValuesInd[index].Length - 1);

                                SampleInfo.ValuesInd[index] = eraseStr;

                                await _botClient.AnswerCallbackQueryAsync(callback.Id, SampleInfo.ValuesInd[index]);
                            }
                            break;

                        default:

                            var val = callback.Data;

                            SampleInfo.ValuesInd[index] += val;

                            await _botClient.AnswerCallbackQueryAsync(callback.Id, SampleInfo.ValuesInd[index]);

                            break;
                    }
                }
            }
        }

    }
}

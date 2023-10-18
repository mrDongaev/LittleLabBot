using littlelab_bot.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using littlelab_bot.MainEntities;
using static littlelab_bot.Dictionaries.DictionariesInMemory;
using static littlelab_bot.BagWithAnimations.StaticPhrase;
using littlelab_bot.DataIntegration;
using Telegram.Bot.Types;
using Telegram.Bot;
using Azure.Core;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;

namespace littlelab_bot.DataIntegration
{
    public class NotifGenerator
    {
        LabUser LabUser { get; set; }

        Request Request { get; set; }

        readonly ITelegramBotClient _botClient;

        public NotifGenerator(LabUser labUser, ITelegramBotClient botClient)
        {
            _botClient = botClient ?? throw new ArgumentNullException(paramName: nameof(_botClient));

            LabUser = labUser ?? throw new ArgumentNullException(paramName: nameof(LabUser));

            if (dictRequests.ContainsKey(LabUser.Tgid)) { Request = dictRequests?[LabUser.Tgid]; }
        }
        public async Task<int> ChangeSendingStatusAsync(CallbackQuery callback, Request.ActivityRequest status, string message) 
        {
            await ChangeRequestInDB(status);

            await _botClient.AnswerCallbackQueryAsync(callback.Id, message);

            return Request.Id;
        }
        public async Task IntroInfoForLab(CallbackQuery callback)
        {
            RequestDBOperations requestDB = new();

            await requestDB.ReadRequestFromDataBaseAsync(Request.Id);

            var chemists = await UserDBOperations.UnloadUsersAsync(_labworker);

            await TextRequestGenerationAndSending(chemists);

            await _botClient.AnswerCallbackQueryAsync(callback.Id, _sentToTheLaboratory);
        }
        public async Task ChangeRequestInDB(Request.ActivityRequest status) 
        {
            RequestDBOperations requestDB = new();

            Request.StatusRequest = status;

            var tempRef = Request;

            await requestDB.ReadRequestFromDataBaseAsync(Request.Id);

            if (Request == null)
            {
                Request = tempRef;

                await requestDB.RequestAddTableAsync(LabUser, Request);
            }
            else
            {
                Request = tempRef;

                await requestDB.RequestUpdateAsync(LabUser, Request);
            }
        }
        public async Task TextRequestGenerationAndSending(IEnumerable<LabUser> chemists)
        {
            string tn = string.Empty;

            RequestDBOperations requestDBOperations = new();

            foreach (var item in Request.Testsnames)
            {
                tn += item + ";";
            }
            Request.Dtsendrequest = DateTime.Now;

            var dt = Request.Dtsendrequest;

            await requestDBOperations.UpdateDateTimeInRequestAsync(Request.Id, dt);

            var reqId = Request.Id;

            var userId = LabUser.Id;

            var text =
                $"Идентификатор заявки: {Request.Id} \n" +
                $"Дата и время отправки: {Request.Dtsendrequest} \n" +
                $"От кого: {LabUser.Fullname} \n" +
                $"Маркировка пробы: {Request.Marking} \n" +
                $"Материал: {Request.Materialname} \n" +
                $"Показатели на определение: {tn} \n";

            await requestDBOperations.AddInfoRequestAsync(reqId.ToString(), text);

            InlineKeyboardMarkup inlineKeyboard = new(new[] {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(_acceptRequest, $"/acceptrequest;{reqId};{userId}")
            } } );

            foreach (var chemist in chemists)
            {
                await _botClient.SendTextMessageAsync(chemist.Tgid, text,
                    replyMarkup: inlineKeyboard);
            }
        }
        public async Task TextSampleGenerationAndSendingAsync(Sample sample, Update update) 
        {
            RequestDBOperations requestDB = new();

            UserDBOperations userDB = new(_botClient, update);

            string strResult = null;

            for (int i = 0; i < sample.NamesInd.Length; i++)
            {
                strResult += sample.NamesInd[i] + " = " + sample.ValuesInd[i] + "\n";
            }
            var labUser = await userDB.ReadUserFromDataBaseAsync(sample.CustomerId);

            var request = await requestDB.ReadRequestFromDataBaseAsync(sample.ReqId);

            await requestDB.UpdateRequestResultAsync(sample);

            await requestDB.UpdateStatusRequestAsync(sample.ReqId, Request.ActivityRequest.completed);

            var text =
            $"Заявка: {sample.ReqId} \n" +
            $"Материал: {request.Materialname} \n" +
            $"Маркировка и код: {sample.Code} \n ----------------------------\n" +
            $"Результаты: \n{strResult}";

            await _botClient.SendTextMessageAsync(labUser.Tgid, text);
        }
    }
}

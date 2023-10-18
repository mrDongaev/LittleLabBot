using littlelab_bot.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using static littlelab_bot.Dictionaries.DictionariesInMemory;
using static littlelab_bot.ButtonDesigner.CommonButtons;
using static littlelab_bot.BagWithAnimations.StaticPhrase;
using littlelab_bot.MainEntities;
using static littlelab_bot.DataIntegration.Request;
using System.Buffers;
using littlelab_bot.ButtonDesigner;
using Telegram.Bot;

namespace littlelab_bot.DataIntegration
{
    public class Sample
    {
        public enum ActivitySample { processed, inputind, inputvalue }
        public ActivitySample Status { get; set; }
        public bool FirstIterationWithNumbers { get; set; }
        public int CurrentInputIndex { get; set; }
        public int Id { get; set; }
        public int ReqId { get; set; }
        public int UserId { get; set; }
        public int CustomerId { get; set; }
        public string Code { get; set; }
        public string[] NamesInd { get; set; }
        public string[] ValuesInd { get; set; }
        public string AllTextRequest { get; set; }

        //Метод для принятия заявки, парсим ид запроса и ид юзера, вызываем создание пробы
        public async Task SampleAcceptanceAsync(LabUser labUser, ITelegramBotClient botClient, Update _update, CallbackQuery callback)
        {
            SampleDBOperations sampleDB = new();

            RequestDBOperations requestDB = new();

            string[] lines = callback.Data.Split(new char[] { ';' });

            //Для ИД в базе нет автоинкремента, поэтому инкрементируем его искусственно
            Id = await sampleDB.ReturnIdSampleAsync() + 1;

            ReqId = int.Parse(lines[1]);

            CustomerId = int.Parse(lines[2]);

            var atWork = ActivityRequest.atwork;

            await requestDB.UpdateStatusRequestAsync(ReqId, atWork);

            await SampleCreateAsync(labUser, botClient, _update);
        }
        //Метод для инициализации полей sample и создания пробы в БД
        public async Task SampleCreateAsync(LabUser labuser, ITelegramBotClient botClient, Update _update)
        {
            RequestDBOperations requestDB = new();

            SampleDBOperations sampleDB = new();

            var req = await requestDB.ReadRequestFromDataBaseAsync(ReqId);

            if (req != null) 
            {
                var arrayTests = await requestDB.ReturnPgArrayAsync(req);

                UserId = labuser.Id;

                Code = req.Marking;

                NamesInd = req.Testsnames;

                var tempRequest = await requestDB.GetInfoRequestAsync(req);

                AllTextRequest = tempRequest.Startinfo;

                await sampleDB.SampleAddInDBAsync(req, this, arrayTests);

                await sampleDB.UpdateDateTimeInSample(Id, DateTime.Now);
            }
            else 
            {
                await botClient.SendTextMessageAsync(await ReturnChatTypeAsync(_update), _errorForSample);
            }

        }
    }
}

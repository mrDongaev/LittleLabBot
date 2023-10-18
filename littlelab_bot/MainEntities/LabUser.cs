using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Data;
using Newtonsoft.Json.Linq;
using littlelab_bot.DataBase;
using littlelab_bot.ButtonDesigner;
using littlelab_bot.BagWithAnimations;
using System.Threading;
using System.Data.SqlClient;
using static littlelab_bot.Dictionaries.DictionariesInMemory;
using Telegram.Bot.Requests;

namespace littlelab_bot.MainEntities
{
    public class LabUser
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public string Fullname { get; set; }
        public string Phone { get; set; }
        public string Tgid { get; set; }
        public string Tgname { get; set; }
        public async Task UserInitAndUpdateAsync(string tgid, ITelegramBotClient botClient, Update update) 
        {
            UserDBOperations userDB = new(botClient, update);

            var labUser = await userDB.ReadUserFromDataBaseAsync(tgid);

            if (labUser == null)
            {
                Id = 0;

                Role = null;

                await AssignUserDataAsync(update, Role, Id);

                await UserDBOperations.UserAddTableAsync(this);
            }
            else 
            {
                await AssignUserDataAsync(update, labUser.Role, labUser.Id);

                await UserDBOperations.UpdateUserInitialsAsync(this);
            }
        }
        /// <summary>
        /// Обработка входящих комманд в виде строк
        /// </summary>
        public async Task WordProcessAsync(ITelegramBotClient botClient, Update update)
        {
            switch (update.Message.Text)
            {
                case "/start":

                    await UserHandingAsync(botClient, update);

                    break;

                default:

                    //Разделяю строковые команды на отдельные методы, срого по ролям
                    await CommandInitiatorDefinitionAsync(botClient, update);

                    break;
            }
        }
        /// <summary>
        /// Собирает все коллбэки
        /// </summary>
        public async Task CallbackProcessAsync(ITelegramBotClient botClient, Update update, CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data == StaticPhrase._labworker || callbackQuery.Data == StaticPhrase._client)
            {
                await RoleSelectionAsync(botClient, update, callbackQuery);
            }
            else
            {
                await CommandInitiatorDefinitionAsync(botClient, update);
            }
        }
        public async Task RoleSelectionAsync(ITelegramBotClient botClient, Update update, CallbackQuery callbackQuery)
        {
            if (Role == "")
            {
                Role = callbackQuery.Data;

                UserDBOperations userDB = new(botClient, update);

                await userDB.UpdateUserRoleAsync(this);

                await botClient.SendTextMessageAsync(await CommonButtons.ReturnChatTypeAsync(update), 
                    $"Добро пожаловать в Little Lab {Tgname}, теперь вы {Role}!");

                await ButtonSettingsAsync(botClient, update);
            }
            else
            {
                await botClient.SendTextMessageAsync(await CommonButtons.ReturnChatTypeAsync(update), 
                    $"Вы уже присвоена роль в Little Lab, {Tgname}, ваша роль {Role}!");
            }
        }
        /// <summary>
        /// Метод, который проверяет, пользователь с какой ролью отправляет команды и в зависимости от роли, перенаправляет поток в нужный метод
        /// </summary>
        public async Task CommandInitiatorDefinitionAsync(ITelegramBotClient botClient, Update update)
        {
            switch (Role)
            {
                case StaticPhrase._client:

                    Customer customer = new(this, botClient, update);

                    await customer.RunCustomerCommandAsync();
                    
                    break;
                case StaticPhrase._labworker:

                    Chemist chemist = new(this, botClient, update);

                    await chemist.RunChemistCommandAsync();

                    break;
                default:
                    break;
            }
        }
        public async Task UserHandingAsync(ITelegramBotClient botClient, Update update) 
        {
            await StaticStickers.MessageWithSticker(botClient, update, StaticPhrase._hello1, StaticStickers._helloDuck);

            //По цепочке обрабатываем и проверяем, существует ли пользователь в БД
            if (Role != null && Role != "")
            {
                await CommandInitiatorDefinitionAsync(botClient, update);

                return;
            }
            else 
            {
                await CommonButtons.ConstructMenuJobChoice(botClient, update);
            }
        }
        /// <summary>
        /// Отдельный метод, для того чтобы исключить ошибки при вызове конструктора User, т.к буду создавать обьект этого типа для выгрузки данных из БД
        /// AssignUserDataAsync нужен на случай хранения данных о пользователе в памяти
        /// </summary>
        public async Task AssignUserDataAsync(Update update, string role, int id)
        {
            Id = id;

            Role = role;

            switch (update.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.Message:

                    Fullname = update.Message.From.FirstName + " " + update.Message.From.LastName;

                    Tgid = update.Message?.From.Id.ToString();

                    Tgname = update.Message?.From.Username;

                    break;

                case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:

                    Fullname = update.CallbackQuery.From.FirstName + " " + update.CallbackQuery.From.LastName;

                    Tgid = update.CallbackQuery?.From.Id.ToString();

                    Tgname = update.CallbackQuery?.From.Username;

                    break;

                default:
                    return;
            }
        }
        public async Task ButtonSettingsAsync(ITelegramBotClient botClient, Update update) 
        {
            if (Role == StaticPhrase._client)
            {
                await CommonButtons.ConstructButtonAsync(botClient, update, "/makerequest", StaticPhrase._startFillingOutApp);
            }
            if (Role == StaticPhrase._labworker)
            {
                await CommonButtons.ConstructButtonAsync(botClient, update, "/checkactiverequest", StaticPhrase._expectApplications);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using static littlelab_bot.BagWithAnimations.StaticPhrase;

namespace littlelab_bot.BagWithAnimations
{
    public class DisplaySymbols
    {
        private string[] arrDisplayTestsForChemist = new string[] { "Bi", "Se", "Ni", "Te", "Zn", "Si", "Sn", "Co", "Закончить ввод и закрыть заявку" };

        private string[] arrDisplayTestsForCustomer = new string[] { "Bi", "Se", "Ni", "Te", "Zn", "Si", "Sn", "Co", "Сохранить и вернуться в меню" };

        private string[] arrNumbers = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", ".", "Стереть", "Сохранить" };
        public string[] ArrNumbers { get { return arrNumbers; } }
        public string[] ArrDisplayTestsForCustomer { get { return arrDisplayTestsForCustomer; } }
        public string[] ArrDisplayTestsForChemist { get { return arrDisplayTestsForChemist; } }

        public string[] arrDisplayMaterials = new string[] { "Медь", "Сохранить и вернуться в меню" };
        public string[] ArrDisplayMaterials { get {  return arrDisplayMaterials; } }

        readonly ITelegramBotClient _botClient;
        
        public Dictionary<string, InlineKeyboardMarkup> inlineKeyboardMarkups;

        public async Task UpdateInlineMenuAsync() 
        {
            inlineKeyboardMarkups = new Dictionary<string, InlineKeyboardMarkup>()
        {
            { "makerequest", new( new[] {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Ввести идентификатор пробы", "/enteridentprob")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Выбрать переданный материал", "/choosemat")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Выбрать необходимые показатели", "/choosetests")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Сохранить", "/saverequest"),

                        InlineKeyboardButton.WithCallbackData("Отправить", "/sendrequest")
                    }
            }) },
            { "choosemat", new( new[] {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(arrDisplayMaterials[0], "0"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(arrDisplayMaterials[1], "/сompletemateriaselect")
                    }
            }) },
            { "choosetests", new( new[] {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(arrDisplayTestsForCustomer[0], "0"), //Висмут

                        InlineKeyboardButton.WithCallbackData(arrDisplayTestsForCustomer[1], "1"), //Селен

                        InlineKeyboardButton.WithCallbackData(arrDisplayTestsForCustomer[2], "2"), //Никель

                        InlineKeyboardButton.WithCallbackData(arrDisplayTestsForCustomer[3], "3"), //Теллур
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(arrDisplayTestsForCustomer[4], "4"), //Цинк

                        InlineKeyboardButton.WithCallbackData(arrDisplayTestsForCustomer[5], "5"), //Кремний

                        InlineKeyboardButton.WithCallbackData(arrDisplayTestsForCustomer[6], "6"), //Олово

                        InlineKeyboardButton.WithCallbackData(arrDisplayTestsForCustomer[7], "7"), //Кобальт
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(arrDisplayTestsForCustomer[8], "/сompletetests")
                    }
            }) },
            { "acceptedapp", new( new[] {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(_acceptRequest, "/createsample")
                    }
            }) },
            { "selectindicator", new( new[] {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(arrDisplayTestsForChemist[0], "0"), //Висмут

                        InlineKeyboardButton.WithCallbackData(arrDisplayTestsForChemist[1], "1"), //Селен

                        InlineKeyboardButton.WithCallbackData(arrDisplayTestsForChemist[2], "2"), //Никель

                        InlineKeyboardButton.WithCallbackData(arrDisplayTestsForChemist[3], "3"), //Теллур
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(arrDisplayTestsForChemist[4], "4"), //Цинк

                        InlineKeyboardButton.WithCallbackData(arrDisplayTestsForChemist[5], "5"), //Кремний

                        InlineKeyboardButton.WithCallbackData(arrDisplayTestsForChemist[6], "6"), //Олово

                        InlineKeyboardButton.WithCallbackData(arrDisplayTestsForChemist[7], "7"), //Кобальт
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(arrDisplayTestsForChemist[8], "/endinputandsend")
                    }
            }) },
            { "enternumbers", new( new[] {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(arrNumbers[1], "1"),

                        InlineKeyboardButton.WithCallbackData(arrNumbers[2], "2"),

                        InlineKeyboardButton.WithCallbackData(arrNumbers[3], "3"),

                        InlineKeyboardButton.WithCallbackData(arrNumbers[4], "4"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(arrNumbers[5], "5"),

                        InlineKeyboardButton.WithCallbackData(arrNumbers[6], "6"),

                        InlineKeyboardButton.WithCallbackData(arrNumbers[7], "7"),

                        InlineKeyboardButton.WithCallbackData(arrNumbers[8], "8"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(arrNumbers[9], "9"),

                        InlineKeyboardButton.WithCallbackData(arrNumbers[0], "0"),

                        InlineKeyboardButton.WithCallbackData(arrNumbers[10], "."),

                        InlineKeyboardButton.WithCallbackData(arrNumbers[11], "/erase"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(arrNumbers[12], "/savenumber")
                    }
            }) }


        };
        }
        public void SetDisplayTestsAsync(int index, string symbol) 
        {
            arrDisplayTestsForCustomer[index] += symbol;
        }
        public void SetDisplayMaterialsAsync(int index, string symbol)
        {
            arrDisplayMaterials[index] += symbol;
        }
        public void ClearItemsMaterialsAsync() 
        {
            _arrMaterials.CopyTo(arrDisplayMaterials, 0);
        }
        public void ClearItemsTestsAsync(int index) 
        {
            arrDisplayTestsForCustomer[index] = _arrTests[index].ToString();
        }
    }
}

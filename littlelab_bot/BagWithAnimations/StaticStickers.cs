using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace littlelab_bot.BagWithAnimations
{
    public class StaticStickers
    {
        public readonly static string _helloDuck = "CAACAgIAAxkBAAPkZMf1b4SRP9_ushoBpF1POXWq-_4AAgEBAAJWnb0KIr6fDrjC5jQvBA";

        public static async Task MessageWithSticker(ITelegramBotClient botClient, Update update, string pharse, string stickers)
        {
            var cts = new CancellationTokenSource();

            var cancellationToken = cts.Token;

            await botClient.SendStickerAsync(update.Message.Chat.Id, sticker: InputFile.FromString(stickers), cancellationToken: cancellationToken);

            await botClient.SendTextMessageAsync(update.Message.Chat, pharse);
        }
    }
}

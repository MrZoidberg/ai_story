namespace AIStory.TelegramBotLambda.Helpers;

using global::Telegram.Bot.Types.ReplyMarkups;
using System;
using System.Collections.Generic;
using System.Linq;

public static class InlineKeyboardMarkupHelper
{
    public static InlineKeyboardMarkup CreateInlineKeyboardMarkup(Dictionary<string, string> items, short maxItemsPerLine = 5)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }
        if (maxItemsPerLine < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxItemsPerLine));
        }

        var lines = items.Select((item, index) => new { item, index })
            .GroupBy(x => x.index / maxItemsPerLine)
            .Select(x => x.Select(v => v.item).ToArray())
            .ToArray();
        var keyboardButtons = lines.Select(line => line.Select(item => InlineKeyboardButton.WithCallbackData(item.Value, item.Key)).ToArray()).ToArray();
        return new InlineKeyboardMarkup(keyboardButtons);
    }
}

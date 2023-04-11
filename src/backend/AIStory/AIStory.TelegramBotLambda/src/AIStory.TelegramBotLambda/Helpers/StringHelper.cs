namespace AIStory.TelegramBotLambda.Helpers;

using System;
using System.Collections.Generic;

public static class StringHelper
{
    public static IEnumerable<string> SplitWithMultipleSeparators(string text, string[] separators)
    { 
        var result = new List<string>(new[] {text});
        foreach (var separator in separators)
        {
            var splitted = new List<string>();
            foreach (var item in result)
            {
                splitted.AddRange(item.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries));
            }
            result = splitted;
        }

        return result.Select(i => i.Trim());
    }
}

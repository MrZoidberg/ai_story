namespace AIStory.StorySendTelegramLambda;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class LambdaOptions
{
    public string? StoriesTableName { get; set; }

    public string TelegramBotToken { get; set; }
}

// See https://aka.ms/new-console-template for more information


//using AIStory.StoryGenerationLambda;
using AIStory.SharedModels.Localization;
using AIStory.SharedModels.Models;
using AIStory.TelegramBotLambda;
using AIStory.TelegramBotLambda.Helpers;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using OpenAI_API;
using System.Text.Json;
using System.Text.Json.Serialization;
using Zoid.AIStory.SharedModels.Dto;


var message = new GenerateStoryMessage
{
    GenerateAudio = false,
    Id = Guid.NewGuid().ToString(),
    Language = "ru",
    Model = "gpt-3.5-turbo",
    StoryCharacters = new List<string>(new[] { "Стас", "Филипп" }),
    StoryLength = 200,
    StoryLocation = "в логове врага",
    StoryTheme = StoryTheme.Fairytale,
    ChatId = Guid.NewGuid().ToString()
};

JsonSerializerOptions jso = new JsonSerializerOptions();
jso.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
jso.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
/*
string jsonString = JsonSerializer.Serialize(message, jso);
Console.WriteLine(jsonString);

var aIRequestGenerator = (new AIRequestGeneratorFactory()).Create(message.Language);
var request = aIRequestGenerator.GenerateStoryRequest(message);
jsonString = JsonSerializer.Serialize(request, MessageJsonSerializerContext.Default.ChatRequest);
Console.WriteLine(jsonString);

Dictionary<string, Single> dict = new Dictionary<string, float>();
dict.Add("a", 1.0f);

jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(dict);
Console.WriteLine(jsonString);

Console.WriteLine(typeof(System.Collections.ObjectModel.ReadOnlyCollection<>).Assembly.FullName);
Console.WriteLine(typeof(OpenAI_API.Chat.ChatChoice).Assembly.FullName);
*/
Telegram.Bot.Types.Update update = new Telegram.Bot.Types.Update();
update.Message = new Telegram.Bot.Types.Message()
{
    Text = "/start",
    Chat = new Telegram.Bot.Types.Chat()
    {
        Id = 234234,
        Username = "mihmerk",
        FirstName = "Mikhail"
    },
    From = new Telegram.Bot.Types.User()
    {
        Username = "mihmerk",
        FirstName = "Mikhail",
        Id = 02346789234789,
        LanguageCode = "ru"
    }
};
var jsonString = JsonSerializer.Serialize(update, jso);
Console.WriteLine(jsonString);


var storytext = new Dictionary<int, string>();
storytext.Add(0, "На ферме, где росли огромные тыквы, жил Джек Потрошитель.");

StringResourceFactory stringResourceFactory = new StringResourceFactory();
stringResourceFactory.Language = "ru";
var text = string.Format(stringResourceFactory.StringResources.HereIsStoryFormat, string.Join(Environment.NewLine, storytext.Select(x => x.Value)));

var result = StringHelper.SplitWithMultipleSeparators("Коля, Маша и Дима", new[] { ",", " и " });

Console.ReadLine();

 
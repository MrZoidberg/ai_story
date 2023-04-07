// See https://aka.ms/new-console-template for more information


using AIStory.StoryGenerationLambda;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenAI_API;
using System.Text.Json;
using Zoid.AIStory.SharedModels;
using Zoid.AIStory.SharedModels.Dto;

var builder = new HostBuilder()
        .ConfigureServices((hostContext, services) =>
        {
            services.AddHttpClient(Options.DefaultName, client =>
            {
                client.Timeout = TimeSpan.FromSeconds(150);
            });
            services.AddSingleton<AIRequestGeneratorFactory>();
            services.AddSingleton(f =>
            {
                string? apiKey = Environment.GetEnvironmentVariable("OpenAI_API_KEY") ?? throw new InvalidOperationException("OpenAI_API_KEY environment variable is not set.");
                return new OpenAIAPI(apiKey)
                {
                    HttpClientFactory = f.GetRequiredService<IHttpClientFactory>()
                };
            });
        });

var host = builder.Build();

host.Services.GetRequiredService<IHttpClientFactory>().CreateClient();

var message = new GenerateStoryMessage
{
    GenerateAudio = false,
    Id = Guid.NewGuid().ToString(),
    Language = "ru",
    Model = "gpt-3.5-turbo",
    StoryCharacters = new List<string>(new[] { "Стас", "Филипп" }),
    StoryLength = 200,
    StoryLocation = "в логове врага",
    StoryTheme = "героический рассказ",
    UserId = Guid.NewGuid().ToString()
};

JsonSerializerOptions jso = new JsonSerializerOptions();
jso.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

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


Console.ReadLine();


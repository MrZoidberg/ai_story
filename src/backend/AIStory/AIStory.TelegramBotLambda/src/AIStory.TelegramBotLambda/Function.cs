namespace AIStory.TelegramBotLambda;

using AIStory.SharedConfiguration;
using AIStory.SharedModels.Localization;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Zoid.AIStory.SharedModels.Dto;

public class Function
{
    private static string EnvName;

    /// <summary>
    /// The main entry point for the Lambda function. The main function is called once during the Lambda init phase. It
    /// initializes the .NET Lambda runtime client passing in the function handler to invoke for each Lambda event and
    /// the JSON serializer to use for converting Lambda JSON format to the .NET types. 
    /// </summary>
    private static async Task Main()
    {
        EnvName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        Func<Stream, ILambdaContext, Task> handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler)
            .Build()
            .RunAsync();
    }

    /// <summary>
    /// To use this handler to respond to an AWS event, reference the appropriate package from 
    /// https://github.com/aws/aws-lambda-dotnet#events
    /// and change the string input parameter to the desired event type. When the event type
    /// is changed, the handler type registered in the main method needs to be updated and the LambdaFunctionJsonSerializerContext 
    /// defined below will need the JsonSerializable updated. If the return type and event type are different then the 
    /// LambdaFunctionJsonSerializerContext must have two JsonSerializable attributes, one for each type.
    ///
    /// When using Native AOT, libraries used with your Lambda function might not be compatible with trimming that
    /// happens as part of the Native AOT compilation. If you find when testing your Native AOT Lambda function that 
    /// you get runtime errors about missing types, methods or constructors then add the assembly that contains the
    /// types into the rd.xml file. This will tell the Native AOT compiler to not trim those assemblies. Currently the 
    /// AWS SDK for .NET does not support trimming and when used should be added to the rd.xml file.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task FunctionHandler(Stream stream, ILambdaContext context)
    {
        var builder = new HostBuilder()
            .ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder
                .AddAmazonSecretsManager(Amazon.RegionEndpoint.USEast1.SystemName, $"{EnvName}/AIStory/TelegramBotLambda")
                .AddEnvironmentVariables();
            })
         .ConfigureServices((hostContext, services) =>
         {
             services.AddHttpClient("telegram_bot_client")
              .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
              {
                  var options = new TelegramBotClientOptions(sp.GetRequiredService<LambdaOptions>().TelegramBotToken);
                  return new TelegramBotClient(options, httpClient);
              });
             services.AddSingleton<IAmazonDynamoDB>(f => new AmazonDynamoDBClient(Amazon.RegionEndpoint.USEast1));
             services.AddSingleton(f => context.Logger);
             services.AddSingleton<ChatMessageHandler>();
             services.AddSingleton<StringResourceFactory>();
             services.AddSingleton<Workflow.Workflow>();
             services.AddSingleton<UserChatRepository>();
             services.AddSingleton<IAmazonSQS>(f => new AmazonSQSClient(Amazon.RegionEndpoint.USEast1));
             services.AddSingleton<StoryBuilder>();
             services.AddSingleton(f => f.GetRequiredService<IOptions<LambdaOptions>>().Value);

             services.Configure<LambdaOptions>(hostContext.Configuration);
         });

        var host = builder.Build();

        using var streamReader = new StreamReader(stream);
        var input = await streamReader.ReadToEndAsync();
        context.Logger.LogInformation($"Received Message:");
        context.Logger.LogInformation(input);

        var update = Newtonsoft.Json.JsonConvert.DeserializeObject<Update>(input);

        if (update == null)
        {
            return;
        }

        var messageHandler = host.Services.GetRequiredService<ChatMessageHandler>();
        await messageHandler.ProcessMessage(update);
    }
}


[JsonSerializable(typeof(GenerateStoryMessage))]
public partial class MessageJsonSerializerContext : JsonSerializerContext
{
    // By using this partial class derived from JsonSerializerContext, we can generate reflection free JSON Serializer code at compile time
    // which can deserialize our class and properties. However, we must attribute this class to tell it what types to generate serialization code for.
    // See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-source-generation
}

namespace AIStory.StorySendTelegramLambda;

using AIStory.SharedConfiguration;
using AIStory.SharedModels.Localization;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;
using Telegram.Bot;

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

        Func<DynamoDBEvent, ILambdaContext, Task<string>> handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<LambdaFunctionJsonSerializerContext>())
            .Build()
            .RunAsync();
    }

    /// <summary>
    /// A simple function that takes a string and does a ToUpper.
    ///
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
    public static async Task<string> FunctionHandler(DynamoDBEvent input, ILambdaContext context)
    {
        var builder = new HostBuilder()
             .ConfigureAppConfiguration((_, configurationBuilder) =>
             {
                 configurationBuilder
                 .AddAmazonSecretsManager(Amazon.RegionEndpoint.USEast1.SystemName, $"{EnvName}/AIStory/StorySendTelegramLambda")
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
               services.AddSingleton<StringResourceFactory>();
               services.AddSingleton<StoriesRepository>();
               services.AddSingleton<EventProcessor>();
               services.AddSingleton(f => f.GetRequiredService<IOptions<LambdaOptions>>().Value);

               services.Configure<LambdaOptions>(hostContext.Configuration);
           });

        var host = builder.Build();

        var processor = host.Services.GetRequiredService<EventProcessor>();

        context.Logger.LogInformation($"Beginning to process {input.Records.Count} records from DynamoDBEvent...");

        foreach (var record in input.Records)
        {
            context.Logger.LogInformation($"Event ID: {record.EventID}");
            context.Logger.LogInformation($"Event Name: {record.EventName}");

            if (record.EventName != OperationType.INSERT)
            {
                context.Logger.LogInformation("Skipping non-INSERT event.");
                continue;
            }

            await processor.ProcessEvent(record);
        }

        context.Logger.LogInformation("Processing complete.");

        return $"Processed {input.Records.Count} events.";
    }
}

/// <summary>
/// This class is used to register the input event and return type for the FunctionHandler method with the System.Text.Json source generator.
/// There must be a JsonSerializable attribute for each type used as the input and return type or a runtime error will occur 
/// from the JSON serializer unable to find the serialization information for unknown types.
/// </summary>
[JsonSerializable(typeof(DynamoDBEvent))]
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext
{
    // By using this partial class derived from JsonSerializerContext, we can generate reflection free JSON Serializer code at compile time
    // which can deserialize our class and properties. However, we must attribute this class to tell it what types to generate serialization code for.
    // See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-source-generation
}
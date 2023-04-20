namespace AIStory.StoryGenerationLambda;

using AIStory.SharedConfiguration;
using AIStory.SharedModels.Localization;
using AIStory.StorySendTelegramLambda;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenAI_API;
using OpenAI_API.Chat;
using System.Text.Json.Serialization;
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

        Func<SQSEvent, ILambdaContext, Task<string>> handler = FunctionHandler;
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
    public static async Task<string> FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        var builder = new HostBuilder()
            .ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder
                .AddAmazonSecretsManager(Amazon.RegionEndpoint.USEast1.SystemName, $"{EnvName}/AIStory/StoryGenerationLambda")
                .AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHttpClient(Options.DefaultName, client =>
                {
                    // since OpenAI API is slow, we need to give it more time than the default 30 seconds. 
                    // we'll give it 10 seconds less than the remaining time of the AWS Lambda,
                    // so that we can still return a response to the user.
                    client.Timeout = context.RemainingTime - TimeSpan.FromSeconds(10);
                });
                services.AddSingleton<AIRequestGeneratorFactory>();
                services.AddSingleton(f =>
                {
                    return new OpenAIAPI(f.GetRequiredService<LambdaOptions>().OpenAIKey)
                    {
                        HttpClientFactory = f.GetRequiredService<IHttpClientFactory>()
                    };
                });
                
                services.AddSingleton< IAmazonDynamoDB>(f => new AmazonDynamoDBClient(Amazon.RegionEndpoint.USEast1));
                services.AddSingleton(f => context.Logger);
                services.AddSingleton<StringResourceFactory>();
                services.AddSingleton<MessageProcessor>();
                services.AddSingleton<StoriesRepository>();
                services.AddSingleton(f => f.GetRequiredService<IOptions<LambdaOptions>>().Value);

                services.Configure<LambdaOptions>(hostContext.Configuration);
            });

        var host = builder.Build();


        context.Logger.LogInformation($"Beginning to process {sqsEvent.Records.Count} records...");

        var processor = host.Services.GetRequiredService<MessageProcessor>();
        
        foreach (var record in sqsEvent.Records)
        {
            context.Logger.LogInformation($"Message ID: {record.MessageId}");
            context.Logger.LogInformation($"Event Source: {record.EventSource}");

            context.Logger.LogInformation($"Record Body:");
            context.Logger.LogInformation(record.Body);

            await processor.ProcessMessage(record);
        }

        context.Logger.LogInformation("Processing complete.");

        return $"Processed {sqsEvent.Records.Count} records.";
    }
}

/// <summary>
/// This class is used to register the input event and return type for the FunctionHandler method with the System.Text.Json source generator.
/// There must be a JsonSerializable attribute for each type used as the input and return type or a runtime error will occur 
/// from the JSON serializer unable to find the serialization information for unknown types.
/// </summary>
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(SQSEvent))]
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext
{
    // By using this partial class derived from JsonSerializerContext, we can generate reflection free JSON Serializer code at compile time
    // which can deserialize our class and properties. However, we must attribute this class to tell it what types to generate serialization code for.
    // See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-source-generation
}

[JsonSerializable(typeof(GenerateStoryMessage))]
[JsonSerializable(typeof(ChatRequest))]
[JsonSerializable(typeof(ChatMessage))]
[JsonSerializable(typeof(ChatMessageRole))]
[JsonSerializable(typeof(ChatResponseModel))]
[JsonSerializable(typeof(ChatResponseModel2))]
public partial class MessageJsonSerializerContext : JsonSerializerContext
{
    // By using this partial class derived from JsonSerializerContext, we can generate reflection free JSON Serializer code at compile time
    // which can deserialize our class and properties. However, we must attribute this class to tell it what types to generate serialization code for.
    // See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-source-generation
}
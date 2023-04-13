namespace AIStories.ApiGetStories;

using AIStories.ApiGetStories.Models;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;
using AIStory.SharedConfiguration;
using Microsoft.Extensions.Configuration;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Amazon.Runtime.Internal.Transform;

public class Function
{
    private static string EnvName;

    private static bool IsDevelopment => EnvName == "Development";

    /// <summary>
    /// The main entry point for the Lambda function. The main function is called once during the Lambda init phase. It
    /// initializes the .NET Lambda runtime client passing in the function handler to invoke for each Lambda event and
    /// the JSON serializer to use for converting Lambda JSON format to the .NET types. 
    /// </summary>
    private static async Task Main()
    {
        EnvName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        Func<APIGatewayHttpApiV2ProxyRequest, ILambdaContext, Task<APIGatewayHttpApiV2ProxyResponse>> handler = FunctionHandler;
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
    public static async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest input, ILambdaContext context)
    {
        try
        {
            var request = GetStoriesRequest.FromParameters(input.QueryStringParameters);
            if (request.PageSize == null || request.PageSize < 1 || request.PageSize > 100)
            {
                ProblemDetails problemDetails = new ProblemDetails()
                {
                    Detail = "Invalid page size",
                    Status = 400,
                    Title = "Invalid request"
                };

                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    StatusCode = 400,
                    Body = JsonSerializer.Serialize(problemDetails, LambdaFunctionJsonSerializerContext.Default.ProblemDetails),
                };
            }
            if (!Constants.SupportedLanguages.Contains(request.Language))
            {
                ProblemDetails problemDetails = new ProblemDetails()
                {
                    Detail = "Language is not supported",
                    Status = 400,
                    Title = "Invalid request"
                };

                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    StatusCode = 400,
                    Body = JsonSerializer.Serialize(problemDetails, LambdaFunctionJsonSerializerContext.Default.ProblemDetails),
                };
            }


            var builder = new HostBuilder()
                .ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    configurationBuilder
                    .AddAmazonSecretsManager(Amazon.RegionEndpoint.USEast1.SystemName, $"{EnvName}/AIStory/ApiGetStories")
                    .AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IAmazonDynamoDB>(f => new AmazonDynamoDBClient(Amazon.RegionEndpoint.USEast1));
                    services.AddSingleton(f => context.Logger);
                    services.AddSingleton<StoriesRepository>();
                    services.AddSingleton(f => f.GetRequiredService<IOptions<LambdaOptions>>().Value);

                    services.Configure<LambdaOptions>(hostContext.Configuration);
                });

            var host = builder.Build();

            var storiesRepository = host.Services.GetRequiredService<StoriesRepository>();

            var page = await storiesRepository.GetStoriesPages(request.LastEvaluatedKey, request.PageSize!.Value, request.Language!);

            JsonSerializerOptions jso = new JsonSerializerOptions();
            jso.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            jso.TypeInfoResolver = LambdaFunctionJsonSerializerContext.Default;
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(new GetStoriesResponse(page), jso),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Headers", "Content-Type"},
                    { "Access-Control-Allow-Origin", "*"},
                    { "Access-Control-Allow-Methods", "OPTIONS,GET" }
                }
            };
        }
        catch (JsonException ex)
        {
            context.Logger.LogError("JsonException: " + ex.Message);

            ProblemDetails problemDetails = new ProblemDetails()
            {
                Detail = IsDevelopment ? ex.Message + Environment.NewLine + ex.StackTrace : "Cannot parse request",
                Status = 500,
                Title = "Internal server error"
            };

            return new APIGatewayHttpApiV2ProxyResponse()
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(problemDetails, LambdaFunctionJsonSerializerContext.Default.ProblemDetails),
            };
        }        
    }
}

/// <summary>
/// This class is used to register the input event and return type for the FunctionHandler method with the System.Text.Json source generator.
/// There must be a JsonSerializable attribute for each type used as the input and return type or a runtime error will occur 
/// from the JSON serializer unable to find the serialization information for unknown types.
/// </summary>
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
[JsonSerializable(typeof(GetStoriesRequest))]
[JsonSerializable(typeof(GetStoriesResponse))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(System.Byte))]
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext
{
    // By using this partial class derived from JsonSerializerContext, we can generate reflection free JSON Serializer code at compile time
    // which can deserialize our class and properties. However, we must attribute this class to tell it what types to generate serialization code for.
    // See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-source-generation
}

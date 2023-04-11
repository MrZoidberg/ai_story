namespace AIStory.StoryGenerationLambda;

using AIStory.SharedModels.Localization;
using AIStory.SharedModels.Models;
using AIStory.StorySendTelegramLambda;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using OpenAI_API;
using OpenAI_API.Chat;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Zoid.AIStory.SharedModels.Dto;
using static Amazon.Lambda.SQSEvents.SQSEvent;

internal sealed class MessageProcessor
{
    private readonly AIRequestGeneratorFactory aiRequestGeneratorFactory;
    private readonly OpenAIAPI openAIAPI;
    private readonly IAmazonDynamoDB dynamoDBClient;
    private readonly ILambdaLogger logger;
    private readonly StringResourceFactory stringResourceFactory;
    private readonly StoriesRepository storiesRepository;

    public MessageProcessor(
        AIRequestGeneratorFactory aiRequestGeneratorFactory,
        OpenAIAPI openAIAPI,
        IAmazonDynamoDB dynamoDBClient,
        ILambdaLogger logger,
        StringResourceFactory stringResourceFactory,
        StoriesRepository storiesRepository)
    {
        this.aiRequestGeneratorFactory = aiRequestGeneratorFactory;
        this.openAIAPI = openAIAPI;
        this.dynamoDBClient = dynamoDBClient;
        this.logger = logger;
        this.stringResourceFactory = stringResourceFactory;
        this.storiesRepository = storiesRepository;
    }

    public async Task ProcessMessage(SQSMessage message)
    {
        GenerateStoryMessage generateStoryMessage = JsonSerializer.Deserialize(message.Body, MessageJsonSerializerContext.Default.GenerateStoryMessage) ?? throw new Exception("Cannot parse message");
        stringResourceFactory.Language = generateStoryMessage.Language;
        var aiRequestGenerator = aiRequestGeneratorFactory.Create(generateStoryMessage.Language);

        ChatRequest request = aiRequestGenerator.GenerateStoryRequest(generateStoryMessage, stringResourceFactory);
        //logger.LogInformation($"Serialized OpenAI request: {JsonSerializer.Serialize(request, MessageJsonSerializerContext.Default.ChatRequest)}");
        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            ChatResult result = await openAIAPI.Chat.CreateChatCompletionAsync(request);

            Story story = new Story()
            {
                StoryId = generateStoryMessage.Id,
                StoryText = result.Choices.ToDictionary(k => k.Index, v => v.Message.Content),
                ProcessingTime = result.ProcessingTime,
                CreatedAt = DateTime.UtcNow,
                Prompt = request.Messages.Select(m => m.Content).ToList(),
                ChatId = generateStoryMessage.ChatId,
                Language = generateStoryMessage.Language,
                StoryTheme = stringResourceFactory.StringResources.GetThemeName(generateStoryMessage.StoryTheme),
                StoryLocation = generateStoryMessage.StoryLocation,
                StoryCharacters = generateStoryMessage.StoryCharacters,
                GenerateAudio = generateStoryMessage.GenerateAudio,
                StoryLength = generateStoryMessage.StoryLength,
                Model = generateStoryMessage.Model,
                CompletionTokens = result.Usage.CompletionTokens
            };

            await storiesRepository.PutStory(story);

            logger.LogInformation($"{generateStoryMessage.Id}: OpenAPI returned response {result.RequestId} in {result.ProcessingTime.TotalSeconds}s");
        }
        catch (Exception ex)
        {
            Story story = new Story()
            {
                StoryId = generateStoryMessage.Id,
                ProcessingTime = stopwatch.Elapsed,
                CreatedAt = DateTime.UtcNow,
                Prompt = request.Messages.Select(m => m.Content).ToList(),
                ChatId = generateStoryMessage.ChatId,
                Language = generateStoryMessage.Language,
                StoryTheme = stringResourceFactory.StringResources.GetThemeName(generateStoryMessage.StoryTheme),
                StoryLocation = generateStoryMessage.StoryLocation,
                StoryCharacters = generateStoryMessage.StoryCharacters,
                GenerateAudio = generateStoryMessage.GenerateAudio,
                StoryLength = generateStoryMessage.StoryLength,
                Model = generateStoryMessage.Model,
                CompletionTokens = 0,
                Error = ex.Message
            };

            await storiesRepository.PutStory(story);

            logger.LogInformation($"{generateStoryMessage.Id}: OpenAPI failed with error {ex.GetType().FullName}. Message: {ex.Message}");
        }
    }
}
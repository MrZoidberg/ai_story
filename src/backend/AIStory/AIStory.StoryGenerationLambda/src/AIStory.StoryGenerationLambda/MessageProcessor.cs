namespace AIStory.StoryGenerationLambda;

using AIStory.SharedModels.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using OpenAI_API;
using OpenAI_API.Chat;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Zoid.AIStory.SharedModels;
using Zoid.AIStory.SharedModels.Dto;
using static Amazon.Lambda.SQSEvents.SQSEvent;

internal sealed class MessageProcessor
{
    private readonly AIRequestGeneratorFactory aiRequestGeneratorFactory;
    private readonly OpenAIAPI openAIAPI;
    private readonly AmazonDynamoDBClient dynamoDBClient;
    private readonly ILambdaLogger logger;


    public string StoriesTable { get; set; } = "Stories";

    public MessageProcessor(AIRequestGeneratorFactory aiRequestGeneratorFactory, OpenAIAPI openAIAPI, AmazonDynamoDBClient dynamoDBClient, ILambdaLogger logger)
    {
        this.aiRequestGeneratorFactory = aiRequestGeneratorFactory;
        this.openAIAPI = openAIAPI;
        this.dynamoDBClient = dynamoDBClient;
        this.logger = logger;
    }

    public async Task ProcessMessage(SQSMessage message)
    {
        GenerateStoryMessage generateStoryMessage = JsonSerializer.Deserialize(message.Body, MessageJsonSerializerContext.Default.GenerateStoryMessage) ?? throw new Exception("Cannot parse message");
        var aiRequestGenerator = aiRequestGeneratorFactory.Create(generateStoryMessage.Language);
        ChatRequest request = aiRequestGenerator.GenerateStoryRequest(generateStoryMessage);
        //logger.LogInformation($"Serialized OpenAI request: {JsonSerializer.Serialize(request, MessageJsonSerializerContext.Default.ChatRequest)}");
        ChatResult result = await openAIAPI.Chat.CreateChatCompletionAsync(request);

        Story story = new Story()
        {
            StoryId = generateStoryMessage.Id,
            StoryText = result.Choices.ToDictionary(k => k.Index, v => v.Message.Content),
            ProcessingTime = result.ProcessingTime,
            CreatedAt = DateTime.UtcNow,
            Prompt = request.Messages.Select(m => m.Content).ToList(),
            UserId = generateStoryMessage.UserId,
            Language = generateStoryMessage.Language,
            StoryTheme = generateStoryMessage.StoryTheme,
            StoryLocation = generateStoryMessage.StoryLocation,
            StoryCharacters = generateStoryMessage.StoryCharacters,
            GenerateAudio = generateStoryMessage.GenerateAudio,
            StoryLength = generateStoryMessage.StoryLength,
            Model = generateStoryMessage.Model,
            CompletionTokens = result.Usage.CompletionTokens
        };

        await WriteStoryToDynamoDB(story);

        logger.LogInformation($"{generateStoryMessage.Id}: OpenAPI returned response {result.RequestId} in {result.ProcessingTime.TotalSeconds}s");
    }

    private async Task WriteStoryToDynamoDB(Story story)
    {
        PutItemRequest request = new PutItemRequest
        {
            TableName = StoriesTable,
            Item = story.ToDynamoDb()
        };

        var result = await dynamoDBClient.PutItemAsync(request);
        if (result.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            logger.LogError($"{story.StoryId}: Cannot write entity to DynamoDB. Http status code: {result.HttpStatusCode}");
        }
    }   
}
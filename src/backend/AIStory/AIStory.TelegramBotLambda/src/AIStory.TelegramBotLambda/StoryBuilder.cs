namespace AIStory.TelegramBotLambda;

using AIStory.SharedModels.Models;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Zoid.AIStory.SharedModels.Dto;

internal class StoryBuilder
{
    private readonly IAmazonSQS sqsClient;
    private readonly string queueUrl;

    public string Model { get; set; } = Constants.DefaultModel;

    public ushort StoryLength { get; set; } = Constants.DefaultStoryLength;

    public StoryBuilder(IAmazonSQS sqsClient, LambdaOptions lambdaOptions)
    {
        this.sqsClient = sqsClient;
        this.queueUrl = lambdaOptions.SqsStoryGenerationUrl;
    }

    public async Task<int> GetQueueLength()
    {
        var request = new GetQueueAttributesRequest
        {
            QueueUrl = queueUrl,
            AttributeNames = new List<string> { "ApproximateNumberOfMessages" }
        };
        var response = await sqsClient.GetQueueAttributesAsync(request);
        var approximateNumberOfMessages = response.ApproximateNumberOfMessages;
        return approximateNumberOfMessages + 1;
    }

    public int CalculateStoryEta(int queueLength, string language)
    {
        switch (language)
        {
            case "ru":
                return queueLength * 60;
            case "uk":
                return queueLength * 60;
            case "en":
                return queueLength * 30;
            default:
                return queueLength * 30;
        }
    }

    public async Task<string> BuildStory(UserChat userChat)
    {
        // Generate message from UserChat
        GenerateStoryMessage generateStoryMessage = new GenerateStoryMessage();
        generateStoryMessage.Id = Guid.NewGuid().ToString();
        generateStoryMessage.Language = userChat.Language!;
        generateStoryMessage.StoryTheme = userChat.StoryTheme!.Value;
        generateStoryMessage.StoryLocation = userChat.StoryLocation!;
        generateStoryMessage.StoryCharacters = userChat.Characters!;
        generateStoryMessage.GenerateAudio = false;
        generateStoryMessage.StoryLength = StoryLength;
        generateStoryMessage.Model = Model;
        generateStoryMessage.ChatId = userChat.ChatId;

        // Send message to SQS
        string messageBody = JsonSerializer.Serialize(generateStoryMessage, MessageJsonSerializerContext.Default.GenerateStoryMessage);
        var sendMessageRequest = new Amazon.SQS.Model.SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = messageBody
        };
        await sqsClient.SendMessageAsync(sendMessageRequest);

        return generateStoryMessage.Id;
    }
}

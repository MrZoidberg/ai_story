namespace AIStory.TelegramBotLambda;

using AIStory.SharedModels.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

public class UserChatRepository
{
    private readonly IAmazonDynamoDB amazonDynamoDB;
    private readonly ILambdaLogger? logger;

    public string TableName { get; set; }

    public UserChatRepository(IAmazonDynamoDB amazonDynamoDB, LambdaOptions lambdaOptions, ILambdaLogger? logger)
    {
        this.amazonDynamoDB = amazonDynamoDB;
        this.logger = logger;
        TableName = lambdaOptions.DynamoDbChatsTable ?? "Chats";
    }

    public async Task<UserChat?> GetUserChat(string chatId)
    {
        try
        {
            var request = new GetItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
            {
                { "ChatId", new AttributeValue { S = chatId } },
            },
                ConsistentRead = false,
            };

            var response = await amazonDynamoDB.GetItemAsync(request);
            if (response.Item.Count == 0)
            {
                return null;
            }
            return response.Item.ToUserChat();
        }
        catch (AmazonDynamoDBException ex)
        {
            logger?.LogError($"Cannot get chat from DynamoDB. Exception: {ex.GetType().Name} {Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            return null;
        }
    }

    public async Task UpdateChatStep(UserChat userChat)
    {
        var request = new UpdateItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "ChatId", new AttributeValue { S = userChat.ChatId.ToString() } },
            },
            UpdateExpression = "set ChatStepId = :chatStepId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":chatStepId", new AttributeValue { N = userChat.ChatStepId.ToString() } },
            },
        };
        await amazonDynamoDB.UpdateItemAsync(request);
    }

    public async Task UpdateChatLanguage(UserChat userChat)
    {
        if (userChat.Language == null)
        {
            throw new InvalidOperationException("Cannot save null Language");
        }

        var request = new UpdateItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "ChatId", new AttributeValue { S = userChat.ChatId.ToString() } },
            },
            UpdateExpression = "set Lang = :lang",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":lang", new AttributeValue { S = userChat.Language } },
            },
        };
        await amazonDynamoDB.UpdateItemAsync(request);
    }

    public async Task UpdateStoryTheme(UserChat userChat)
    {
        if (userChat.StoryTheme == null)
        {
            throw new InvalidOperationException("Cannot save null StoryTheme");
        }

        var request = new UpdateItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "ChatId", new AttributeValue { S = userChat.ChatId.ToString() } },
            },
            UpdateExpression = "set StoryTheme = :storyTheme",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":storyTheme", new AttributeValue { N = ((int)userChat.StoryTheme!).ToString() } },
            },
        };
        await amazonDynamoDB.UpdateItemAsync(request);
    }

    public async Task UpdateCharacters(UserChat userChat)
    {
        if (userChat.Characters == null)
        {
            throw new InvalidOperationException("Cannot save null Characters");
        }

        var request = new UpdateItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "ChatId", new AttributeValue { S = userChat.ChatId.ToString() } },
            },
            UpdateExpression = "set Characters = :storyCharacters",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":storyCharacters", new AttributeValue { SS = userChat.Characters!.ToList() } },
            },
        };
        await amazonDynamoDB.UpdateItemAsync(request);
    }

    public async Task UpdateStoryLocation(UserChat userChat)
    {
        if (userChat.StoryLocation == null)
        {
            throw new InvalidOperationException("Cannot save null StoryLocation");
        }

        var request = new UpdateItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
                {
                { "ChatId", new AttributeValue { S = userChat.ChatId.ToString() } },
            },
            UpdateExpression = "set StoryLocation = :storyLocation",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                { ":storyLocation", new AttributeValue { S = userChat.StoryLocation } },
            },
        };
    }

    public Task AddUserChat(UserChat userChat)
    {
        try
        {
            var request = new PutItemRequest()
            {
                TableName = TableName,
                Item = userChat.ToDynamoDb(),
            };

            return amazonDynamoDB.PutItemAsync(request);
        }
        catch (AmazonDynamoDBException ex)
        {
            logger?.LogError($"Cannot write chat to DynamoDB. Exception: {ex.GetType().Name} {Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            throw;
        }
    }
}

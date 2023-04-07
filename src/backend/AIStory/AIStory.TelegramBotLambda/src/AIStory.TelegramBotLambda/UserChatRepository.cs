namespace AIStory.TelegramBotLambda;

using AIStory.SharedModels.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class UserChatRepository
{
    private readonly IAmazonDynamoDB amazonDynamoDB;

    public string TableName { get; set; } = "UserChats";

    public UserChatRepository(IAmazonDynamoDB amazonDynamoDB)
    {
        this.amazonDynamoDB = amazonDynamoDB;
    }

    public async Task<UserChat?> GetUserChat(long userId)
    {
        var request = new GetItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "UserId", new AttributeValue { N = userId.ToString() } },
            },
        };

        var response = await amazonDynamoDB.GetItemAsync(request);
        if (response.Item == null)
        {
            return null;
        }
        return response.Item.ToUserChat();
    }

    public Task UpdateUserChat(UserChat userChat)
    {
        var request = new UpdateItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "UserId", new AttributeValue { N = userChat.UserId.ToString() } },
            },
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#ChatStepId", "ChatStepId" },
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":ChatStepId", new AttributeValue { N = userChat.ChatStepId.ToString() } },
            },
            UpdateExpression = "SET #ChatStepId = :ChatStepId",
        };
        return amazonDynamoDB.UpdateItemAsync(request);
    }

    public Task AddUserChat(UserChat userChat)
    {
        var request = new PutItemRequest()
        {
            TableName = TableName,
            Item = userChat.ToDynamoDb()
        };

        return amazonDynamoDB.PutItemAsync(request);
    }
}

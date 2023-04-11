namespace AIStory.StorySendTelegramLambda;

using AIStory.SharedModels.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

internal class StoriesRepository
{
    private readonly IAmazonDynamoDB amazonDynamoDB;

    public string TableName { get; set; }

    public StoriesRepository(IAmazonDynamoDB amazonDynamoDB, LambdaOptions lambdaOptions)
    {
        this.amazonDynamoDB = amazonDynamoDB;
        TableName = lambdaOptions.StoriesTableName ?? "Stories";
    }

    public async Task UpdateIsSentToChat(Story story)
    {
        var request = new UpdateItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "StoryId", new AttributeValue { S = story.StoryId.ToString() } },
            },
            UpdateExpression = "set IsSentToChat = :v_isSentToChat",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_isSentToChat", new AttributeValue { BOOL = story.IsSentToChat } },
            },
        };
        await amazonDynamoDB.UpdateItemAsync(request);
    }
}

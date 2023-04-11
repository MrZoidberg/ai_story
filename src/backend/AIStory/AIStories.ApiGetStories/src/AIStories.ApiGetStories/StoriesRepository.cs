namespace AIStories.ApiGetStories;

using AIStory.SharedModels.Dto;
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

    /// <summary>
    /// Gets stories page from DynamoDB using Language-CreatedAt-index index and returns it as StoryShortPage
    /// </summary>
    public async Task<StoryShortPage> GetStoriesPages(string? lastKey, int pageSize, string language)
    { 
        var request = new QueryRequest
        {
            TableName = TableName,
            IndexName = "Language-CreatedAt-index",
            KeyConditionExpression = "#Language = :v_language",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_language", new AttributeValue { S = language } },
            },
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#Language", "Language" },
            },
            ScanIndexForward = false,
            Limit = pageSize,
            ExclusiveStartKey = lastKey == null ? null : new Dictionary<string, AttributeValue>
            {
                { "StoryId", new AttributeValue { S = lastKey } },
            },
        };
        var response = await amazonDynamoDB.QueryAsync(request);
        var stories = new List<StoryShortProjection>();
        foreach (var item in response.Items)
        {
            stories.Add(item.ToStoryShortProjection());
        }

        return new StoryShortPage()
        {
            Stories = stories,
            LastEvaluatedKey = response.LastEvaluatedKey == null ? null : response.LastEvaluatedKey["StoryId"].S,
            HasMore = response.LastEvaluatedKey != null,
            PageSize = pageSize
        };
    }
}

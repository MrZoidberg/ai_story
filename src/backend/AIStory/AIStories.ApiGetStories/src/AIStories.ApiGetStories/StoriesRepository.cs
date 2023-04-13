namespace AIStories.ApiGetStories;

using AIStory.SharedModels.Dto;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
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
        Dictionary<string, AttributeValue>? exclusiveStartKey = null;
        if (!string.IsNullOrEmpty(lastKey)) {
            var lastKeyJson = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(lastKey));
            exclusiveStartKey = Document.FromJson(lastKeyJson).ToAttributeMap();
        }

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
                { "#Story", "Story" },
            },
            ScanIndexForward = false,
            Limit = pageSize,
            ExclusiveStartKey = exclusiveStartKey,
            FilterExpression = "attribute_exists(#Story)"
        };
        var response = await amazonDynamoDB.QueryAsync(request);
        var stories = new List<StoryShortProjection>();
        foreach (var item in response.Items)
        {
            stories.Add(item.ToStoryShortProjection());
        }
        
        string? lastEvaluatedKeyBase64 = null;
        if (response.LastEvaluatedKey != null && response.LastEvaluatedKey.Count > 0)
        {
            var json = Document.FromAttributeMap(response.LastEvaluatedKey).ToJson();
            lastEvaluatedKeyBase64 = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
        }

        return new StoryShortPage()
        {
            Stories = stories,
            LastEvaluatedKey = lastEvaluatedKeyBase64,
            HasMore = lastEvaluatedKeyBase64 != null,
            PageSize = response.Count
        };
    }
}

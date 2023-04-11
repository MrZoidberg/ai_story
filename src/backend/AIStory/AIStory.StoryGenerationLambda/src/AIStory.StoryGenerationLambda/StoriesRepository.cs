namespace AIStory.StorySendTelegramLambda;

using AIStory.SharedModels.Models;
using AIStory.StoryGenerationLambda;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
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

   public async Task PutStory(Story story)
    {
        PutItemRequest request = new PutItemRequest
        {
            TableName = TableName,
            Item = story.ToDynamoDb()
        };
        await amazonDynamoDB.PutItemAsync(request);
    }
}

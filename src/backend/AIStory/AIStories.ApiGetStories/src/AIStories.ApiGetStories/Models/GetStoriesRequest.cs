namespace AIStories.ApiGetStories.Models;
public class GetStoriesRequest
{
    public string? Language { get; set; }
    public int? PageSize { get; set; }
    public string? LastEvaluatedKey { get; set; }

    public static GetStoriesRequest FromParameters(IDictionary<string, string> requestParameters)
    {
        return new GetStoriesRequest()
        {
            Language = requestParameters.ContainsKey("language") ? requestParameters["language"] : null,
            PageSize = requestParameters.ContainsKey("pageSize") ? int.Parse(requestParameters["pageSize"]) : null,
            LastEvaluatedKey = requestParameters.ContainsKey("lastKey") ? requestParameters["lastKey"] : null,
        };        
    }
}

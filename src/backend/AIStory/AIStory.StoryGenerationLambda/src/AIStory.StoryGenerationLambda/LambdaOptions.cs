namespace AIStory.StoryGenerationLambda;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class LambdaOptions
{
    public string OpenAIKey { get; set; }

    public string? StoriesTableName { get; set; }
}

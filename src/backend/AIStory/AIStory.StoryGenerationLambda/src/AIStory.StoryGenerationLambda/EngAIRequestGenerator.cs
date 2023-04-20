namespace AIStory.StoryGenerationLambda;
using OpenAI_API.Models;
using System.Collections.Generic;

internal sealed class EngAIRequestGenerator : AIRequestGenerator
{
    private static string[] Prepositions = new string[] { "in ", "on ", "under ", "before ", "after ", "by ", "at ", "from ", "to ", "under ", "before ", "against ", "about ", "near " };

    protected override string GetPromtTemplate(string language, string model, int participantsCount, ushort lengthTokens)
    {
        return $"Write a story in the genre {{0}} with {{1}} main characters {{2}}. The story must take place {{3}} and text must contain at least {GetMinLengthFromTokens(model, lengthTokens)} and no more than {GetMaxLengthFromTokens(model, lengthTokens)} words. Be sure to finish the story. Come up with a name for the story and hashtags for it. Do not add hashtags directly to the text field.";
    }

    protected override string GetSystemPrompt(bool generateAudio)
    {
        string audioPart = generateAudio ? " Use the Speech Synthesis Markup Language to set intonation in text." : string.Empty;
        string prompt = "Give the response as JSON with text, hashtags, title fields. \r\nExample:\r\n{\r\n\"text\": \"Here goes story text\",\r\n\"title\": \"Story title\",\r\n\"hashTags\": [\"#FirstTag\", \"#SecondTag\"]\r\n}\r\n" + audioPart;
        return prompt;
    }

    protected override string GetParticipantsPromptPart(string language, IEnumerable<string> participants)
    {
        return string.Join(" and ", participants);
    }

    protected override ushort GetTokensCount(string model, ushort lengthCharacters)
    {
        return (ushort)(lengthCharacters * 5);
    }

    private ushort GetMinLengthFromTokens(string model, ushort lengthTokens)
    {
        if (model == Model.ChatGPTTurbo)
        {
            return (ushort)(lengthTokens / 4);
        }
        else
        {
            return (ushort)(lengthTokens / 4);
        }
    }

    private ushort GetMaxLengthFromTokens(string model, ushort lengthTokens)
    {
        if (model == Model.ChatGPTTurbo)
        {
            return (ushort)(lengthTokens / 3);
        }
        else
        {
            return (ushort)(lengthTokens / 3);
        }
    }

    protected override string PrepareLocationText(string location)
    {
        location = location.Trim();
        return Prepositions.Any(p => location.StartsWith(p, StringComparison.OrdinalIgnoreCase)) ? location : $"in {location}";
    }
}

namespace AIStory.StoryGenerationLambda;
using OpenAI_API.Models;
using System.Collections.Generic;

internal sealed class EngAIRequestGenerator : AIRequestGenerator
{

    protected override string GetPromtTemplate(string language, string model, int participantsCount, ushort lengthTokens, bool generateAudio)
    {
        string audioPart = generateAudio ? " Use the Speech Synthesis Markup Language to set intonation in text." : string.Empty;
        return $"Write {{0}} with {{1}} main characters {{2}}. History must take place in a place {{3}} and must contain at least {GetMinLengthFromTokens(model, lengthTokens)} and no more than {GetMaxLengthFromTokens(model, lengthTokens)} words. Be sure to finish the story.{audioPart}";
    }

    protected override string GetParticipantsPromptPart(string language, IEnumerable<string> participants)
    {
        return string.Join(" and ", participants);
    }

    protected override ushort GetTokensCount(string model, ushort lengthCharacters)
    {
        return (ushort)(lengthCharacters * 3);
    }

    private ushort GetMinLengthFromTokens(string model, ushort lengthTokens)
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

    private ushort GetMaxLengthFromTokens(string model, ushort lengthTokens)
    {
        if (model == Model.ChatGPTTurbo)
        {
            return (ushort)(lengthTokens / 2);
        }
        else
        {
            return (ushort)(lengthTokens / 2);
        }
    }
}

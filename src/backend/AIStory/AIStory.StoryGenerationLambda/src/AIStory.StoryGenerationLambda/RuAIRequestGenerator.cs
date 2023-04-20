namespace AIStory.StoryGenerationLambda;
using OpenAI_API.Models;
using System.Collections.Generic;

internal sealed class RuAIRequestGenerator: AIRequestGenerator
{
    private static string[] Prepositions = new string[] { "в ", "на ", "под ", "перед ", "за ", "по ", "у ", "из ", "до ", "под ", "перед ", "против ", "про ", "рядом" };

    protected override string GetPromtTemplate(string language, string model, int participantsCount, ushort lengthTokens)
    {
        string participantsPart = participantsCount == 1 ? "главным персонажем" : "главными персонажами";
        
        return $"Напиши историю в жанре {{0}} c {{1}} {participantsPart} {{2}}. История должна происходить {{3}} и текст должен содержать не менее {GetMinLengthFromTokens(model, lengthTokens)} и не более {GetMaxLengthFromTokens(model, lengthTokens)} слов. Обязательно закончи рассказ. Придумай название истории и хештеги к ней.";
    }

    protected override string GetSystemPrompt(bool generateAudio)
    {
        string audioPart = generateAudio ? " Используй Speech Synthesis Markup Language для расстановки интонации в тексте." : string.Empty;
        string prompt = "Выдавай результат только в виде JSON с полями story, hashtags, title.\r\nПример:\r\n{\r\n\"text\": \"Here goes story text\",\r\n\"title\": \"Story title\",\r\n\"hashTags\": [\"#FirstTag\", \"#SecondTag\"]\r\n}\r\n" + audioPart;
        return prompt;
    }

    protected override string GetParticipantsPromptPart(string language, IEnumerable<string> participants)
    {
        return string.Join(" и ", participants);
    }

    protected override ushort GetTokensCount(string model, ushort lengthCharacters)
    {
        return (ushort)(lengthCharacters * 6);
    }

    private ushort GetMinLengthFromTokens(string model, ushort lengthTokens)
    {
        if (model == Model.ChatGPTTurbo)
        {
            return (ushort)(lengthTokens / 5);
        }
        else
        {
            return (ushort)(lengthTokens / 5);
        }
    }

    private ushort GetMaxLengthFromTokens(string model, ushort lengthTokens)
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

    protected override string PrepareLocationText(string location)
    {
        location = location.Trim();
        return Prepositions.Any(p => location.StartsWith(p, StringComparison.OrdinalIgnoreCase)) ? location : $"в {location}";
    }
}

namespace AIStory.StoryGenerationLambda;
using OpenAI_API.Models;
using System.Collections.Generic;

internal sealed class UkAIRequestGenerator : AIRequestGenerator
{
    private static string[] Prepositions = new string[] { "в ", "на ", "під ", "перед ", "за ", "по ", "у ", "з ", "до ", "під ", "перед ", "проти ", "про ", "бiля " };

    protected override string GetPromtTemplate(string language, string model, int participantsCount, ushort lengthTokens)
    {
        string participantsPart = participantsCount == 1 ? "головним персонажем" : "головними персонажами";
        
        return $"Напиши історію у жанрі {{0}} з {{1}} {participantsPart} {{2}}. Історія має відбуватися {{3}} і текст має містити не менше {GetMinLengthFromTokens(model, lengthTokens)} і не більше {GetMaxLengthFromTokens(model, lengthTokens)} слів. Обов'язково закінчи розповідь. Придумай назву історії та хештеги до неї.";
    }

    protected override string GetSystemPrompt(bool generateAudio)
    {
        string audioPart = generateAudio ? " Використовуйте Speech Synthesis Markup Language для розміщення інтонації в тексті." : string.Empty;
        string prompt = "Віддай відповідь лише у вигляді JSON з полями text, hashtags, title.\r\nПриклад:\r\n{\r\n\"text\": \"Here goes story text\",\r\n\"title\": \"Story title\",\r\n\"hashTags\": [\"#FirstTag\", \"#SecondTag\"]\r\n}\r\n" + audioPart;
        return prompt;
    }

    protected override string GetParticipantsPromptPart(string language, IEnumerable<string> participants)
    {
        return string.Join(" та ", participants);
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
       return Prepositions.Any(p => location.StartsWith(p, StringComparison.OrdinalIgnoreCase)) ? location : $"у {location}";
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Redis;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IRedisStorage _storage;

    public IndexModel(ILogger<IndexModel> logger, IRedisStorage storage)
    {
        _logger = logger;
        _storage = storage;
    }

    public void OnGet()
    {

    }

    private static string GetRank(string text)
    {
        if (String.IsNullOrEmpty(text))
            return "0";

        double nonalphaCounter = 0;

        foreach (var ch in text) {
            if (!Char.IsLetter(ch))
            {
                nonalphaCounter++;
            }
        }

        return Convert.ToString(nonalphaCounter / text.Length);
    }

    private int GetSimilarity(string text)
    {
        var keys = _storage.GetKeys();
        string textPrefix = "TEXT-";
        foreach (var ch in keys)
        {
            if (ch.StartsWith(textPrefix) && _storage.Get(ch) == text)
            {
                return 1;
            }
        }
        return 0;
    }

    public IActionResult OnPost(string text)
    {
        if (string.IsNullOrEmpty(text))
            Redirect($"index");

        _logger.LogDebug(text);

        string id = Guid.NewGuid().ToString();

        string similarityKey = "SIMILARITY-" + id;
        //TODO: посчитать similarity и сохранить в БД по ключу similarityKey
        int similarity = GetSimilarity(text);
        _storage.Set(similarityKey, similarity.ToString());

        string textKey = "TEXT-" + id;
        //TODO: сохранить в БД text по ключу textKey
        _storage.Set(textKey, text);

        string rankKey = "RANK-" + id;
        //TODO: посчитать rank и сохранить в БД по ключу rankKey
        string rank = GetRank(text);
        _storage.Set(rankKey, rank);

        return Redirect($"summary?id={id}");
    }
}
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
        double nonalphaCounter = 0;

        foreach (var ch in text) {
            if (!Char.IsLetter(ch))
            {
                nonalphaCounter++;
            }
        }

        return Convert.ToString(nonalphaCounter / text.Length);
    }

    private int GetSimilarity(string text, string key)
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
        _logger.LogDebug(text);

        string id = Guid.NewGuid().ToString();

        string textKey = "TEXT-" + id;
        //TODO: сохранить в БД text по ключу textKey
        _storage.Set(textKey, text);

        string rankKey = "RANK-" + id;
        //TODO: посчитать rank и сохранить в БД по ключу rankKey
        string rank = GetRank(text);
        _storage.Set(rankKey, rank);

        string similarityKey = "SIMILARITY-" + id;
        //TODO: посчитать similarity и сохранить в БД по ключу similarityKey
        int similarity = GetSimilarity(text, id);
        _storage.Set(similarityKey, similarity.ToString());

        return Redirect($"summary?id={id}");
    }
}
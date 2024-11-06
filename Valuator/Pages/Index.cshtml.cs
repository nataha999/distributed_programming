using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NATS.Client;
using System.Text;
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

    static async Task ProduceAsync(CancellationToken ct, string id)
    {
        ConnectionFactory cf = new ConnectionFactory();

        using (IConnection c = cf.CreateConnection())
        {
            byte[] data = Encoding.UTF8.GetBytes(id);
            c.Publish("valuator.processing.rank", data);
            await Task.Delay(1000);
            c.Drain();

            c.Close();
        }
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
            return Redirect($"index");

        _logger.LogDebug(text);

        string id = Guid.NewGuid().ToString();

        string similarityKey = "SIMILARITY-" + id;
        //TODO: посчитать similarity и сохранить в БД по ключу similarityKey
        int similarity = GetSimilarity(text);
        _storage.Set(similarityKey, similarity.ToString());

        string textKey = "TEXT-" + id;
        //TODO: сохранить в БД text по ключу textKey
        _storage.Set(textKey, text);

        CancellationTokenSource cts = new CancellationTokenSource();
        ConnectionFactory cf = new ConnectionFactory();

        using (IConnection c = cf.CreateConnection())
        {
            byte[] data = Encoding.UTF8.GetBytes(id);
            c.Publish("valuator.processing.rank", data);

            c.Drain();

            c.Close();
        }
        cts.Cancel();

        return Redirect($"summary?id={id}");
    }
}
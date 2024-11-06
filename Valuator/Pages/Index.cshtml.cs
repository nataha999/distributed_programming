using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NATS.Client;
using System.Text;
using System.Text.Json;
using Valuator.Redis;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    public class MessageInfo
    {
        public string Id { get; set; }
        public string Result { get; set; }

        public MessageInfo(string id, string result)
        {
            Id = id;
            Result = result;
        }
    }

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

    public IActionResult OnPost(string text)
    {
        if (string.IsNullOrEmpty(text))
            return Redirect($"index");
        else
        {
            _logger.LogDebug(text);

            string id = Guid.NewGuid().ToString();

            string similarityKey = "SIMILARITY-" + id;
            //TODO: посчитать similarity и сохранить в БД по ключу similarityKey
            int similarity = GetSimilarity(text);
            _storage.Set(similarityKey, similarity.ToString());

            string textKey = "TEXT-" + id;
            //TODO: сохранить в БД text по ключу textKey
            _storage.Set(textKey, text);

            ConnectionFactory connectionFactory = new ConnectionFactory();

            using (IConnection c = connectionFactory.CreateConnection())
            {
                byte[] data = Encoding.UTF8.GetBytes(id);
                c.Publish("valuator.processing.rank", data);

                MessageInfo? info = new(textKey, similarity);
                string jsonData = JsonSerializer.Serialize(info);

                byte[] jsonDataEncoded = Encoding.UTF8.GetBytes(jsonData);

                c.Publish("similarityCalculated", jsonDataEncoded);

                c.Drain();

                c.Close();
            }

            return Redirect($"summary?id={id}");
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

}
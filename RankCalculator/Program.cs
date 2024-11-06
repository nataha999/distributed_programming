using NATS.Client;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

namespace RankCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionFactory cf = new ConnectionFactory();
            IConnection c = cf.CreateConnection();

            var s = c.SubscribeAsync("valuator.processing.rank", "rankCalculator", (sender, args) =>
            {
                string data = Encoding.UTF8.GetString(args.Message.Data);
                IdAndCountryOfText? structData = JsonSerializer.Deserialize<IdAndCountryOfText>(data);

                string dbEnvironmentVariable = $"DB_{structData?.country}";
                string? dbConnection = Environment.GetEnvironmentVariable(dbEnvironmentVariable);

                if (dbConnection == null)
                {
                    return;
                }

                IDatabase savingDb = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(dbConnection)).GetDatabase();

                string textKey = "TEXT-" + structData?.textId;
                string? text = savingDb?.StringGet(textKey);

                string rankKey = "RANK-" + structData?.textId;

                double rank = GetRank(text);

                savingDb?.StringSet(rankKey, rank);
                Console.WriteLine($"LOOKUP: {structData?.textId}, {structData?.country}");

                if (structData == null)
                {
                    return;
                }
                MessageInfo textData = new(structData.textId, rank);
                string jsonData = JsonSerializer.Serialize(textData);

                byte[] jsonDataEncoded = Encoding.UTF8.GetBytes(jsonData);

                c.Publish("valuator.logs.events.rank", jsonDataEncoded);
            });

            s.Start();

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }

        static string GetRank(string text)
        {
            double all = text.Length;
            double nonalphaCounter = 0;

            foreach (var ch in text)
            {
                if (!Char.IsLetter(ch))
                {
                    nonalphaCounter++;
                }
            }

            return (nonalphaCounter / all).ToString();
        }
    }
}

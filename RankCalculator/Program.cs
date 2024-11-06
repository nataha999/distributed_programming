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
            ConfigurationOptions redisConfiguration = ConfigurationOptions.Parse("localhost:6379");
            ConnectionMultiplexer redisConnection = ConnectionMultiplexer.Connect("localhost");
            IDatabase db = redisConnection.GetDatabase();
            ConnectionFactory cf = new ConnectionFactory();
            IConnection c = cf.CreateConnection();
            Console.WriteLine("RankCalculator started");

            var s = c.SubscribeAsync("valuator.processing.rank", "rankCalculator", (sender, args) =>
            {
                string id = Encoding.UTF8.GetString(args.Message.Data);

                string textKey = "TEXT-" + id;
                string text = db.StringGet(textKey);

                string rankKey = "RANK-" + id;

                string rank = GetRank(text);

                db.StringSet(rankKey, rank);

                MessageInfo data = new(textKey, rank);
                string jsonData = JsonSerializer.Serialize(data);

                byte[] jsonDataEncoded = Encoding.UTF8.GetBytes(jsonData);

                c.Publish("rankCalculated", jsonDataEncoded);
            });

            s.Start();

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();

            s.Unsubscribe();

            c.Drain();
            c.Close();
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

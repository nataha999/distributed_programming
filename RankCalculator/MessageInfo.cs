namespace RankCalculator
{
    class MessageInfo
    {
        public MessageInfo(string id, double data)
        {
            this.id = id;
            this.data = data;
        }
        public string id { get; set; }
        public double data { get; set; }
    }

    class IdAndCountryOfText
    {
        public IdAndCountryOfText(string country, string textId)
        {
            this.textId = textId;
            this.country = country;
        }
        public string country { get; set; }
        public string textId { get; set; }
    }
}
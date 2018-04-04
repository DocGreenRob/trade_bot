namespace TradeBot.Models
{
    public class Underlying
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public double PriorDayClose { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
    }
}
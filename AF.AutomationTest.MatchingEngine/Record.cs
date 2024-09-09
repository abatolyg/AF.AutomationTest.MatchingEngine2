using System;

namespace AF.AutomationTest.MatchingEngine
{
    public class Record
    {
        public Record(string id, string symbol, int quantity, decimal price, DateTime settlementDate, string side)
        {
            Id = id;
            Symbol = symbol;
            Quantity = quantity;
            Price = price;
            SettlementDate = settlementDate;
            Side = side;
        }

        public string Id { get; }

        public string Symbol { get; }

        public int Quantity { get; }

        public decimal Price { get; }

        public DateTime SettlementDate { get; }

        public string Side { get; }

        public override string ToString()
        {
            return $"Id:{Id}, Symbol:{Symbol}, Quantity:{Quantity}, Price:{Price}, SettlementDate:{SettlementDate}, Side:{Side}";
        }
    }
}

using System;

namespace AF.AutomationTest.MatchingEngine
{
    public class Record
    {
        public Record(string id, Symbol symbol, int quantity, double price, DateTime settlementDate, string side)
        {
            Id = id;
            Symbol = symbol; // TOBE REPLACED WITH Symbol enum
            Quantity = quantity;
            Price = price; //  to be double and not decimal
            SettlementDate = settlementDate;
            Side = side;
        }

        public string Id { get; }

        public Symbol Symbol { get; }

        public int Quantity { get; }

        public double Price { get; }

        public DateTime SettlementDate { get; }

        public string Side { get; }

        public override string ToString()
        {
            return $"Id:{Id}, Symbol:{Symbol}, Quantity:{Quantity}, Price:{Price}, SettlementDate:{SettlementDate}, Side:{Side}";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AF.AutomationTest.MatchingEngine
{
    internal class MatchingHandler
    {
        internal Dictionary<string, Record> Trades { get; }

        internal Dictionary<(string trade, string counterTrade), (Record trade, Record counterTrade)> Matches { get; }

        internal MatchingHandler()
        {
            Trades = new Dictionary<string, Record>();
            Matches = new Dictionary<(string trade, string counterTrade), (Record trade, Record counterTrade)>();
        }

        internal void TryMatch(string tradeId)
        {
            var tradeToMatch = Trades[tradeId];

            foreach (var counterTrade in Trades.Values.Where(trade => trade.Id != tradeId))
            {
                var result = IsMatch(tradeToMatch, counterTrade);
                if (result)
                {
                    return;
                }
            }

            Trace.WriteLine($"Could not find match for {tradeToMatch} ");
        }

        private bool IsMatch(Record trade, Record counterTrade)
        {
            const int quantityTolerance = 10;

            if (trade.Symbol == counterTrade.Symbol &&
                Math.Abs((trade.Quantity - counterTrade.Quantity)) <= quantityTolerance &&
                DateTime.Compare(trade.SettlementDate,counterTrade.SettlementDate) == 0 && 
                trade.Price == counterTrade.Price &&
                trade.Side != counterTrade.Side)
            {
                Matches.Add((trade.Id, counterTrade.Id), (trade, counterTrade));
                Trace.WriteLine($"match found!! tradeId: {trade.Id}, counterTradeId: {counterTrade.Id}");
                return true;
            }

            return false;
        }
    }
}

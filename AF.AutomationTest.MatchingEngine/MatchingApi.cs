using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AF.AutomationTest.MatchingEngine
{
    public class MatchingApi
    {
        private readonly MatchingHandler _matchingHandler;
        public MatchingApi()
        {
            _matchingHandler = new MatchingHandler();
        }

        /// <summary>
        /// Send trade to matching engine
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="quantity"></param>
        /// <param name="price"></param>
        /// <param name="settlementDate"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        public Record CreateRecord(string symbol, int quantity, double price, DateTime settlementDate, Side side)
        {
            var id = Guid.NewGuid().ToString();
            var record = new Record(id, symbol.ToString(), quantity, new decimal(Math.Round(price, 2)), settlementDate, side.ToString());
            try
            {
                _matchingHandler.Trades.Add(id, record);
                Trace.WriteLine($"Symbol:{symbol} sent");
                _matchingHandler.TryMatch(record.Id);
            }
            catch (Exception)
            {
                Trace.WriteLine("Failed to send trade");
                return null;
            }

            return record;
        }

        /// <summary>
        /// Get all trades sent to matching engine
        /// </summary>
        /// <returns></returns>
        public IList<Record> GetAllRecords()
        {
            var allTrades = _matchingHandler.Trades.Values.ToList();
            Trace.WriteLine($"{Environment.NewLine}All trades:");
            if (allTrades.Any())
            {
                allTrades.ForEach(t => Trace.WriteLine(t));
            }
            else
            {
                Trace.WriteLine("No trades found");
            }
            
            return allTrades;
        }

        /// <summary>
        /// Get all matches for symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public IList<(Record trade, Record counterTrade)> FindMatch(string symbol)
        {
            return _matchingHandler.Matches.Values.Where(t => t.trade.Symbol == symbol).ToList();
        }

        /// <summary>
        /// Get all matches for symbol
        /// </summary>
        /// <param name="record"></param>
        /// <param name="counterRecord"></param>
        /// <returns></returns>
        public bool CheckIfRecordsMatched(Record record, Record counterRecord)
        {
            var numOfMatch = _matchingHandler.Matches.Values.Where(t => (t.trade.Id == record.Id && t.counterTrade.Id == counterRecord.Id) 
                                                                   || t.trade.Id == counterRecord.Id && t.counterTrade.Id == record.Id);
            
            return (numOfMatch.Count() != 0);
        }

        /// <summary>
        /// Clear all data
        /// </summary>
        public void ClearData()
        {
            _matchingHandler.Trades.Clear();
            _matchingHandler.Matches.Clear();
        }
    }
}

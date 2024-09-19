public class TradeRecord
{
    public Id _id { get; set; }
    public Record record1 { get; set; }
    public Record record2 { get; set; }
    public Record record3 { get; set; }
    public bool expectedMatch { get; set; }
    public string DisplayName { get; set; }
}

public class Id
{
    public string $oid { get; set; }
}

public class Record
{
    public string Symbol { get; set; }
    public double Price { get; set; }
    public int Quantity { get; set; }
    public string Side { get; set; }
}

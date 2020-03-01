namespace TurtleTrade.Abstraction.Database
{
    public interface ITurtlePriceEntry
    {
        decimal? ATR { get; }
        decimal? N20 { get; }
        decimal? N40 { get; }
        decimal? N60 { get; }
        decimal? HighIn20 { get; }
        decimal? LowIn10 { get; }
        decimal? HighIn40 { get; }
        decimal? LowIn15 { get; }
        decimal? HighIn60 { get; }
        decimal? LowIn20 { get; }
        decimal? MA20 { get; }
        decimal? MA40 { get; }
        decimal? MA60 { get; }
        decimal? MA120 { get; }
        decimal? MA240 { get; }
    }
}

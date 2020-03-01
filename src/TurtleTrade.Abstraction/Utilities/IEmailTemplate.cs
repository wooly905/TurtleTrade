namespace TurtleTrade.Abstraction.Utilities
{
    public interface IEmailTemplate
    {
        string HtmlContent { get; }

        string ReceipentEmail { get; }

        string Subject { get; }
    }
}

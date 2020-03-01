namespace TurtleTrade.Abstraction.Config
{
    public interface ISMTPInfo
    {
        string SenderEmail { get; set; }

        string SenderPassword { get; set; }

        string SMTPServer { get; set; }

        int SMTPTCPPort { get; set; }
    }
}

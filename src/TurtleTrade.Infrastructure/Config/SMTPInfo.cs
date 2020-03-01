using TurtleTrade.Abstraction.Config;

namespace TurtleTrade.Infrastructure.Config
{
    public class SMTPInfo : ISMTPInfo
    {
        public SMTPInfo(string senderEmail, string senderPassword, string smtpServer, int smtpTCPPort)
        {
            SenderEmail = senderEmail;
            SenderPassword = senderPassword;
            SMTPServer = smtpServer;
            SMTPTCPPort = smtpTCPPort;
        }

        public string SenderEmail { get; set; }

        public string SenderPassword { get; set; }

        public string SMTPServer { get; set; }

        public int SMTPTCPPort { get; set; }
    }
}

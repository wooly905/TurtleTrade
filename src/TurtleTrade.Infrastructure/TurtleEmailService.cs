using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using TurtleTrade.Abstraction.Config;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.Infrastructure
{
    public class TurtleEmailService : INofiticationService
    {
        private const string _workerKind = "TurtleEmailService";
        private readonly ITurtleLogger _logger;
        private readonly ISMTPInfo _smtpInfo;

        public TurtleEmailService(ITurtleLogger logger, ISMTPInfo smtpInfo)
        {
            _logger = logger;
            _smtpInfo = smtpInfo;
        }

        public Task SendEmailAsync(CountryKind country, DateTime time, IEmailTemplate template)
        {
            return Task.Run(() =>
            {
                if (template == null)
                {
                    return;
                }

                if (string.IsNullOrEmpty(template.ReceipentEmail))
                {
                   _logger?.WriteToErrorLogAsync(country, time, _workerKind, new Exception("template.ReceipentEmail is null or empty."));
                    return;
                }

                if (_smtpInfo == null)
                {
                    _logger?.WriteToErrorLogAsync(country, time, _workerKind, new Exception("SMTPInfo is emtpy so email can't be sent."));
                    return;
                }

                try
                {
                    SmtpClient client = new SmtpClient(_smtpInfo.SMTPServer, _smtpInfo.SMTPTCPPort)
                    {
                        Credentials = new NetworkCredential(_smtpInfo.SenderEmail, _smtpInfo.SenderPassword),
                        EnableSsl = true
                    };

                    MailMessage body = new MailMessage(_smtpInfo.SenderEmail, template.ReceipentEmail, template.Subject, template.HtmlContent)
                    {
                        IsBodyHtml = true,
                        BodyEncoding = Encoding.UTF8
                    };

                    client.SendAsync(body, null);
                    _logger.WriteToEmailLogAsync(country, time, _workerKind, template.HtmlContent);
                }
                catch (Exception ex)
                {
                    _logger?.WriteToErrorLogAsync(country, time, _workerKind, ex);
                }
            });
        }
    }
}

using System;
using System.Threading.Tasks;

namespace TurtleTrade.Abstraction.Utilities
{
    public interface INofiticationService
    {
        Task SendEmailAsync(CountryKind country, DateTime time, IEmailTemplate template);
    }
}

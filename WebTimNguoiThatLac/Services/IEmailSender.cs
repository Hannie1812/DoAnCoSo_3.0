using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity.UI.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
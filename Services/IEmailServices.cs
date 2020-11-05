using AspNetCore_JWT.Models;
using System.Threading.Tasks;

namespace AspNetCore_JWT.Services
{
    public interface IEmailServices
    {
        //Task<EmailResponse> SendEmailBySendGridAsync(string email, string assunto, string mensagem);

        Task<EmailResponse> SendEmailBySmtpAsync(string email, string assunto, string mensagem);
    }
}
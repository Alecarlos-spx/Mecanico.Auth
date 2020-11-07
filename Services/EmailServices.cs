using AspNetCore_JWT.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AspNetCore_JWT.Services
{
    public class EmailServices : IEmailServices
    {
        private readonly string EMAIL_ORIGEM;
        private readonly string EMAIL_SENHA;

        public EmailServices(IConfiguration configuration)
        {
            EMAIL_ORIGEM = configuration["EmailService:email_remetente"];
        
            EMAIL_SENHA = configuration["EmailService:email_senha"];
        }

        public async Task<EmailResponse> SendEmailBySmtpAsync(string email, string assunto, string mensagem)
        {
            using (var mensagemDeEmail = new MailMessage())
            {

                mensagemDeEmail.From = new MailAddress(EMAIL_ORIGEM);

                mensagemDeEmail.Subject = assunto;
                mensagemDeEmail.To.Add(email);
                mensagemDeEmail.Body = mensagem;
                mensagemDeEmail.IsBodyHtml = true;
                


                //SMTP - Simple Mail Transport Protocol

                using (var smtpClient = new SmtpClient())
                {

                    smtpClient.UseDefaultCredentials = true;
                    smtpClient.Credentials = new NetworkCredential(EMAIL_ORIGEM, EMAIL_SENHA);

                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.Host = "smtp.gmail.com";
                    smtpClient.Port = 587;
                    smtpClient.EnableSsl = true;

                    smtpClient.Timeout = 20_000;


                    //obj retorno
                    var retorno = new EmailResponse();
                    retorno.Enviado = true;

                    try
                    {

                        await smtpClient.SendMailAsync(mensagemDeEmail);
                    }
                    catch (SmtpException e)
                    {
                        retorno.Enviado = false;
                        retorno.error = ErrorResponse.FromEmail(e.StatusCode);
                    }

                    return retorno;


                }
            }
        }
    }


        //private SendGridOptions _sendGridOptions { get; }

        //public EmailServices(IOptions<SendGridOptions> sendGridOptions)
        //{
        //    _sendGridOptions = sendGridOptions.Value;
        //}

        //public async Task<EmailResponse> SendEmailBySendGridAsync(string email, string assunto, string mensagem)
        //{
        //    try
        //    {
        //        // buscar SendGrid key
        //        var client = new SendGridClient(_sendGridOptions.SendGridKey);

        //        // obj de email
        //        var msg = new SendGridMessage()
        //        {
        //            From = new EmailAddress(_sendGridOptions.FromEmail, _sendGridOptions.FromFullName),
        //            Subject = assunto,
        //            PlainTextContent = mensagem,
        //            HtmlContent = mensagem
        //        };

        //        // email do usuario
        //        msg.AddTo(new EmailAddress("alexandrecarlos2@gmail.com", email));

        //        // envio do email
        //        var responseSend = await client.SendEmailAsync(msg);

        //        // obj retorno
        //        var retorno = new EmailResponse();
        //        retorno.Enviado = true;

        //        // verificação de envio
        //        if (!responseSend.StatusCode.Equals(System.Net.HttpStatusCode.Accepted))
        //        {
        //            retorno.Enviado = false;
        //            retorno.error = ErrorResponse.FromEmail(responseSend);
        //        }

        //        return retorno;
        //    }
        //    catch (Exception ex)
        //    {
        //        return new EmailResponse()
        //        {
        //            Enviado = false,
        //            error = ErrorResponse.From(ex)
        //        };
        //    }
        //}

}


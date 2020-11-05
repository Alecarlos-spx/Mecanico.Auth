using AspNetCore_JWT.Models;
using AspNetCore_JWT.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace AspNetCore_JWT.Extensions
{
    public static class EmailServicesExtensions
    {
        //extensão IEmailServices
        //public static Task<EmailResponse> SendEmailResetPasswordAsync(this IEmailServices emailServices, string email, string link)
        //{
        //    return emailServices.SendEmailBySendGridAsync(email, "Reset Password",
        //        $"Por favor para resetar sua senha clique nesse link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>");
        //}

        //public static Task<EmailResponse> SendConfirmEmailAsync(this IEmailServices emailServices, string email, string link)
        //{
        //    var result = emailServices.SendEmailBySendGridAsync(email, "Confirmação de email",
        //        $"Por favor para confirmar seu email clique nesse link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>");
        //    return result;
        //}

    }
}

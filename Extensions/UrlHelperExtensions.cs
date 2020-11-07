using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore_JWT.Extensions
{
    public static class UrlHelperExtensions
    {
        // extensao IUrlHelper
        public static string ResetPasswordCallbackLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
        {
            return $"{scheme}://localhost:5000/api/v1/login/resetPassword?userId={userId}&code={code}";
        }

        public static string ConfirmEmailCallbackLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
        {
        //return $"{scheme}://localhost:5000/api/v1/Login/confirmEmail?userId={userId}&code={code}";


            return $"{scheme}://mecanicoweb.azurewebsites.net/api/v1/Login/confirmEmail?userId={userId}&code={code}";
        
        }
    }
}

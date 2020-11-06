using AspNetCore_JWT.Data;
using AspNetCore_JWT.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore_JWT.ConfigStartup
{
    public static class IdentityConfig
    {
        public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var statusApp = configuration.GetValue<string>("StatusApp");

            if (statusApp == "Producao")
            {
                services.AddDbContext<AuthDBContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("ProducaoConnection")));
            }
            else
            {
                services.AddDbContext<AuthDBContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            }
            services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AuthDBContext>()
                .AddDefaultTokenProviders();

            // JWT
            // acesso à seção do arq appSettings
            var appSettingsSection = configuration.GetSection("");



            if (statusApp == "Producao")
            {
                appSettingsSection = configuration.GetSection("AppSettingsProducao");
            }
            else
            {
                appSettingsSection = configuration.GetSection("AppSettings");
            }




            // config com parametros
            services.Configure<TokenSettings>(appSettingsSection);

            // cast para objeto
            var appSettings = appSettingsSection.Get<TokenSettings>();

            // chave gerada a partir do parametro
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = true;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = appSettings.ValidoEm,
                    ValidIssuer = appSettings.Emissor
                };
            });
            return services;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AspNetCore_JWT.DTO;
using AspNetCore_JWT.Extensions;
using AspNetCore_JWT.Models;
using AspNetCore_JWT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AspNetCore_JWT.Controllers
{
    [ApiVersion("1.0")]
    //[Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class LoginController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly TokenSettings _tokenSettings;
        private readonly IEmailServices _emailServices;

        public LoginController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager,
            IOptions<TokenSettings> tokenSettings, IEmailServices emailServices)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _tokenSettings = tokenSettings.Value;
            _emailServices = emailServices;
        }

        // método de teste de autoriação
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            return Ok("Teste autorização Ok!!!");
        }




        [HttpPost("cadastrar")]
        [AllowAnonymous]
        public async Task<ActionResult> Cadastrar(RegisterUserDTO registerUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new IdentityUser
            {
                UserName = registerUser.Email,
                Email = registerUser.Email,
                EmailConfirmed = false
            };

            //cria usuario na base com senha criptografada
            var result = await _userManager.CreateAsync(user, registerUser.Password);

            if (result.Succeeded)
            {
                await SendConfirmEmail(user);
                return Ok(result.Succeeded);
            }

            return BadRequest(ErrorResponse.FromIdentity(result.Errors.ToList()));
        }



        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginUserDTO loginUser)
        {
            //if (!ModelState.IsValid)
            //{
            //    var ListaErros = new List<string>();
            //    foreach (var values in ModelState.Values)
            //    {
            //        foreach (var erros in values.Errors)
            //        {
            //            ListaErros.Add(erros.ErrorMessage);
            //        }
            //    }
            //    return BadRequest(ListaErros);
            //}

            var result = await _signInManager.PasswordSignInAsync(loginUser.Email, loginUser.Password, false, true);

            if (result.Succeeded)
            {
                return Ok(await GerarJwt(loginUser.Email));
            }
            if (result.IsLockedOut)
            {
                return BadRequest(loginUser);
            }
            return NotFound(loginUser);
        }

        [HttpPost("logout")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        //requisição de redefinição de senha
        [HttpPost("RecuperarSenha")]
        [AllowAnonymous]
        public async Task<IActionResult> RecuperarSenha(ForgotPasswordDTO forgotPassword)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(forgotPassword.Email);
            if (user == null)
            {
                return NotFound($"Usuário '{forgotPassword}' não encontrado.");
            }
            else
            {
                var forgotMail = await ForgotMainPassword(user);
                if (forgotMail.Enviado)
                {
                    return Ok();
                }
                //return Unauthorized(forgotMail.error);
                return Unauthorized();
            }
        }


        //buscar dados através do usuário passado
        [HttpGet("resetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return BadRequest("Não foi possível resetar a senha");
            }

            var resetPassword = new ResetPasswordDTO();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Usuário ID '{userId}' não encontrado.");
            }
            else
            {
                resetPassword.Code = code;
                resetPassword.Email = user.Email;
                resetPassword.UserId = userId;
                return Ok(resetPassword);
            }
        }

        //buscar dados através do usuário passado
        [HttpGet("confirmEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return BadRequest("Não foi possível confirmar o email");
            }

            var confirmEmail = new ConfirmEmailDTO();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Usuário ID '{userId}' não encontrado.");
            }
            else
            {
                confirmEmail.Code = code;
                confirmEmail.Email = user.Email;
                confirmEmail.UserId = userId;
                var retorno = await _userManager.ConfirmEmailAsync(user, confirmEmail.Code);

                //buscar no banco de dados empresa o site para redirecionar

                var siteEmpresa = "https://localhost:44303/";

                return Redirect(siteEmpresa);
            }
        }



        // envio nova senha
        [HttpPost("resetPasswordConfirm")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordConfirm(ResetPasswordConfirmDTO resetPassword)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(resetPassword.Email);
            if (user == null)
            {
                return NotFound($"Ususário ID não encontrado.");
            }
            else
            {
                //reset senha Identity
                return Ok(await _userManager.ResetPasswordAsync(user, resetPassword.Code, resetPassword.Password));
            }
        }

        private async Task<LoginResponseDTO> GerarJwt(string email)
        {
            //buscar usuário na base
            var user = await _userManager.FindByEmailAsync(email);

            //busca claims
            var claims = await _userManager.GetClaimsAsync(user);

            //busca regras
            var userRoles = await _userManager.GetRolesAsync(user);

            //add nas claims
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName));

            //add roles
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim("role", userRole));
            }

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);

            //criar token
            var tokenHandler = new JwtSecurityTokenHandler();

            //chave - app settings
            var key = Encoding.ASCII.GetBytes(_tokenSettings.Secret);

            //criar token vaseado nas info
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _tokenSettings.Emissor,
                Audience = _tokenSettings.ValidoEm,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(_tokenSettings.ExpiracaoHoras),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            });

            //gerar código
            var encodedToken = tokenHandler.WriteToken(token);

            //add obj resposta
            var response = new LoginResponseDTO
            {
                AccessToken = encodedToken,
                ExpiresIn = TimeSpan.FromHours(2).TotalSeconds,
                UserToken = new UserTokenDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    Claims = claims.Select(c => new ClaimDTO { Type = c.Type, Value = c.Value })
                }

            };
            return response;
        }

        private async Task<EmailResponse> ForgotMainPassword(IdentityUser user)
        {
            //gerar JWT para reset de senha
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            //criar link para retorno
            var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, HttpUtility.UrlEncode(code), Request.Scheme);

            //método de extensão de URL
            return await _emailServices.SendEmailBySmtpAsync(user.Email, "Reset Password", callbackUrl);


        }

        private async Task<EmailResponse> SendConfirmEmail(IdentityUser user)
        {
            //gerar JWT para confirmação de email
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);



            //criar link para retorno
            var callbackUrl = "Link para confirmação de email do sistema de Auto Mecanica, clique e será redirecionado para pagina de confirmação. "
                + Url.ConfirmEmailCallbackLink(user.Id, HttpUtility.UrlEncode(code), Request.Scheme);

            //método de extensão de URL
            var result = await _emailServices.SendEmailBySmtpAsync(user.Email, "Confirmação de email", callbackUrl);
            return result;

        }


    }
}

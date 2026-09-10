using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace PortalTarefas.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Resposta genérica para não revelar a existência do e-mail (Atividade 6 - Item 4)
                return Unauthorized(new { Erro = "Credenciais inválidas." });
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!, 
                model.Password, 
                isPersistent: true, 
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var claims = await _userManager.GetClaimsAsync(user);

                return Ok(new
                {
                    Mensagem = "Login realizado com sucesso.",
                    Usuario = new
                    {
                        Email = user.Email,
                        Roles = roles,
                        Claims = claims.Select(c => new { c.Type, c.Value })
                    }
                });
            }

            if (result.IsLockedOut)
            {
                return StatusCode(StatusCodes.Status423Locked, new { Erro = "Conta temporariamente bloqueada por excesso de tentativas falhas." });
            }

            return Unauthorized(new { Erro = "Credenciais inválidas." });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { Mensagem = "Sessão encerrada com sucesso." });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized(new { Erro = "Usuário não autenticado." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { Erro = "Usuário não encontrado." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            return Ok(new
            {
                Email = user.Email,
                Roles = roles,
                Claims = claims.Select(c => new { c.Type, c.Value })
            });
        }
    }

    public record LoginRequest(string Email, string Password);
}
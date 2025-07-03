using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace APICatalogo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)] 
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        public AuthController(
            ITokenService tokenService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }


        [HttpPost]
        [Route("CreateRole")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            var roleExist = await _roleManager.RoleExistsAsync(roleName);

            if (!roleExist)
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));

                if (roleResult.Succeeded)
                {
                    _logger.LogInformation(1, "Role adicionada");
                    return StatusCode(StatusCodes.Status200OK, new Response
                    {
                        Status = "Success",
                        Message = $"Role {roleName} criada com sucesso!"
                    });
                }
                else
                {
                    _logger.LogError(1, "Erro ao adicionar role");
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response
                    {
                        Status = "Error",
                        Message = $"Erro ao criar role {roleName}!"
                    });
                }
            }
            return StatusCode(StatusCodes.Status400BadRequest, new Response
            {
                Status = "Error",
                Message = $"Role {roleName} já existe!"
            });

        }

        [HttpPost]
        [Route("AddRoleToUser")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> AddUserToRole(string email, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if(user != null)
            {
                var result = await _userManager.AddToRoleAsync(user, roleName);

                if(result.Succeeded)
                {
                    _logger.LogInformation(1, $"Usuário {user.UserName} adicionado à role {roleName}");
                    return StatusCode(StatusCodes.Status200OK, new Response
                    {
                        Status = "Success",
                        Message = $"Usuário {user.UserName} adicionado à role {roleName} com sucesso!"
                    });
                }
                else
                {
                    _logger.LogError(1, $"Erro ao adicionar usuário {user.UserName} à role {roleName}");
                    return StatusCode(StatusCodes.Status400BadRequest, new Response
                    {
                        Status = "Error",
                        Message = $"Erro ao adicionar usuário {user.UserName} à role {roleName}!"
                    });
                }
            }
            return BadRequest(new {error = "Usuário não encontrado." });
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModelDTO modelDTO)
        {
            var user = await _userManager.FindByNameAsync(modelDTO.UserName!);

            if (user is not null && await _userManager.CheckPasswordAsync(user, modelDTO.Password!))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims =  new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim("id", user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach(var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = _tokenService.GenerateAcessToken(authClaims, _configuration);

                var refreshToken = _tokenService.GenerateRefreshToken();

                _ = int.TryParse(_configuration["Jwt:RefreshTokenValidityInMinutes"], out int refreshTokenValidityInMinutes);

                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(refreshTokenValidityInMinutes);
                
                user.RefreshToken = refreshToken;

                await _userManager.UpdateAsync(user);

                return Ok(new
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    Expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModelDTO modelDTO)
        {
            var userExists = await _userManager.FindByNameAsync(modelDTO.UserName!);

            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response
                {
                    Status = "Error",
                    Message = "Usuário já existe!"
                });
            }

            ApplicationUser user = new ApplicationUser()
            {
                Email = modelDTO.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = modelDTO.UserName!
            };

            var result = await _userManager.CreateAsync(user, modelDTO.Password!);

            if(!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response
                {
                    Status = "Error",
                    Message = "Erro ao criar usuário!"
                });
            }
            return Ok(new Response {
                Status = "Success",
                Message = "Usuário criado com sucesso!"
            });
        }
        [HttpPost]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken(TokenModel tokenModel)
        {
            if (tokenModel is null)
            {
                return BadRequest("Token inválido.");
            }

            string? acessToken = tokenModel.AccessToken ?? throw new ArgumentNullException(nameof(tokenModel));

            string? refreshToken = tokenModel.RefreshToken ?? throw new ArgumentNullException(nameof(tokenModel));

            var principal = _tokenService.GetPrincipalFromExpiredToken(acessToken!, _configuration);

            if (principal is null)
            {
                return BadRequest("Token / refresh token inválido.");
            }

            string userName = principal.Identity?.Name;

            var user = await _userManager.FindByNameAsync(userName!);

            if(user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return BadRequest("Token / refresh token inválido.");
            }

            var newAacessToken = _tokenService.GenerateAcessToken(principal.Claims.ToList(), _configuration);

            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;

            await _userManager.UpdateAsync(user);

            return new ObjectResult(new
            {
                acessToken = new JwtSecurityTokenHandler().WriteToken(newAacessToken),
                refreshToken = newRefreshToken,
            });
        }
        [Authorize]
        [HttpPost]
        [Route("revoke/{username}")]
        [Authorize(Policy = "ExclusiveOnly")]
        public async Task<IActionResult> Revoke(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            user.RefreshToken = null;

            await _userManager.UpdateAsync(user);

            return NoContent();
        }

       
    }
}

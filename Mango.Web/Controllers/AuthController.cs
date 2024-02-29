using IdentityModel;
using Mango.Web.Models;
using Mango.Web.Models.Utility;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Mango.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        public AuthController(IAuthService authService, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
        }

        [HttpGet]
        public IActionResult Login()
        {
            LoginRequestDto loginRequestDto = new();
            return View(loginRequestDto);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
        {
            ResponseDto responseDto = await _authService.LoginAsync(loginRequestDto);

            if (responseDto != null && responseDto.IsSuccess)
            {
                LoginResponseDto loginResponseDto = JsonConvert.DeserializeObject<LoginResponseDto>(Convert.ToString(responseDto.Result));
                await SignInUser(loginResponseDto);
                _tokenProvider.SetToken(loginResponseDto.Token);
                return RedirectToAction("Index", "Home");
            }
            else
            { 
                TempData["error"] = responseDto.Message;
                return View(loginRequestDto);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            var roleList = new List<SelectListItem>()
            {
                new SelectListItem{Text=SD.RoleAdmin, Value=SD.RoleAdmin},
                new SelectListItem{Text=SD.RolerCustomer, Value=SD.RolerCustomer},
            };
            ViewBag.RoleList = roleList;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegistrationRequestDto registrationRequestDto)
        {
            ResponseDto responseDto = await _authService.RegisterAsync(registrationRequestDto);
            ResponseDto assignRole;
            if(responseDto != null && responseDto.IsSuccess) {
                if(string.IsNullOrEmpty(registrationRequestDto.Role))
                {
                    registrationRequestDto.Role = SD.RolerCustomer;
                }
                assignRole = await _authService.AssignRoleAsync(registrationRequestDto);
                if(assignRole != null && assignRole.IsSuccess)
                {
                    TempData["success"] = "Registration Successful";
                    return RedirectToAction(nameof(Login));
                }
            }
            else
            {
                TempData["error"] = responseDto.Message;
            }
            var roleList = new List<SelectListItem>()
            {
                new SelectListItem{Text=SD.RoleAdmin, Value=SD.RoleAdmin},
                new SelectListItem{Text=SD.RolerCustomer, Value=SD.RolerCustomer},
            };
            ViewBag.RoleList = roleList;
            return View(registrationRequestDto);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            _tokenProvider.ClearToken();
            return RedirectToAction("Index", "Home");
        }

        private async Task SignInUser(LoginResponseDto loginResponseDto)
        {
            var handler = new JwtSecurityTokenHandler();

            var jwt = handler.ReadJwtToken(loginResponseDto.Token);

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

            identity.AddClaim(new Claim(JwtClaimTypes.Email, 
                jwt.Claims.FirstOrDefault(u => u.Type == JwtClaimTypes.Email).Value));

            identity.AddClaim(new Claim(JwtClaimTypes.Subject,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtClaimTypes.Subject).Value));

            identity.AddClaim(new Claim(JwtClaimTypes.Name,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtClaimTypes.Name).Value));

            identity.AddClaim(new Claim(JwtClaimTypes.Email,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtClaimTypes.Email).Value));

            identity.AddClaim(new Claim(JwtClaimTypes.Role,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtClaimTypes.Role).Value));

            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
    }
}

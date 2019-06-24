﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DempApiApp.Helpers;
using DempApiApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DempApiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AppSettings _appSettings;
        public AccountController(UserManager<IdentityUser> userManager,
                                 SignInManager<IdentityUser> signInManager,
                                IOptions<AppSettings> appSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appSettings = appSettings.Value;
        }

        //Register
        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel formData)
        {
            // will hold the errors related to register
            List<string> errorList = new List<string>();
            var user = new IdentityUser
            {
                Email = formData.Email,
                UserName = formData.Username,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, formData.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");

                //sending confirmation email
                return Ok(new { username = user.UserName, email = user.Email, status = 1, message = "Registration Successful" });
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                    errorList.Add(error.Description);
                }
                return BadRequest(new JsonResult(errorList));
            }

        }

        //Login
        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody]LoginViewModel formData)
        {
            // get user from the dataabse
            var user = await _userManager.FindByNameAsync(formData.Username);

            var roles = await _userManager.GetRolesAsync(user);

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.Secret));

            double tokenExpiry = Convert.ToDouble(_appSettings.ExpireTime);

            if (user != null && await _userManager.CheckPasswordAsync(user, formData.Password))
            {
                //confirmation of Email
                var TokenHandler = new JwtSecurityTokenHandler();
                var TokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[] {
                        new Claim(JwtRegisteredClaimNames.Sub, formData.Username),
                        new Claim(JwtRegisteredClaimNames.Sub, formData.Username),
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                        new Claim("LoggedOn", DateTime.Now.ToString())
                    }),
                    SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _appSettings.Site,
                    Audience = _appSettings.Audience,
                    Expires = DateTime.UtcNow.AddMinutes(tokenExpiry)
                };
                //Generate Token JWT

                var token = TokenHandler.CreateToken(TokenDescriptor);
                return Ok(new { token=TokenHandler.WriteToken(token)
                    ,expiration=token.ValidTo
                    ,username=user.UserName
                    ,userRole=roles.FirstOrDefault()});
            }

            // return error
            ModelState.AddModelError("", "Username / Password was not found");
            return Unauthorized(new { LoginError = "Please check the login credentials - Invalid Username/Password entered" });
        }
    }
}
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController(DataContext dbContext, ITokenService tokenService) : BaseApiController
    {


        [HttpPost("register")] // account/register
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {

            if(await UserExist(registerDto.Username))
            {
                return BadRequest("Username is taken");
            }
            return Ok();
            // using var hmac = new HMACSHA512();

            // var user = new AppUser{
            //     UserName = registerDto.Username.ToLower(),
            //     PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            //     PasswordSalt = hmac.Key
            // };

            // dbContext.Users.Add(user);
            // await dbContext.SaveChangesAsync();        

            //             var response =  new UserDto
            // {
            //     Username = user.UserName,
            //     token = tokenService.CreateToken(user)
            // };

            // return response;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());

            if(user == null)
            {
                return Unauthorized("Invalid username");
            }

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedhash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedhash.Length; i++)
            {
                if(computedhash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }

            var response =  new UserDto
            {
                Username = user.UserName,
                token = tokenService.CreateToken(user)
            };

            return response;
        }

        private async Task<bool> UserExist(string UserName)
        {
            return await dbContext.Users.AnyAsync(u => u.UserName.ToLower() == UserName.ToLower());
        }
    }
}

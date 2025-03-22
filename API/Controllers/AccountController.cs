using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController(DataContext dbContext) : BaseApiController
    {


        [HttpPost("register")] // account/register
        public async Task<ActionResult<AppUser>> Register(RegisterDto registerDto)
        {

            if(await UserExist(registerDto.Username))
            {
                return BadRequest("Username is taken");
            }
            
            using var hmac = new HMACSHA512();

            var user = new AppUser{
                UserName = registerDto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();        

            return user;
        }

        private async Task<bool> UserExist(string UserName)
        {
            return await dbContext.Users.AnyAsync(u => u.UserName.ToLower() == UserName.ToLower());
        }
    }
}

using System.Net.Mail;
using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Validations;

namespace API.Controllers
{
    [Authorize]
    public class UsersController
    (IUserRepository userRepository,
     IMapper mapper,
     IPhotoService photoService
    ) : BaseApiController
    {

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await userRepository.GetMembersAsync();

            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await userRepository.GetMemberAsync(username);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MembersUpdateDto membersUpdateDto)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());

            if(user == null) return BadRequest("Could not find user");

            mapper.Map(membersUpdateDto, user);

            if(await userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update the user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)  
        {
            var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());

            if(user == null) return BadRequest("Could not find user");

            var results = await photoService.AddPhotoAsync(file);

            if(results.Error != null) return BadRequest(results.Error.Message);

            var photo = new Photo
            {
                Url = results.SecureUrl.AbsoluteUri,
                PublicId = results.PublicId
            };

            user.Photos.Add(photo);

            if(await userRepository.SaveAllAsync()) 
            {
                return CreatedAtAction(nameof(GetUser), new {username = user.UserName},  mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Problem adding photo");
        }
    }
}

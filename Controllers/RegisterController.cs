using JWTDemo.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JWTDemo.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public RegisterController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
        }

        [HttpPost]
        public async Task<ActionResult<RegisterResult>> Register([FromBody] RegisterModel model)
        {
            var newUser = new IdentityUser { UserName = model.Email, Email = model.Email };

            var checkEx = await _userManager.FindByEmailAsync(newUser.Email!);
            if (checkEx != null)
            {
                return BadRequest(new RegisterResult { Successful = false, Message = "Error occured, email cannot be used" });
            }

            var result = await _userManager.CreateAsync(newUser, model.Password!);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(x => x.Description);
                return Ok(new RegisterResult { Successful = false, Message = errors.ToString() });
            }

            if (newUser.Email!.ToLower().StartsWith("admin"))
            {
                // check if any of the emails contain admin, then assign the currentemail as user
                var user = await signInManager.UserManager.Users.Where(x => x.Email!.Contains("admin")).ToListAsync();
                if (user.Count() > 1)
                {
                    await _userManager.AddToRoleAsync(newUser, "User");
                    return Ok(new RegisterResult { Successful = true });
                }
                await _userManager.AddToRoleAsync(newUser, "Admin");
                return Ok(new RegisterResult { Successful = true });
            }
            await _userManager.AddToRoleAsync(newUser, "User");
            return Ok(new RegisterResult { Successful = true });
        }

        [HttpGet]
        public async Task<ActionResult<List<RoleModel>>> GetRoles()
        {

            List<RoleModel> roleModels = new();
            var allRoles = await roleManager.Roles.ToListAsync();
            foreach (var role in allRoles)
            {
                var roleModel = new RoleModel();
                roleModel.Name = role.Name;
                roleModel.StringId = role.Id;
                roleModels.Add(roleModel);
            }
            return roleModels;
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<UserModel>>> GetUsers()
        {
            List<UserModel> userModels = new();
            var allUsers = await _userManager.Users.ToListAsync();
            foreach (var role in allUsers)
            {
                var userModel = new UserModel();
                userModel.Name = role.UserName;
                userModel.UserId = role.Id;
                userModels.Add(userModel);
            }
            return userModels;
        }

        [HttpPost("assign-role")]
        public async Task<ActionResult<RegisterResult>> AssignRole(UserRoleModel model)
        {
            if (model != null)
            {
                var user = await signInManager.UserManager.FindByEmailAsync(model.UserName!);
                if (user != null)
                {
                    //get all user roles
                    var userRoles = await _userManager.GetRolesAsync(user);
                    if (userRoles.Count() > 0)
                    {
                        await signInManager.UserManager.RemoveFromRolesAsync(user, userRoles);
                    }
                    await signInManager.UserManager.AddToRoleAsync(user, model.RoleName!);
                    return Ok(new RegisterResult { Successful = true, Message = "Role successfully assigned" });
                }
            }
            return BadRequest(new RegisterResult { Successful = false, Message = "Error occured" });
        }
    }
}

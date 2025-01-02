using Elfie.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Form.Migrations;
using Web_Form.Models;

namespace Web_Form.Controllers
{
    
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UserManagementController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
        }
        [HttpGet]
        public async Task<IActionResult> GetTableData()
        {
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var currentUser = await userManager.GetUserAsync(User);

            if (currentUser != null && !await userManager.IsInRoleAsync(currentUser, "Admin"))
            {
                return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
            }

            //var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var users = await userManager.Users.OrderByDescending(x=> x.IsAdmin).ToListAsync();

            return Json(users);
        }
        

        [HttpGet]
        public async Task<IActionResult> ManageUser()
        {
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var currentUser = await userManager.GetUserAsync(User);

            if (currentUser != null && !await userManager.IsInRoleAsync(currentUser, "Admin"))
            {
                return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
            }
            var Currentuser = await userManager.GetUserAsync(User);
            if (!signInManager.IsSignedIn(User) && await userManager.IsInRoleAsync(Currentuser, "Admin"))
            {
                return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string[] email)
        {
            var Currentuser = await userManager.GetUserAsync(User);
            if (!signInManager.IsSignedIn(User) && await userManager.IsInRoleAsync(Currentuser, "Admin"))
            {
                return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
            }
            if (email.Count() > 0)
            {
                foreach (var id in email)
                {
                    var user = await userManager.FindByEmailAsync(id);
                    if (user != null)
                    {
                        var result = await userManager.DeleteAsync(user);


                    }
                }
            }
            return RedirectToAction("ManageUser");
        }
        [HttpPost]
        public async Task<IActionResult> BlockUser(string[] email)
        {
            var Currentuser = await userManager.GetUserAsync(User);
            if (!signInManager.IsSignedIn(User) && await userManager.IsInRoleAsync(Currentuser, "Admin"))
            {
                return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
            }
            if (email.Length > 0)
            {
                foreach (var id in email)
                {
                    var user = await userManager.FindByEmailAsync(id);
                    if (user != null)
                    {

                        if (user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow)
                        {
                            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);

                            await userManager.UpdateAsync(user);

                        }


                        var currentUserEmail = userManager.GetUserName(User);
                        var currentUser = await userManager.FindByEmailAsync(currentUserEmail);
                        if (currentUser != null && currentUser.LockoutEnd > DateTimeOffset.UtcNow)
                        {
                            await signInManager.SignOutAsync();

                        }

                    }
                }
            }
            Response.Redirect(Request.Path);
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> UnBlockUser(string[] email)
        {
            var Currentuser = await userManager.GetUserAsync(User);
            if (!signInManager.IsSignedIn(User) && await userManager.IsInRoleAsync(Currentuser, "Admin"))
            {
                return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
            }
            if (email.Count() > 0)
            {
                foreach (var id in email)
                {
                    var user = await userManager.FindByEmailAsync(id);

                    if (user != null && await userManager.IsLockedOutAsync(user))
                    {
                        user.LockoutEnd = null;
                        await userManager.UpdateAsync(user);


                    }
                }
            }
            return View("ManageUser");
        }
        [HttpPost]
        public async Task<IActionResult> MakeAdmin(string[] email)
        {
            var Currentuser = await userManager.GetUserAsync(User);
            if (!signInManager.IsSignedIn(User) && await userManager.IsInRoleAsync(Currentuser, "Admin"))
            {
                return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
            }
            if (email.Count() > 0)
            {
                foreach (var id in email)
                {
                    var user = await userManager.FindByEmailAsync(id);
                    var role = await roleManager.RoleExistsAsync("Admin");

                    if (user != null && role)
                    {
                      var result =  await userManager.AddToRoleAsync(user, "Admin");
                        if(result.Succeeded)
                        {
                            user.IsAdmin = true;
                           await userManager.UpdateAsync(user);
                        }


                    }
                }
            }
            return View("ManageUser");
        }
        [HttpPost]
        public async Task<IActionResult> RemoveFromAdmin(string[] email)
        {
            var Currentuser = await userManager.GetUserAsync(User);
            if (!signInManager.IsSignedIn(User) && await userManager.IsInRoleAsync(Currentuser, "Admin"))
            {
                return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
            }
            if (email.Count() > 0)
            {
                foreach (var id in email)
                {
                    var user = await userManager.FindByEmailAsync(id);
                    var role = await roleManager.RoleExistsAsync("Admin");

                    if (user != null && await userManager.IsInRoleAsync(user, "Admin"))
                    {
                       var result = await userManager.RemoveFromRoleAsync(user, "Admin");
                        
                        if (result.Succeeded)
                        {
                            user.IsAdmin = false;
                            await userManager.UpdateAsync(user);

                            // Invalidate the user's authentication cookie
                            //await signInManager.SignOutAsync();
                            await signInManager.SignInAsync(user, isPersistent: false);
                        }
                    
                    }
                }
            }
            return View("ManageUser");
        }


    }
}
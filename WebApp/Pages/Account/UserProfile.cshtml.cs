using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebApp.Data.Account;

namespace WebApp.Pages.Account
{
    public class UserProfileModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        [BindProperty]
        public UserProfileViewModel UserProfile { get; set; }
        [BindProperty]
        public string? Success { get; set; }

        public UserProfileModel(UserManager<User> userManager)
        {
            _userManager = userManager;
            UserProfile = new UserProfileViewModel();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Success = string.Empty;

            var (user, departmentClaim, positionClaim) = await GetUserInfoAsync();

            if (user != null)
            {
                UserProfile.Email = User.Identity?.Name ?? string.Empty;
                UserProfile.Department = departmentClaim?.Value ?? string.Empty;
                UserProfile.Position = positionClaim?.Value ?? string.Empty;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            try
            {
                var (user, departmentClaim, positionClaim) = await GetUserInfoAsync();

                if (user != null && departmentClaim != null)
                {
                   await _userManager.ReplaceClaimAsync(user, departmentClaim, new Claim(departmentClaim.Type, UserProfile.Department));
                }

                if (user != null && positionClaim != null)
                {
                   await _userManager.ReplaceClaimAsync(user, positionClaim, new Claim(positionClaim.Type, UserProfile.Position));
                }
            }
            catch
            {
                ModelState.AddModelError("UserProfile", "Error occured during updating user profile.");
            }

            Success = "The user profile is updated successfully.";

            return Page();
        }

        public async Task<(User? user, Claim? departmentClaim, Claim? positionClaim)> GetUserInfoAsync()
        {
            var user = await _userManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

            if (user != null)
            {
                var claims = await _userManager.GetClaimsAsync(user);
                var departmentClaim = claims.FirstOrDefault(x => x.Type == "Department");
                var positionClaim = claims.FirstOrDefault(x => x.Type == "Position");

                return (user, departmentClaim, positionClaim);
            }
            else
            {
                return (null, null, null);
            }
        }
    }

    public class UserProfileViewModel
    {
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Department { get; set; } = string.Empty;
        [Required]
        public string Position { get; set; } = string.Empty;
    }
}

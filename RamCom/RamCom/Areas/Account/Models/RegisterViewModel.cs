using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Business.CustomValidations;

namespace RamCom.Areas.Account.Models
{
    public class RegisterViewModel : LoginViewModel
    {
        [Required(ErrorMessage = "First Name is required.")]
        [DisplayName("First Name")]
        [RegularExpression(@"^[A-Za-z]{1,30}$", ErrorMessage = "Invalid First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [DisplayName("Last Name")]
        [RegularExpression(@"^[A-Za-z]{1,30}$", ErrorMessage = "Invalid Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email Address is required.")]
        [DisplayName("Email Address")]
        [EmailAddress(ErrorMessage = "Invalid Email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; }


        [Required(ErrorMessage = "Phone number is required.")]
        [DisplayName("Phone number")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Invalid Phone number")]
        public string PhoneNumber { get; set; }
        [ValidateNever]
        public List<SelectListItem> UserRoles { get; set; }

        public bool IsUpdateMode { get; set; }

        [RequiredIf("IsUpdateMode", ConditionValue ="True", ErrorMessage = "Please provide current password")]
        public string? CurrentPassword { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}

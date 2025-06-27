using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RamCom.Areas.Account.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "User Name is required.")]
        [DisplayName("User Name")]
        [RegularExpression(@"^[A-Za-z0-9_]{6,30}$", ErrorMessage = "Invalid User Name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*])[a-zA-Z0-9!@#$%^&*]{8,16}$", ErrorMessage = "Password is not in accepted format")]
        public string Password { get; set; }
    }
}

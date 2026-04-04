using System.ComponentModel.DataAnnotations;

namespace MinesweeperApp.Models
{
    // This view model matches the Login form fields.
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace MinesweeperApp.Models
{
    // This model represents one registered user in the database.
    public class UserModel
    {
        // Primary key for the database table
        public int Id { get; set; }

        // User first name
        [Required]
        public string FirstName { get; set; }

        // User last name
        [Required]
        public string LastName { get; set; }

        // User sex
        [Required]
        public string Sex { get; set; }

        // User age
        [Required]
        [Range(1, 120)]
        public int Age { get; set; }

        // User state
        [Required]
        public string State { get; set; }

        // User email address
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        // Username used for login
        [Required]
        public string Username { get; set; }

        // Stores the hashed password instead of plain text
        [Required]
        public string PasswordHash { get; set; }

        // Stores the random salt used for hashing
        [Required]
        public string Salt { get; set; }
    }
}

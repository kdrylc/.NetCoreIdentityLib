using System.ComponentModel.DataAnnotations;

namespace NetCoreIdentity.DTO
{
    public class RegisterModelDTO
    {
        [Required]
        public string FirstName { get; set; }
        [Required]

        public string LastName { get; set; }
        [Required]

        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]

        public string ConfirmPassword { get; set; }
        [Required]

        public string Email { get; set; }
    }
}

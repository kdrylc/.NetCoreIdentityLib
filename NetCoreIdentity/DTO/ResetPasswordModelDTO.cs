using System.ComponentModel.DataAnnotations;

namespace NetCoreIdentity.DTO
{
    public class ResetPasswordModelDTO
    {
        [Required]
        public string Token { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]

        public string Password { get; set; }
    }
}

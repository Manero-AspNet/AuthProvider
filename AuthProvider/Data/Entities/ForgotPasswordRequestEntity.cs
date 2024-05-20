using System.ComponentModel.DataAnnotations;

namespace AuthProvider.Data.Entities
{
    public class ForgotPasswordRequestEntity
    {
        [Key]
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
        public DateTime ExpirationDate { get; set; } = DateTime.Now.AddMinutes(5);
    }
}



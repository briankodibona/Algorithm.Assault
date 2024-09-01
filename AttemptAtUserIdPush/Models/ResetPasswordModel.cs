using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AttemptAtUserIdPush.Models;

public class ResetPasswordModel
{
    [Required]
    [EmailAddress]
    public string email { get; set; }
    
    [PasswordPropertyText]
    [Required]
    public string newPassword { get; set; }
    
    [PasswordPropertyText]
        [Required]
    public string confirmPassword { get; set; }
    
}
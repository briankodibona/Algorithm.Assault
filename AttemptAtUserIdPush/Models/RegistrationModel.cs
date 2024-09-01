using System.ComponentModel.DataAnnotations;

namespace AttemptAtUserIdPush.Models;

public class RegistrationModel
{
    [Required]
    public string first_name { get; set; }
    
    [Required]
    public string last_name { get; set; }
    
    [Required]
    public string email { get; set; }
    
    [Required]
    public string phone_number { get; set; }
    
    [Required]
    public DateTime birthday { get; set; }
    
    [Required]
    public string password { get; set; }
    
}
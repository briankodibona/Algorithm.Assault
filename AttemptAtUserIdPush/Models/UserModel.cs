using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AttemptAtUserIdPush.Models;

public class UserModel
{
    [Required]
    public int Id { get; set; }
    
    
    [Required]
    public string first_name { get; set; }
    
    [Required]
    public string last_name { get; set; }
    
    [Required]
    [EmailAddress]
    public string email { get; set; }
    
    [Required]
    [Phone]
    public string phone_number { get; set; }
    
    [Required]
    [Range(typeof(DateTime), "01/01/1920", "01/01/2999")]
    public DateTime birthday { get; set; }
    
    [JsonIgnore]
    public string hashed_password { get; set; }
    
    [JsonIgnore]
    public string salt { get; set;}
    
}
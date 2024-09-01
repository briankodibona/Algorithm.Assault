using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AttemptAtUserIdPush.Models;

public class customUserIdModel
{
    
    [JsonIgnore]
    public string userId { get; set; }
    
    [Key]
    public string email { get; set; }
}
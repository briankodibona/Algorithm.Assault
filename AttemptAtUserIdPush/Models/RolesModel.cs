using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AttemptAtUserIdPush.Models;


public class RolesModel
{
    
    public bool Admin { get; set; }
    public bool normalUser { get; set; }
    
    [Key]
    [JsonIgnore]
    public string userId { get; set; }
    
}
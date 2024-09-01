using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AttemptAtUserIdPush.Models;

public class TimeMangementModel
{
    [Key]
    [JsonIgnore]
    public string userId { get; set; }
    
    
    [Required]
    public DateTime dayBeingCaptured { get; set; }
    
    [Required]
    public string totalHours { get; set; }
    
}
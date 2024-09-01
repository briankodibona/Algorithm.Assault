using System.Text.Json.Serialization;

namespace AttemptAtUserIdPush.Models;

public class UserRecordReturnByEmail
{
    [JsonIgnore]
    public int customId { get; set; }
    public string Email { get; set; }
}
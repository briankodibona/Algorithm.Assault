using System.IdentityModel.Tokens.Jwt;

namespace AttemptAtUserIdPush.Methods;

public class JWTHelpingMethods
{
    
    // Method that retrives Guid from JWT
    public static Guid? GetGuidFromJwt(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var guidClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "Sid");
        return guidClaim != null ? Guid.Parse(guidClaim.Value) : (Guid?)null;
    }
}
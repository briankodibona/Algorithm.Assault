using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AttemptAtUserIdPush.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using AttemptAtUserIdPush.InMemoryDB;
using AttemptAtUserIdPush.Methods;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


namespace AttemptAtUserIdPush.Controllers;

public class TimeCapturingController : Controller
{
    private readonly ApplicationDbContext _context;

    public TimeCapturingController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("api/time/capturing{userId}")]
    public async Task<IActionResult> SubmitShiftTimes(string userId, [FromBody] TimeMangementModel timeMangementModel)
    {
        //Checks if info in valid format
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid request1");
        }
        
        //Strips JWT 
        var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
        var token = authorizationHeader?.Replace("Bearer ", "");
        
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized("Invalid token1");
        }

        //Extract ID from JWT
        var guid = JWTHelpingMethods.GetGuidFromJwt(token);

        if (!guid.HasValue)
        {
            return Unauthorized("Invalid token2");
        }
        
        //Lookup User In DB
        var user = await _context.CustomUsersIdModels.FindAsync(guid.Value);
        
        if (user == null)
        {
            return BadRequest("User not found");
        }
        
        //Finds userId
        var userIdHolder = await _context.CustomUsersIdModels.FindAsync(userId);

        if (userIdHolder == null)
        {
            return BadRequest("Custom User Id not found");
        }

        timeMangementModel.userId = userIdHolder.userId;
        timeMangementModel.totalHours = timeMangementModel.totalHours;
           
        //Updates database and saves changes
        _context.TimeMangementModels.Add(timeMangementModel);
        await _context.SaveChangesAsync();
        
        return Ok(timeMangementModel);
    }
    
    private string hoursWorked(DateTime startTime, DateTime endTime)
    {
        var shiftLength = endTime - startTime;
        
        return shiftLength.ToString();
    }
}
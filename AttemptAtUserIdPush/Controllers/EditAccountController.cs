using AttemptAtUserIdPush.InMemoryDB;
using AttemptAtUserIdPush.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AttemptAtUserIdPush.Methods;

namespace AttemptAtUserIdPush.Controllers;

public class EditAccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public EditAccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    //FInd a user ID based on their email
    [HttpGet("user-by-email")]
    public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
    {
        //Checks if email is not null
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest("Email cannot be empty");
        }
        
        //Query to find the user
        var user = _context.Users.Where(u => u.email == email).Select(u => new UserRecordReturnByEmail
        {
            customId = u.Id,
            Email = u.email,
        }).FirstOrDefault();

        if (user == null)
        {
            return NotFound();
        }
        
        return Ok(user);
    }
    
    
    
    [HttpPost("Edit Account/{id}")]
    public async Task<IActionResult> EditAccountInformation(int id,
        [FromBody] EditUserInformationModel editUserInformationModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid Information");
        }
        
        //Extract and validate JWT
        var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
        var token = authorizationHeader?.Replace("Bearer ", "");

        //var customId = _context.CustomUsersIdModels.FindAsync(id);
        
        
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized("Invalid token1");
        }
        
        var guid = JWTHelpingMethods.GetGuidFromJwt(token);

        //Tests if there is a custom ID
        if (!guid.HasValue)
        {
            return Unauthorized("Invalid token2");
        }

        //Test if GUID matches 
        if (guid.Value != Guid.Empty)
        {
            return Forbid("You are not allowed to edit this account");
        }
        
        var userAccount = await _context.Users.FindAsync(id);

        //Tests if account exists
        if (userAccount == null)
        {
            return NotFound("User not found");
        }

        //Updates values and sends to database
        userAccount.birthday = editUserInformationModel.Birthday; 
        userAccount.email = editUserInformationModel.Email;
        userAccount.first_name = editUserInformationModel.FirstName;
        userAccount.last_name = editUserInformationModel.LastName;
        userAccount.phone_number = editUserInformationModel.PhoneNumber;
        userAccount.hashed_password = userAccount.hashed_password;
        userAccount.salt = userAccount.salt;
        
        //Save changes
        await _context.SaveChangesAsync();

            return Ok(userAccount);

        }
}
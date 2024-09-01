using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AttemptAtUserIdPush.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using AttemptAtUserIdPush.InMemoryDB;
using AttemptAtUserIdPush.Methods;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


namespace AttemptAtUserIdPush.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    //Sets up configuration and database variables
    public AccountController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }


    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegistrationModel registrationModel)
    {
        //Tests if info entered is valid
        if (ModelState.IsValid)
        {
            {
                //Hash and salts password
                var (hashedPassword, salt) = HashPassword(registrationModel.password);
                
                //Checks if email is valid
                var validEmail = CheckIfEmailIsValid(registrationModel.email);


                if (validEmail == true)
                {
                    //Tests if email is registered
                    var existingUser = _context.Users.FirstOrDefault(u => u.email == registrationModel.email);

                    if (existingUser != null)
                    {
                        return BadRequest("Email already exists");
                    }
                    
                    //Creates user based on info given
                    var user = new UserModel()
                    {
                        first_name = registrationModel.first_name,
                        last_name = registrationModel.last_name,
                        email = registrationModel.email,
                        phone_number = registrationModel.phone_number,
                        birthday = registrationModel.birthday,
                        hashed_password = hashedPassword,
                        salt = salt,
                    };

                    //Update database file
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    //Creates custom user Id for the email
                    var customUserId = GenerateUniqueUserID(registrationModel.email);

                    //Creates object of the userId Model
                    var userIdObject = new customUserIdModel();

                    //If userId doesn't exist creates new object and updates model values
                    if (customUserId != null)
                    {
                        userIdObject.email = registrationModel.email;
                        userIdObject.userId = customUserId;
                    }

                    //Saves User Id to database table and uses email as primary key
                    _context.CustomUsersIdModels.Add(userIdObject);
                    await _context.SaveChangesAsync();

                    //Assigns user basic role when registering
                    var assignBasicRole = new RolesModel()
                    {
                        userId = userIdObject.userId,
                        normalUser = true,
                        Admin = false,

                    };
                    
                    //Updates role database
                    _context.RolesModels.Add(assignBasicRole);
                    await _context.SaveChangesAsync();
                    
                    return Ok("User Registered Successfully");
                }

                return BadRequest("Invalid Email");
            }
        }

        return BadRequest("Invalid registration");
    }


    [HttpGet("Users")]
    public IActionResult GetUsers()
    {
        //Checks if any valid users (USED FOR TESTING)
        var users = _context.Users.ToList();
        if (users.Count == 0)
        {
            return NotFound("No users found");
        }

        return Ok(users);
    }

    //Get a user by their ID in users table (AKA primary key)
    [HttpGet("Users/{id}")]
    public IActionResult GetUserId()
    {
        var customUserId = _context.CustomUsersIdModels.ToList();

        //Checks if user exists
        if (customUserId.Count == 0)
        {
            return NotFound("No custom users id's found");
        }

        return Ok(customUserId);
    }
    
    
    [HttpPost]
    [Route(("api/Role Assignment"))]
    public async Task<IActionResult> RoleAssignment([FromBody] RolesModel rolesModel)
    {
        var userIdTableObject = await _context.CustomUsersIdModels.FirstOrDefaultAsync(
            c => c.userId == rolesModel.userId);


        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid Role Assignment");
        }

        var user = new RolesModel()
        {
            userId = rolesModel.userId,
            Admin = rolesModel.Admin,
            normalUser = rolesModel.normalUser,
        };
        
        _context.RolesModels.Add(user);
        await _context.SaveChangesAsync();

        return Ok(user);
    }

    [HttpPost]
    [Route(("api/AdminRoleAssignment"))]
    public async Task<IActionResult> AdminRoleAssignment([FromBody] RolesModel rolesModel)
    {
        var userIdTableObject = await _context.CustomUsersIdModels.FirstOrDefaultAsync(
            c => c.userId == rolesModel.userId);
        
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid Role Assignment");
        }
        
        //Extract and validate Jwt
        var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
        var token = authorizationHeader?.Replace("Bearer ", "");
        
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized("Invalid Token");
        }
        
        //Extract GUID from JWT
        var guid = JWTHelpingMethods.GetGuidFromJwt(token);
        
        if ( !guid.HasValue)
        {
            return Unauthorized("Invalid  Token");
        }
        
        var userIdAsString = guid.Value.ToString();
        //Find the user in the database using the extracted GUID
        var user = await _context.CustomUsersIdModels.FirstOrDefaultAsync(
            c =>c.userId== userIdAsString);
        
        if(user == null){
            return Unauthorized("User Not Found");
        }
        
        //REMINDER IMPLEMENT Permissions to assign roles in phase 2 
        
        //For phase 2 for updating others roles
        /*var userToUpdate = await _context.CustomUsersIdModels.FirstOrDefaultAsync();
        
        if (userToUpdate == null){
            return NotFound("User Not Found");
        }*/
        
        //Assigning role
        rolesModel.Admin = rolesModel.Admin;
        
        //Save changes
        await _context.SaveChangesAsync();
        
        return Ok("Role assignment Successful");

    }


    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
    {
        //Checks if info is in valid format
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid login data");
        }

        //Checks email if the user exists
        var user = await _context.Users.FirstOrDefaultAsync(u => u.email == loginModel.email);

        if (user == null)
        {
            return Unauthorized("Invalid User");
        }

        //Hashes Password
        var (hashedPassword, salt) =
            HashPassWordWithExistingSalt(loginModel.Password, Convert.FromBase64String(user.salt));

        //Compares Hashes
        if (hashedPassword != user.hashed_password)
        {
            return Unauthorized("Invalid Password");
        }

        //Creates JWT 
        string token = CreateToken(loginModel);
        return Ok(token);
    }


    [HttpPost("Reset Password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel resetPasswordModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid reset password data");
        }
        
        //Strips JWT of all info except the GUID
        var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
        var token = authorizationHeader?.Replace("Bearer ", "");

        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized("Invalid Token");
        }
        
        //Gets the GUID from JWT
        var guid = JWTHelpingMethods.GetGuidFromJwt(token);

        if (!guid.HasValue)
        {
            return Unauthorized("Invalid Token");
        }

        //Checks if email corresponds to the current user email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.email == resetPasswordModel.email);

        if (user == null)
        {
            return Unauthorized("User does not exist");
        }

        //Checks if 2 passwords match
        if (resetPasswordModel.newPassword != resetPasswordModel.confirmPassword)
        {
            return Unauthorized("Passwords do not match");
        }

        //Hashes and salts password
        var (hashedPassword, salt) = HashPassword(resetPasswordModel.newPassword);

        //Update model info
        user.hashed_password = hashedPassword;
        user.salt = salt;

        //Sends to database
        _context.Users.Update(user);

        //Saves changes
        await _context.SaveChangesAsync();

        return Ok("Reset Password Successful");
    }

    

//Custom Hashing Method
    private (string hashedPassword, string Salt) HashPassword(string password)
    {
        //Generates salt
        byte[] salt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        //Hashes password and salt
        string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8
        ));
        
        //Returns password
        return (hashedPassword, Convert.ToBase64String(salt));
    }

    //Custom Method to check  if both hashed passwords are the same
    private (string hashedPassword, string salt) HashPassWordWithExistingSalt(string password, byte[] salt)
    {
        
        //Hashes given password with the given salt
        string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));
        return (hashedPassword, Convert.ToBase64String(salt));
    }

    //Custom method to see if the email is valid
    private bool CheckIfEmailIsValid(string email)
    {
        //If string is empty or first index is letter returns false
        if (string.IsNullOrEmpty(email) || isChar(email.ElementAt(0)) == false)
        {
            return false;
        }

        //Checks if @ is in the string and not the last character in the string
        var atSignIndex = findIndexOfCharInString(email, '@');
        if (atSignIndex == -1 && atSignIndex != email.Length - 1)
        {
            return false;
        }


        //Checks that a dot is present and after the at sign
        var dotSignIndex = findIndexOfCharInString(email, '.', atSignIndex + 1);
        if (dotSignIndex == -1 && dotSignIndex != email.Length - 1)
        {
            return false;
        }

        return true;
    }

    //Checks if a char value is an actual letter
    private bool isChar(char charToBeChecked)
    {
        return char.IsLetter(charToBeChecked);
    }

    //Metho that finds the index of any type of char in a srtring
    private int findIndexOfCharInString(string email, char characterBeingLookedFor, int startingIndex = 0)
    {
        for (int i = 0; i < email.Length; i++)
        {
            if (email.ElementAt(i).Equals(characterBeingLookedFor))
            {
                return i;
            }
        }

        return -1;
    }

    //Method to generated JWT token
    private string CreateToken([FromBody] LoginModel loginModel)
    {
        var userIdObject = _context.CustomUsersIdModels.FirstOrDefault(u => u.email == loginModel.email);
        var userRole = _context.RolesModels.FirstOrDefault(r => r.userId == userIdObject.userId);

        //Tests if user exists
        if (userIdObject != null)
        {
            string userId = userIdObject.userId;
            var userRoleToString = "";
            
            //Checks what the users role is
            if (userRole != null && userRole.Admin == true)
            {
                userRoleToString = "Admin";
            }
            else
            {
                userRoleToString = "Normal User";
            }

            //Claims for the JWT
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, userIdObject.userId),
                new Claim(ClaimTypes.Role, userRoleToString)
            };

            //Generates JWT 
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("JWT:Key").Value!));

            //Signing credentials for the JWT
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //Instantiates the JWT
            var token = new JwtSecurityToken(
                issuer: "Algorithm Assault",
                audience: "Work Force Management App",
                claims: claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: signingCredentials
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        return null;
    }

    //Method to generate GUID
    private Guid _id;
    private string GenerateUniqueUserID(string email)
    {
        //Checks if user exists 
        var user = _context.Users.FirstOrDefault(u => u.email == email);

        if (user == null)
        {
            return null;
        }

        //Uses GUID library to generate GUID
        string newGuid = Guid.NewGuid().ToString();

        return newGuid;
    }
}
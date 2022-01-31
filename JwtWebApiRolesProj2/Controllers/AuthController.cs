using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;



// A using JWT security token for authorization purposes example project.  Shows how to create a user registration, login a user, return a JWT.
// Of course this is just a working example, you would want to use the JWT in Authorization header of your  HTTP  requests.
// To use, you would use the authorize attribute in the methods you would want to restrict to only authorized users.  Maybe do another example for authorized roles etc...
//OK this is the example for authorized roles....


namespace JwtWebApiRolesProj2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static User user = new User();       //Intialize the user...
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)     //Constuctor for the CreateClaims method using the "secret key" injection
        {
            _configuration = configuration;
        }


        //Call the register methods below.  But be aware in an actual app would not have this logic in the controller,
        //but use the Repository pattern along with an Authentication Service call and dependency injection....

        // Use cryptography algorithm methods to create the PasswordHash and PasswordSalt,
        // and store in the user object.  When user logs in use the PasswordHash and stored Salt , compare this and verify the login,
        // then create the Jason Web Token for authorized access.

        [HttpPost("register")]      //Register the user
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt); //Call the CreatePasswordHash method to generate the values using the user
                                                                                                    // supplied Password of course ;)....

            user.Username = request.Username;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            if (user.Username != request.Username)          // First check if a valid user; if not, return a message indicating such
            {
                return BadRequest("User not found.");

            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt)) //Then verify password; if not correct send message indicating this...
            {
                return BadRequest("Wrong password.");
            }

            string token = CreateToken(user);   //If everything correct, then create the securityJwt Token using the CreateToken method and then return the jwt...
            return Ok(token);
        }

        private string CreateToken(User user)
        {
            //Claims are properties that describe the auhtorized user stored in the token, such as:name, email ect...
            List<Claim> claims = new List<Claim>       // make sure using System.Security.Claims; namespace refernce... 
            {
                new Claim(ClaimTypes.Name, user.Username),  //Would usually add a Identifier such as NameIdentifier Id and store in a database.
                new Claim(ClaimTypes.Role, "Noob"),  // A role that cannot access the weather forecast...
                new Claim(ClaimTypes.Role, "Admin") // And then the role that has authorization to access the weather forecast(Admin) add this after trying just the noob role first
                                                    // (so now have the role of a noob and Admin)...
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(              //using System.IdentityModel.Tokens; namespace reference...
                _configuration.GetSection("AppSettings:Token").Value));  //Need a top secret key we put in our appsettings.json file;use the configuration to retrieve it(Value)...

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature); //Using the key created above...

            var token = new JwtSecurityToken(       //Make sure and install the package System.IdentiyModel.Tokens.Jwt  ...
                claims: claims,
                expires: DateTime.Now.AddDays(1),   // Here we set the token to be valid for just (1) one day...
                signingCredentials: cred);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())      //Need to creat an instance of this cryptography algorithm using the System.Security.Cryptography library...
            {
                passwordSalt = hmac.Key; //Using the method computes the key, which is stored as the salt ...
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));  //Uses the key (which is the salt) to compute the hash using the password object...
            }
        }


        // Use the password the user logged in with, then using the stored users PasswordHash and PasswordSalt
        // verify this is the proper user or at least right user name and password... 
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))  //Once again creating the cryptograhy instance, but providing the users stored PasswordSalt...
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)); //Password the user used logging in...
                return computedHash.SequenceEqual(passwordHash); //compare byte by byte using the SequenceEqual method and will return either a true or false bool....
            }
        }
    }
}


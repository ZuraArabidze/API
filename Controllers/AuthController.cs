using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly IConfiguration _config;
        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _config = config;
        }

        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                string sqlCheckUserExists = $"SELECT Email FROM Auth " +
                                            $"WHERE Email = '{userForRegistration.Email}'";
                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
                if (existingUsers.Count() == 0)
                {
                    byte[] passwordSalt = new byte[128 / 8];
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt);
                    }

                    byte[] passwordHash = GetPasswordHash(userForRegistration.Password, passwordSalt);

                    string sqlAddAuth = $"INSERT INTO Auth(Email,PasswordHash,PasswordSalt) " +
                                        $"VALUES('{userForRegistration.Email}', @PasswordHash, @PasswordSalt)";

                    List<SqlParameter> sqlParameters = new List<SqlParameter>();

                    SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSalt", SqlDbType.VarBinary);
                    passwordSaltParameter.Value = passwordSalt;

                    SqlParameter passwordHashParameter = new SqlParameter("@PasswordHash", SqlDbType.VarBinary);
                    passwordHashParameter.Value = passwordHash;

                    sqlParameters.Add(passwordSaltParameter);
                    sqlParameters.Add(passwordHashParameter);

                    if (_dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters))
                    {
                        string sqlAddUser = $"INSERT INTO Users (FirstName,LastName,Email,Gender,Active)" +
                                     $"VALUES('{userForRegistration.FirstName}','{userForRegistration.LastName}'," +
                                     $"'{userForRegistration.Email}','{userForRegistration.Gender}'" +
                                     $", 1)";

                        if (_dapper.ExecuteSql(sqlAddUser))
                        {
                            return Ok();
                        }

                        throw new Exception("Failed to add user.");
                    }

                    throw new Exception("Failed to register user.");
                }
                
                throw new Exception("User with this email already exists!");
            }

            throw new Exception("Passwords do not match!");
        }

        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userFoLogin)
        {
            string sqlForHashAndSalt = $"SELECT PasswordSalt,PasswordHash FROM Auth " +
                                       $"WHERE Email = '{userFoLogin.Email}'";

            UserForLoginConfirmationDto userForConfirmation =
                _dapper.LoadDataSingle<UserForLoginConfirmationDto>(sqlForHashAndSalt);

            byte[] passwordHash = GetPasswordHash(userFoLogin.Password, userForConfirmation.PasswordSalt);

            //if(passwordHash == userForConfirmation.PasswordHash) Won't work because they are objects
            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != userForConfirmation.PasswordHash[index])
                {
                    return StatusCode(401,"Incorrect password!");
                }
            }

            string userIdSql = $"SELECT UserId FROM Users WHERE Email = '{userFoLogin.Email}'";
            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            return Ok(new Dictionary<string,string>
            {
                {"token", CreateToken(userId)}
            });
        }

        private byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value
                                            + Convert.ToBase64String(passwordSalt);

            byte[] passwordHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 1000,
                numBytesRequested: 256 / 8
            );

            return passwordHash;
        }

        // This method creates a JWT (JSON Web Token) for a user based on their userId.
        private string CreateToken(int userId)
        {
            // 1. Define the claims (payload) of the JWT.
            // Claims are the information that will be encoded into the token. 
            // In this case, we're adding the user's ID as a claim.

            Claim[] claims = new Claim[]
            {
                new Claim("userId", userId.ToString())
            };

            // 2. Create a security key for signing the token.
            // The token key is retrieved from the configuration (Appsettings.json), and it's used
            // to securely sign the token. SymmetricSecurityKey uses a shared secret for signing

            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config.GetSection("Appsettings:TokenKey").Value)
            );

            // 3. Define the signing credentials.
            // These credentials tell the JWT how to sign the token (in this case using HMAC-SHA512).

            SigningCredentials credentials = new SigningCredentials(
                tokenKey, SecurityAlgorithms.HmacSha512Signature);

            // 4. Create the security token descriptor.
            // This object contains all the token's settings, like the subject (claims),
            // signing credentials, and the expiration date.

            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddDays(1)
            };

            // 5. Create the token handler that will generate the token.

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            // 6. Create the token based on the descriptor.
            // This step actually builds the JWT.

            SecurityToken token = tokenHandler.CreateToken(descriptor);

            // 7. Return the generated JWT as a string.
            // WriteToken serializes the token to a string, which can then be returned to the client.

            return tokenHandler.WriteToken(token);
        }
    }
}

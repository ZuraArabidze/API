using System.Data;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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

            return Ok();
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
    }
}

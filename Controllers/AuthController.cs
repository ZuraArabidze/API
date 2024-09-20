using System.Data;
using System.Security.Cryptography;
using API.Data;
using API.Dtos;
using API.Helpers;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly AuthHelper _authHelper;
        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
        }

        [AllowAnonymous]
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

                    byte[] passwordHash = _authHelper.GetPasswordHash(userForRegistration.Password, passwordSalt);

                    string sqlAddAuth = $"EXEC dbo.spRegistration_Upsert @Email = @EmailParam," +
                                        $"@PasswordHash = @PasswordHashParam, @PasswordSalt = @PasswordSaltParam";

                    List<SqlParameter> sqlParameters = new List<SqlParameter>();

                    SqlParameter emailParameter = new SqlParameter("@EmailParam", SqlDbType.VarChar);
                    emailParameter.Value = userForRegistration.Email;
                    sqlParameters.Add(emailParameter);

                    SqlParameter passwordHashParameter = new SqlParameter("@PasswordHashParam", SqlDbType.VarBinary);
                    passwordHashParameter.Value = passwordHash;
                    sqlParameters.Add(passwordHashParameter);

                    SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSaltParam", SqlDbType.VarBinary);
                    passwordSaltParameter.Value = passwordSalt;
                    sqlParameters.Add(passwordSaltParameter);

                    if (_dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters))
                    {
                        string sqlAddUser = $"EXEC dbo.User_Upsert @FirstName = '{userForRegistration.FirstName}', " +
                        $"@LastName = '{userForRegistration.LastName}'," +
                        $"@Email = '{userForRegistration.Email}', " +
                        $"@Gender = '{userForRegistration.Gender}', " +
                        $"@Active = 1, " +
                        $"@JobTitle = '{userForRegistration.JobTitle}', " +
                        $"@Department = '{userForRegistration.Department}', " +
                        $"@Salary = {userForRegistration.Salary}";

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

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userFoLogin)
        {
            string sqlForHashAndSalt = $"SELECT PasswordSalt,PasswordHash FROM Auth " +
                                       $"WHERE Email = '{userFoLogin.Email}'";

            UserForLoginConfirmationDto userForConfirmation =
                _dapper.LoadDataSingle<UserForLoginConfirmationDto>(sqlForHashAndSalt);

            byte[] passwordHash = _authHelper.GetPasswordHash(userFoLogin.Password, userForConfirmation.PasswordSalt);

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
                {"token", _authHelper.CreateToken(userId)}
            });
        }

        [HttpGet("RefreshToken")]
        public IActionResult RefreshToken()
        {
            string userId = User.FindFirst("userId")?.Value + "";

            string userIdSql = $"SELECT userId FROM Users WHERE UserId = {userId}";

            int userIdFromDb = _dapper.LoadDataSingle<int>(userIdSql);

            return Ok(new Dictionary<string, string>
            {
                {"token", _authHelper.CreateToken(userIdFromDb)}
            });
        }
    }
}

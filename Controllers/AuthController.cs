using System.Data;
using System.Security.Cryptography;
using API.Data;
using API.Dtos;
using API.Helpers;
using API.Models;
using Dapper;
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
                   UserForLoginDto userForSetPassword = new UserForLoginDto()
                   {
                       Email = userForRegistration.Email,
                       Password = userForRegistration.Password,
                   };

                    if(_authHelper.setPassword(userForSetPassword)) 
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

        [HttpPut("ResetPassword")]
        public IActionResult ResetPassword(UserForLoginDto userForSetPassword)
        {
            if (_authHelper.setPassword(userForSetPassword))
            {
                return Ok();
            }
            throw new Exception("Failed to update password!");
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userFoLogin)
        {
            string sqlForHashAndSalt = $"EXEC dbo.spLoginConfirmation_Get @Email = @EmailParam ";

            DynamicParameters sqlParameters = new DynamicParameters();

            //SqlParameter emailParameter = new SqlParameter("@EmailParam",SqlDbType.VarChar);
            //emailParameter.Value = userFoLogin.Email;
            //sqlParameters.Add(emailParameter);

            sqlParameters.Add("@EmailParam",userFoLogin.Email,DbType.String);

            UserForLoginConfirmationDto userForConfirmation =
                _dapper.LoadDataSingleWithParameters<UserForLoginConfirmationDto>(sqlForHashAndSalt,sqlParameters);

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

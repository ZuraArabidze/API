using API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Dtos;
using Dapper;
using System.Data;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserCompleteController : ControllerBase
    {
        DataContextDapper _dapper;

        public UserCompleteController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        
        [HttpGet("GetUsers/{userId}/{Active}")]
        public IEnumerable<UserComplete> GetUsers(int userId, bool active)
        {
            string sql = "EXEC dbo.spUsers_Get";
            string stringParameters = "";
            DynamicParameters sqlParameters = new DynamicParameters();

            if (userId != 0)
            {
                stringParameters += ", @UserId = UserIdParameter";
                sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);
            }
            if (active)
            {
                stringParameters += ", @Active = ActiveParameter";
                sqlParameters.Add("@ActiveParameter", active, DbType.Boolean);
            }
            if (stringParameters.Length > 0)
            {
                sql += stringParameters.Substring(1);//, parameters.Length);
            }

            IEnumerable<UserComplete> users = _dapper.LoadDataWithParameters<UserComplete>(sql,sqlParameters);
            return users;
        }

       
        [HttpPut("UpsertUser")]
        public IActionResult UpsertUser(UserComplete user)
        {
            string sql = $"EXEC dbo.User_Upsert @FirstName = @FirstNameParameter, @LastName = @LastNameParameter," +
                         $"@Email = @EmailParameter, @Gender = @GenderParameter, @Active = @ActiveParameter, " +
                         $"@UserId = @UserIdParameter, @JobTitle = JobTitleParameter, " +
                         $"@Department = @DepartmentParameter, @Salary = @SalaryParameter";

            DynamicParameters sqlParameters = new DynamicParameters();

            sqlParameters.Add("@FirstNameParameter", user.FirstName, DbType.String);
            sqlParameters.Add("@LastNameParameter", user.LastName, DbType.String);
            sqlParameters.Add("@EmailParameter", user.Email, DbType.String);
            sqlParameters.Add("@GenderParameter", user.Gender, DbType.String);
            sqlParameters.Add("@ActiveParameter", user.Active, DbType.Boolean);
            sqlParameters.Add("@JobTitleParameter", user.JobTitle, DbType.String);
            sqlParameters.Add("@DepartmentParameter", user.Department, DbType.String);
            sqlParameters.Add("@SalaryParameter", user.Salary, DbType.Decimal);
            sqlParameters.Add("@UserIdParameter", user.UserId, DbType.Int32);

            if (_dapper.ExecuteSqlWithParameters(sql,sqlParameters))
            {
                return Ok();
            }
            else
            {
                throw new Exception("Failed to Update User");
            }
        }

       
        [HttpDelete("DeleteUser/{userId}")]
        public IActionResult DeleteUser(int userId)
        {
            string sql = $"EXEC dbo.spUser_Delete UserId = @UserIdParameter";

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);

            if (_dapper.ExecuteSqlWithParameters(sql,sqlParameters))
            {
                return Ok();
            }
            else
            {
                throw new Exception("Failed to Delete User");
            }
        }

    }
}

using API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Dtos;

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
            string parameters = "";

            if (userId != 0)
            {
                parameters += ", @UserId = " + userId.ToString();
            }
            if (active)
            {
                parameters += ", @Active = " + active.ToString();
            }
            if (parameters.Length > 0)
            {
                sql += parameters.Substring(1);//, parameters.Length);
            }

            IEnumerable<UserComplete> users = _dapper.LoadData<UserComplete>(sql);
            return users;
        }

       
        [HttpPut("UpserUser")]
        public IActionResult UpsertUser(UserComplete user)
        {
            string sql = $"EXEC dbo.User_Upsert @FirstName = '{user.FirstName}', @LastName = '{user.LastName}'," +
                         $"@Email = '{user.Email}', @Gender = '{user.Gender}', @Active = {user.Active}, " +
                         $"@UserId = {user.UserId}, @JobTitle = '{user.JobTitle}', " +
                         $"@Department = '{user.Department}', @Salary = {user.Salary}";

            if (_dapper.ExecuteSql(sql))
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
            string sql = $"EXEC dbo.spUser_Delete UserId = {userId}";

            if (_dapper.ExecuteSql(sql))
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

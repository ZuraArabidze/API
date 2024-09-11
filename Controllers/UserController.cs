using API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Dtos;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        DataContextDapper _dapper;

        public UserController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        /// <summary>
        /// Retrieves a list of users from the database.
        /// </summary>
        [HttpGet("GetUsers")]
        public IEnumerable<User> GetUsers()
        {
            string sql = $"SELECT UserId,FirstName,LastName,Email,Gender,Active FROM dbo.Users";
            IEnumerable<User> users = _dapper.LoadData<User>(sql);
            return users;
        }

        /// <summary>
        /// Retrieves a single user from the database based on the provided user ID.
        /// </summary>
        [HttpGet("GetSingleUser/{userId}")]
        public User GetUser(int userId)
        {
            string sql = $"SELECT UserId,FirstName,LastName,Email,Gender,Active FROM dbo.Users " +
                         $"WHERE UserId = {userId}";
            User user = _dapper.LoadDataSingle<User>(sql);
            return user;
        }

        /// <summary>
        /// Updates the details of an existing user based on the provided user information.
        /// </summary>
        [HttpPut("EditUSer")]
        public IActionResult EditUser(User user)
        {
            string sql = $"UPDATE Users SET FirstName = {user.FirstName}, LastName = {user.LastName}," +
                         $"Email = {user.Email}, Gender = {user.Gender}, Active = {user.Active} " +
                         $"WHERE UserId = {user.UserId}";
            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }
            else
            {
                throw new Exception("Failed to Update User");
            }
        }

        /// <summary>
        /// Adds a new user to the database with the provided details.
        /// </summary>
        [HttpPost("AddUser")]
        public IActionResult AddUser(UserToAddDto user)
        {
            string sql = $"INSERT INTO Users (FirstName,LastName,Email,Gender,Active)" +
                         $"VALUES('{user.FirstName}','{user.LastName}','{user.Email}','{user.Gender}'" +
                         $",'{user.Active}')";
            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }
            else
            {
                throw new Exception("Failed to Add User");
            }
        }

        /// <summary>
        /// Deletes a user from the database based on the provided user ID.
        /// </summary>
        [HttpDelete("DeleteUser/{userId}")]
        public IActionResult DeleteUser(int userId)
        {
            string sql = $"DELETE FROM Users WHERE UserId = {userId}";
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

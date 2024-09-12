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

        /// <summary>
        /// Retrieves the salary information for a specific user based on the provided user ID.
        /// </summary>
        [HttpGet("UserSalary/{userId}")]
        public IEnumerable<UserSalary> GetUserSalary(int userId)
        {
            string sql = $"SELECT UserId, Salary FROM UserSalary WHERE UserId = {userId}";
            return _dapper.LoadData<UserSalary>(sql);
        }

        /// <summary>
        /// Adds a new salary entry for a specific user.
        /// </summary>
        [HttpPost("UserSalary")]
        public IActionResult PostUserSalary(UserSalary userSalaryForInsert)
        {

            string sql = $"INSERT INTO UserSalary(UserId,Salary) VALUES({userSalaryForInsert.UserId}" +
                          $",{userSalaryForInsert.Salary})";

            if (_dapper.ExecuteSqlWithRowCount(sql) > 0)
            {
                return Ok(userSalaryForInsert);
            }
            throw new Exception("Adding User Salary failed on save");
        }

        /// <summary>
        /// Updates the salary of an existing user based on the provided user information.
        /// </summary>
        [HttpPut("UserSalary")]
        public IActionResult PutUserSalary(UserSalary userSalaryForUpdate)
        {
            string sql = $"UPDATE UserSalary SET Salary = {userSalaryForUpdate.Salary}" +
                         $" WHERE UserId = {userSalaryForUpdate.UserId}";

            if (_dapper.ExecuteSql(sql))
            {
                return Ok(userSalaryForUpdate);
            }
            throw new Exception("Updating User Salary failed on save");
        }

        /// <summary>
        /// Deletes the salary entry for a specific user based on the provided user ID.
        /// </summary>
        [HttpDelete("UserSalary/{userId}")]
        public IActionResult DeleteUserSalary(int userId)
        {
            string sql = $"DELETE FROM UserSalary WHERE UserId = {userId}";

            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }
            throw new Exception("Deleting User Salary failed on save");
        }

        /// <summary>
        /// Retrieves job information (job title, department) for a specific user based on the provided user ID.
        /// </summary>
        [HttpGet("UserJobInfo/{userId}")]
        public IEnumerable<UserJobInfo> GetUserJobInfo(int userId)
        {
            string sql = $"SELECT UserId,JobTitle,Department FROM UserJobInfo WHERE UserId = {userId}";
            return _dapper.LoadData<UserJobInfo>(sql);
            
        }

        /// <summary>
        /// Adds new job information for a specific user.
        /// </summary>
        [HttpPost("UserJobInfo")]
        public IActionResult PostUserJobInfo(UserJobInfo userJobInfoForInsert)
        {
            string sql = $"INSERT INTO UserJobInfo(UserId,Department,JobTitle) " +
                         $"VALUES({userJobInfoForInsert.UserId},'{userJobInfoForInsert.Department}','{userJobInfoForInsert.JobTitle}')";

            if (_dapper.ExecuteSql(sql))
            {
                return Ok(userJobInfoForInsert);
            }
            throw new Exception("Adding User Job Info failed on save");
        }

        /// <summary>
        /// Updates the job information (department, job title) of an existing user.
        /// </summary>
        [HttpPut("UserJobInfo")]
        public IActionResult PutUserJobInfo(UserJobInfo userJobInfoForUpdate)
        {
            string sql = $"UPDATE UserJobInfo SET Department = '{userJobInfoForUpdate.Department}'," +
                         $"JobTitle ='{userJobInfoForUpdate.JobTitle}' WHERE UserId = {userJobInfoForUpdate.UserId}";

            if (_dapper.ExecuteSql(sql))
            {
                return Ok(userJobInfoForUpdate);
            }
            throw new Exception("Updating User Job Info failed on save");
        }

        /// <summary>
        /// Deletes the job information for a specific user based on the provided user ID.
        /// </summary>
        [HttpDelete("UserJobInfo/{userId}")]
        public IActionResult DeleteUserJobInfo(int userId)
        {
            string sql = $"DELETE FROM UserJobInfo WHERE UserId = {userId}";

            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to Delete User");
        }

    }
}

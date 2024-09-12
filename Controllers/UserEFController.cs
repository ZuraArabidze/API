using API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Dtos;
using AutoMapper;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserEFController : ControllerBase
    {
        DataContextEF _entityFramework;
        IMapper _mapper;

        public UserEFController(IConfiguration config)
        {
            _entityFramework = new DataContextEF(config);
            _mapper = new Mapper(new MapperConfiguration(cfg =>
            {
                // source,destination
                cfg.CreateMap<UserToAddDto, User>();
            }));
        }

        /// <summary>
        /// Retrieves a list of users from the database.
        /// </summary>
        [HttpGet("GetUsers")]
        public IEnumerable<User> GetUsers()
        {
            IEnumerable<User> users = _entityFramework.Users.ToList<User>();
            return users;
        }

        /// <summary>
        /// Retrieves a single user from the database based on the provided user ID.
        /// </summary>
        [HttpGet("GetSingleUser/{userId}")]
        public User GetUser(int userId)
        {
            User? user = _entityFramework.Users.Where(u => u.UserId == userId).FirstOrDefault<User>();
            if (user != null)
            {
                return user;
            }
            else
            {
                throw new Exception("Failed to Get User");
            }
            
        }

        /// <summary>
        /// Updates the details of an existing user based on the provided user information.
        /// </summary>
        [HttpPut("EditUSer")]
        public IActionResult EditUser(User user)
        {
            User? userDb = _entityFramework.Users.Where(u => u.UserId == user.UserId)
                .FirstOrDefault<User>();

            if (userDb != null)
            {
                userDb.FirstName = user.FirstName;
                userDb.LastName = user.LastName;
                userDb.Email = user.Email;
                userDb.Gender = user.Gender;
                userDb.Active = user.Active;

                if (_entityFramework.SaveChanges() > 0)
                {
                    return Ok();
                }
                else
                {
                    throw new Exception("Failed to Update User");
                }
            }
            else
            {
                throw new Exception("Failed to Get User");
            }
            
        }

        /// <summary>
        /// Adds a new user to the database with the provided details.
        /// </summary>
        [HttpPost("AddUser")]
        public IActionResult AddUser(UserToAddDto user)
        {
            User userDb = _mapper.Map<User>(user);

            _entityFramework.Add(userDb);
            if (_entityFramework.SaveChanges() > 0)
            {
                return Ok();
            }

            throw new Exception("Failed to Add User");
        }

        /// <summary>
        /// Deletes a user from the database based on the provided user ID.
        /// </summary>
        [HttpDelete("DeleteUser/{userId}")]
        public IActionResult DeleteUser(int userId)
        {
            User? userDb = _entityFramework.Users
                .Where(u => u.UserId == userId)
                .FirstOrDefault<User>();

            if (userDb != null)
            {
                _entityFramework.Users.Remove(userDb);
                if (_entityFramework.SaveChanges() > 0)
                {
                    return Ok();
                }

                throw new Exception("Failed to Delete User");
            }

            throw new Exception("Failed to Get User");
        }

        /// <summary>
        /// Retrieves the salary information for a specific user using Entity Framework.
        /// </summary>
        [HttpGet("UserSalary/{userId}")]
        public IEnumerable<UserSalary> GetUserSalaryEF(int userId)
        {
            return _entityFramework.UserSalary
                .Where(u => u.UserId == userId)
                .ToList();
        }

        /// <summary>
        /// Adds new salary information for a specific user using Entity Framework.
        /// </summary>
        [HttpPost("UserSalary")]
        public IActionResult PostUserSalaryEf(UserSalary userForInsert)
        {
            _entityFramework.UserSalary.Add(userForInsert);
            if (_entityFramework.SaveChanges() > 0)
            {
                return Ok();
            }
            throw new Exception("Adding UserSalary failed on save");
        }

        /// <summary>
        /// Updates the salary information of an existing user using Entity Framework.
        /// </summary>
        [HttpPut("UserSalary")]
        public IActionResult PutUserSalaryEf(UserSalary userForUpdate)
        {
            UserSalary? userToUpdate = _entityFramework.UserSalary
                .Where(u => u.UserId == userForUpdate.UserId)
                .FirstOrDefault();

            if (userToUpdate != null)
            {
                _mapper.Map(userForUpdate, userToUpdate);
                if (_entityFramework.SaveChanges() > 0)
                {
                    return Ok();
                }
                throw new Exception("Updating UserSalary failed on save");
            }
            throw new Exception("Failed to find UserSalary to Update");
        }

        /// <summary>
        /// Deletes salary information for a specific user using Entity Framework.
        /// </summary>
        [HttpDelete("UserSalary/{userId}")]
        public IActionResult DeleteUserSalaryEf(int userId)
        {
            UserSalary? userToDelete = _entityFramework.UserSalary
                .Where(u => u.UserId == userId)
                .FirstOrDefault();

            if (userToDelete != null)
            {
                _entityFramework.UserSalary.Remove(userToDelete);
                if (_entityFramework.SaveChanges() > 0)
                {
                    return Ok();
                }
                throw new Exception("Deleting UserSalary failed on save");
            }
            throw new Exception("Failed to find UserSalary to delete");
        }

        /// <summary>
        /// Retrieves job information for a specific user using Entity Framework.
        /// </summary>
        [HttpGet("UserJobInfo/{userId}")]
        public IEnumerable<UserJobInfo> GetUserJobInfoEF(int userId)
        {
            return _entityFramework.UserJobInfo
                .Where(u => u.UserId == userId)
                .ToList();
        }

        /// <summary>
        /// Adds new job information for a specific user using Entity Framework.
        /// </summary>
        [HttpPost("UserJobInfo")]
        public IActionResult PostUserJobInfoEf(UserJobInfo userForInsert)
        {
            _entityFramework.UserJobInfo.Add(userForInsert);
            if (_entityFramework.SaveChanges() > 0)
            {
                return Ok();
            }
            throw new Exception("Adding UserJobInfo failed on save");
        }

        /// <summary>
        /// Updates the job information of an existing user using Entity Framework.
        /// </summary>
        [HttpPut("UserJobInfo")]
        public IActionResult PutUserJobInfoEf(UserJobInfo userForUpdate)
        {
            UserJobInfo? userToUpdate = _entityFramework.UserJobInfo
                .Where(u => u.UserId == userForUpdate.UserId)
                .FirstOrDefault();

            if (userToUpdate != null)
            {
                _mapper.Map(userForUpdate, userToUpdate);
                if (_entityFramework.SaveChanges() > 0)
                {
                    return Ok();
                }
                throw new Exception("Updating UserJobInfo failed on save");
            }
            throw new Exception("Failed to find UserJobInfo to Update");
        }

        /// <summary>
        /// Deletes job information for a specific user using Entity Framework.
        /// </summary>
        [HttpDelete("UserJobInfo/{userId}")]
        public IActionResult DeleteUserJobInfoEf(int userId)
        {
            UserJobInfo? userToDelete = _entityFramework.UserJobInfo
                .Where(u => u.UserId == userId)
                .FirstOrDefault();

            if (userToDelete != null)
            {
                _entityFramework.UserJobInfo.Remove(userToDelete);
                if (_entityFramework.SaveChanges() > 0)
                {
                    return Ok();
                }
                throw new Exception("Deleting UserJobInfo failed on save");
            }
            throw new Exception("Failed to find UserJobInfo to delete");
        }
    }
}

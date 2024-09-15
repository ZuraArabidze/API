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
        IUserRepository _userRepository;
        IMapper _mapper;

    public UserEFController(IConfiguration config, IUserRepository userRepository)
    {
        _userRepository = userRepository;

        _mapper = new Mapper(new MapperConfiguration(cfg =>{
            cfg.CreateMap<UserToAddDto, User>();
            cfg.CreateMap<UserSalary, UserSalary>().ReverseMap();
            cfg.CreateMap<UserJobInfo, UserJobInfo>().ReverseMap();
        }));
    }

        /// <summary>
        /// Retrieves a list of users from the database.
        /// </summary>
        [HttpGet("GetUsers")]
        public IEnumerable<User> GetUsers()
        {
            IEnumerable<User> users = _userRepository.GetUsers();
            return users;
        }

        /// <summary>
        /// Retrieves a single user from the database based on the provided user ID.
        /// </summary>
        [HttpGet("GetSingleUser/{userId}")]
        public User GetUser(int userId)
        {
            return _userRepository.GetSingleUser(userId);
        }

        /// <summary>
        /// Updates the details of an existing user based on the provided user information.
        /// </summary>
        [HttpPut("EditUSer")]
        public IActionResult EditUser(User user)
        {
            User? userDb = _userRepository.GetSingleUser(user.UserId);
            if (userDb != null)

            {
                userDb.FirstName = user.FirstName;
                userDb.LastName = user.LastName;
                userDb.Email = user.Email;
                userDb.Gender = user.Gender;
                userDb.Active = user.Active;

                if (_userRepository.SaveChanges())
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

            _userRepository.AddEntity<User>(userDb);
            if (_userRepository.SaveChanges())
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
            User? userDb = _userRepository.GetSingleUser(userId);

            if (userDb != null)
            {
                _userRepository.RemoveEntity<User>(userDb);
                if (_userRepository.SaveChanges())
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
        public UserSalary GetUserSalaryEF(int userId)
        {
            return _userRepository.GetSingleUserSalary(userId);
        }

        /// <summary>
        /// Adds new salary information for a specific user using Entity Framework.
        /// </summary>
        [HttpPost("UserSalary")]
        public IActionResult PostUserSalaryEf(UserSalary userForInsert)
        {
            _userRepository.AddEntity<UserSalary>(userForInsert);
            if (_userRepository.SaveChanges())
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
            UserSalary? userToUpdate = _userRepository.GetSingleUserSalary(userForUpdate.UserId);

            if (userToUpdate != null)
            {
                _mapper.Map(userForUpdate, userToUpdate);
                if (_userRepository.SaveChanges())
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
            UserSalary? userToDelete = _userRepository.GetSingleUserSalary(userId);

            if (userToDelete != null)
            {
                _userRepository.RemoveEntity<UserSalary>(userToDelete);
                if (_userRepository.SaveChanges())
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
        public UserJobInfo GetUserJobInfoEF(int userId)
        {
            return _userRepository.GetSingleUserJobInfo(userId);
        }

        /// <summary>
        /// Adds new job information for a specific user using Entity Framework.
        /// </summary>
        [HttpPost("UserJobInfo")]
        public IActionResult PostUserJobInfoEf(UserJobInfo userForInsert)
        {
            _userRepository.AddEntity<UserJobInfo>(userForInsert);
            if (_userRepository.SaveChanges())
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
            UserJobInfo? userToUpdate = _userRepository.GetSingleUserJobInfo(userForUpdate.UserId);

            if (userToUpdate != null)
            {
                _mapper.Map(userForUpdate, userToUpdate);
                if (_userRepository.SaveChanges())
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
            UserJobInfo? userToDelete = _userRepository.GetSingleUserJobInfo(userId);

            if (userToDelete != null)
            {
                _userRepository.RemoveEntity<UserJobInfo>(userToDelete);
                if (_userRepository.SaveChanges())
                {
                    return Ok();
                }
                throw new Exception("Deleting UserJobInfo failed on save");
            }
            throw new Exception("Failed to find UserJobInfo to delete");
        }
    }
}

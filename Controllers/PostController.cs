using API.Data;
using API.Dtos;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly DataContextDapper _dapper;

        public PostController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        [HttpGet("Posts")]
        public IEnumerable<Post> GetPosts()
        {
            string sql = $"SELECT UserId,PostTitle,PostContent,PostCreated,PostUpdated FROM Posts";

            return _dapper.LoadData<Post>(sql);
        }

        [HttpGet("PostSingle/{postId}")]
        public IEnumerable<Post> GetPostSingle(int postId)
        {
            string sql = $"SELECT UserId,PostTitle,PostContent,PostCreated,PostUpdated FROM Posts" +
                         $"WHERE PostId = {postId}";

            return _dapper.LoadData<Post>(sql);
        }

        [HttpGet("PostsByUser/{userId}")]
        public IEnumerable<Post> GetPostsByUser(int userId)
        {
            string sql = $"SELECT UserId,PostTitle,PostContent,PostCreated,PostUpdated FROM Posts" +
                         $"WHERE UserId = {userId}";

            return _dapper.LoadData<Post>(sql);
        }

        [HttpGet("MyPosts")]
        public Post GetMyPosts()
        {
            string sql = $"SELECT UserId,PostTitle,PostContent,PostCreated,PostUpdated FROM Posts" +
                         $"WHERE UserId = {this.User.FindFirst("userId")?.Value}";

            return _dapper.LoadDataSingle<Post>(sql);
        }

        [HttpPost("Post")]
        public IActionResult AddPost(PostToAddDto postToAdd)
        {
            string sql = $"INSERT INTO Posts(UserId,PostTitle,PostContent,PostCreated,PostUpdated)" +
                         $"VALUES({this.User.FindFirst("userId")?.Value},'{postToAdd.PostTitle}'," +
                         $"'{postToAdd.PostContent}',GETDATE(),GETDATE())";

            if(_dapper.ExecuteSql((sql)))
            {
                return Ok();
            }

            throw new Exception("Failed to create new post!");
        }

        [HttpPost("EditPost")]
        public IActionResult AddPost(PostToEditDto postToEdit)
        {
            string sql = $"UPDATE Posts SET PostContent = '{postToEdit.PostContent}', " +
                         $"PostTitle = '{postToEdit.PostTitle}',PostUpdated = GETDATE()" +
                         $"WHERE PostId = {postToEdit.PostId} AND " +
                         $"UserId = {this.User.FindFirst("userId")?.Value}";

            if (_dapper.ExecuteSql((sql)))
            {
                return Ok();
            }

            throw new Exception("Failed to edit post!");
        }

        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = $"DELETE FROM Posts WHERE PostId = {postId}" +
                         $"AND UserId = {this.User.FindFirst("userId")?.Value}";

            if (_dapper.ExecuteSql((sql)))
            {
                return Ok();
            }

            throw new Exception("Failed to delete post!");
        }


        [HttpGet("PostBySearch/{searchParam}")]
        public IEnumerable<Post> PostBySearch(string searchParam)
        {
            string sql = $"SELECT PostId,UserId,PostTitle,PostContent,PostCreated,PostUpdated FROM Posts" +
                         $"WHERE PostTitle LIKE '%{searchParam}%' OR PostContent LIKE '%{searchParam}%'";

            return _dapper.LoadData<Post>(sql);
        }
    }
}

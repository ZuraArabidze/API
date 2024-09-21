using API.Data;
using API.Dtos;
using API.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;

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

        [HttpGet("Posts/{postId}/{userId}/{searchParam}")]
        public IEnumerable<Post> GetPosts(int postId = 0, int userId = 0, string searchParam = "None")
        {
            string sql = $"EXEC dbo.spPosts_Get";
            string stringParameters = "";

            DynamicParameters sqlParameters = new DynamicParameters();
            if (postId != 0)
            {
                stringParameters += ", @PostId=@PostIdParameter";
                sqlParameters.Add("@PostIdParameter", postId, DbType.Int32);
            }
            if (userId != 0)
            {
                stringParameters += ", @UserId=@UserIdParameter";
                sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);
            }

            if (searchParam.ToLower() != "none")
            {
                stringParameters += ", @SearchValue=@SearchValueParameter";
                sqlParameters.Add("@SearchValueParameter", searchParam, DbType.String);
            }
            if (stringParameters.Length > 0)
            {
                sql += stringParameters.Substring(1);//, parameters.Length);
            }

            return _dapper.LoadDataWithParameters<Post>(sql, sqlParameters);
        }

        /*[HttpGet("PostSingle/{postId}")]
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
        }*/

        /*[HttpGet("MyPosts")]
        public Post GetMyPosts()
        {
            string sql = $"SELECT UserId,PostTitle,PostContent,PostCreated,PostUpdated FROM Posts" +
                         $"WHERE UserId = {this.User.FindFirst("userId")?.Value}";

            return _dapper.LoadDataSingle<Post>(sql);
        }*/

        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string sql = $"EXEC dbo.spPosts_Get @UserId=@UserIdParameter";
            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@UserIdParameter", this.User.FindFirst("userId")?.Value, DbType.Int32);

            return _dapper.LoadDataWithParameters<Post>(sql, sqlParameters);
        }

        /*[HttpPost("Post")]
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

        [HttpPut("EditPost")]
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
        }*/

        [HttpPut("UpsertPost")]
        public IActionResult UpsertPost(Post postToUpsert)
        {
            string sql = $"EXEC dbo.spPosts_Upsert @UserId = @UserIdParameter, " +
                         $"@PostTitle = PostTitleParameter, " +
                         $"@PostContent = @PostContentParameter";

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@UserIdParameter", this.User.FindFirst("userId")?.Value, DbType.Int32);
            sqlParameters.Add("@PostTitleParameter", postToUpsert.PostTitle, DbType.String);
            sqlParameters.Add("@PostContentParameter", postToUpsert.PostContent, DbType.String);

            if (postToUpsert.PostId > 0)
            {
                sql += ", @PostId=@PostIdParameter";
                sqlParameters.Add("@PostIdParameter", postToUpsert.PostId, DbType.Int32);
            }

            if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
            {
                return Ok();
            }

            throw new Exception("Failed to upsert post!");
        }


        /*[HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = $"DELETE FROM Posts WHERE PostId = {postId}" +
                         $"AND UserId = {this.User.FindFirst("userId")?.Value}";

            if (_dapper.ExecuteSql((sql)))
            {
                return Ok();
            }

            throw new Exception("Failed to delete post!");
        }*/

        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = @"EXEC dbo.spPost_Delete @UserId=@UserIdParameter, " +
                         $"@PostId=@PostIdParameter";

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@UserIdParameter", this.User.FindFirst("userId")?.Value, DbType.Int32);
            sqlParameters.Add("@PostIdParameter", postId, DbType.Int32);

            if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
            {
                return Ok();
            }

            throw new Exception("Failed to delete post!");
        }


        /*[HttpGet("PostBySearch/{searchParam}")]
        public IEnumerable<Post> PostBySearch(string searchParam)
        {
            string sql = $"SELECT PostId,UserId,PostTitle,PostContent,PostCreated,PostUpdated FROM Posts" +
                         $"WHERE PostTitle LIKE '%{searchParam}%' OR PostContent LIKE '%{searchParam}%'";

            return _dapper.LoadData<Post>(sql);
        }*/
    }
}

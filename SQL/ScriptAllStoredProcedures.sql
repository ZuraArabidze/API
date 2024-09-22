USE API;
GO

CREATE OR ALTER PROCEDURE dbo.spUsers_Get
/*EXEC dbo.spUsers_Get @UserId=3*/
    @UserId INT = NULL
    , @Active BIT = NULL
AS
BEGIN
    DROP TABLE IF EXISTS #AverageDeptSalary
    -- IF OBJECT_ID('tempdb..#AverageDeptSalary') IS NOT NULL
    --     DROP TABLE #AverageDeptSalary;

    SELECT UserJobInfo.Department
        , AVG(UserSalary.Salary) AvgSalary
        INTO #AverageDeptSalary
    FROM dbo.Users AS Users 
        LEFT JOIN dbo.UserSalary AS UserSalary
            ON UserSalary.UserId = Users.UserId
        LEFT JOIN dbo.UserJobInfo AS UserJobInfo
            ON UserJobInfo.UserId = Users.UserId
        GROUP BY UserJobInfo.Department

    CREATE CLUSTERED INDEX cix_AverageDeptSalary_Department ON #AverageDeptSalary(Department)

    SELECT [Users].[UserId],
        [Users].[FirstName],
        [Users].[LastName],
        [Users].[Email],
        [Users].[Gender],
        [Users].[Active],
        UserSalary.Salary,
        UserJobInfo.Department,
        UserJobInfo.JobTitle,
        AvgSalary.AvgSalary
    FROM dbo.Users AS Users 
        LEFT JOIN dbo.UserSalary AS UserSalary
            ON UserSalary.UserId = Users.UserId
        LEFT JOIN dbo.UserJobInfo AS UserJobInfo
            ON UserJobInfo.UserId = Users.UserId
        LEFT JOIN #AverageDeptSalary AS AvgSalary
            ON AvgSalary.Department = UserJobInfo.Department
        WHERE Users.UserId = ISNULL(@UserId, Users.UserId)
            AND ISNULL(Users.Active, 0) = COALESCE(@Active, Users.Active, 0)
END
GO

CREATE OR ALTER PROCEDURE dbo.spUser_Upsert
	@FirstName NVARCHAR(50),
	@LastName NVARCHAR(50),
	@Email NVARCHAR(50),
	@Gender NVARCHAR(50),
	@JobTitle NVARCHAR(50),
	@Department NVARCHAR(50),
    @Salary DECIMAL(18, 4),
	@Active BIT = 1,
	@UserId INT = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT * FROM dbo.Users WHERE UserId = @UserId)
        BEGIN
        IF NOT EXISTS (SELECT * FROM dbo.Users WHERE Email = @Email)
            BEGIN
                DECLARE @OutputUserId INT

                INSERT INTO dbo.Users(
                    [FirstName],
                    [LastName],
                    [Email],
                    [Gender],
                    [Active]
                ) VALUES (
                    @FirstName,
                    @LastName,
                    @Email,
                    @Gender,
                    @Active
                )

                SET @OutputUserId = @@IDENTITY

                INSERT INTO dbo.UserSalary(
                    UserId,
                    Salary
                ) VALUES (
                    @OutputUserId,
                    @Salary
                )

                INSERT INTO dbo.UserJobInfo(
                    UserId,
                    Department,
                    JobTitle
                ) VALUES (
                    @OutputUserId,
                    @Department,
                    @JobTitle
                )
            END
        END
    ELSE 
        BEGIN
            UPDATE dbo.Users 
                SET FirstName = @FirstName,
                    LastName = @LastName,
                    Email = @Email,
                    Gender = @Gender,
                    Active = @Active
                WHERE UserId = @UserId

            UPDATE dbo.UserSalary
                SET Salary = @Salary
                WHERE UserId = @UserId

            UPDATE dbo.UserJobInfo
                SET Department = @Department,
                    JobTitle = @JobTitle
                WHERE UserId = @UserId
        END
END
GO

CREATE OR ALTER PROCEDURE dbo.spUser_Delete
    @UserId INT
AS
BEGIN
    DECLARE @Email NVARCHAR(50);

    SELECT  @Email = Users.Email
      FROM  dbo.Users
     WHERE  Users.UserId = @UserId;

    DELETE  FROM dbo.UserSalary
     WHERE  UserSalary.UserId = @UserId;

    DELETE  FROM dbo.UserJobInfo
     WHERE  UserJobInfo.UserId = @UserId;

    DELETE  FROM dbo.Users
     WHERE  Users.UserId = @UserId;

    DELETE  FROM dbo.Auth
     WHERE  Auth.Email = @Email;
END;
GO



CREATE OR ALTER PROCEDURE dbo.spPosts_Get
/*EXEC dbo.spPosts_Get @UserId = 1003, @SearchValue='Second'*/
/*EXEC dbo.spPosts_Get @PostId = 2*/
    @UserId INT = NULL
    , @SearchValue NVARCHAR(MAX) = NULL
    , @PostId INT = NULL
AS
BEGIN
    SELECT [Posts].[PostId],
        [Posts].[UserId],
        [Posts].[PostTitle],
        [Posts].[PostContent],
        [Posts].[PostCreated],
        [Posts].[PostUpdated] 
    FROM dbo.Posts AS Posts
        WHERE Posts.UserId = ISNULL(@UserId, Posts.UserId)
            AND Posts.PostId = ISNULL(@PostId, Posts.PostId)
            AND (@SearchValue IS NULL
                OR Posts.PostContent LIKE '%' + @SearchValue + '%'
                OR Posts.PostTitle LIKE '%' + @SearchValue + '%')
END
GO

CREATE OR ALTER PROCEDURE dbo.spPosts_Upsert
    @UserId INT
    , @PostTitle NVARCHAR(255)
    , @PostContent NVARCHAR(MAX)
    , @PostId INT = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT * FROM dbo.Posts WHERE PostId = @PostId)
        BEGIN
            INSERT INTO dbo.Posts(
                [UserId],
                [PostTitle],
                [PostContent],
                [PostCreated],
                [PostUpdated]
            ) VALUES (
                @UserId,
                @PostTitle,
                @PostContent,
                GETDATE(),
                GETDATE()
            )
        END
    ELSE
        BEGIN
            UPDATE dbo.Posts 
                SET PostTitle = @PostTitle,
                    PostContent = @PostContent,
                    PostUpdated = GETDATE()
                WHERE PostId = @PostId
        END
END
GO

CREATE OR ALTER PROCEDURE dbo.spPost_Delete
    @PostId INT
    , @UserId INT 
AS
BEGIN
    DELETE FROM dbo.Posts 
        WHERE PostId = @PostId
            AND UserId = @UserId
END
GO



CREATE OR ALTER PROCEDURE dbo.spLoginConfirmation_Get
    @Email NVARCHAR(50)
AS
BEGIN
    SELECT [Auth].[PasswordHash],
        [Auth].[PasswordSalt] 
    FROM dbo.Auth AS Auth 
        WHERE Auth.Email = @Email
END;
GO

CREATE OR ALTER PROCEDURE dbo.spRegistration_Upsert
    @Email NVARCHAR(50),
    @PasswordHash VARBINARY(MAX),
    @PasswordSalt VARBINARY(MAX)
AS 
BEGIN
    IF NOT EXISTS (SELECT * FROM dbo.Auth WHERE Email = @Email)
        BEGIN
            INSERT INTO dbo.Auth(
                [Email],
                [PasswordHash],
                [PasswordSalt]
            ) VALUES (
                @Email,
                @PasswordHash,
                @PasswordSalt
            )
        END
    ELSE
        BEGIN
            UPDATE dbo.Auth 
                SET PasswordHash = @PasswordHash,
                    PasswordSalt = @PasswordSalt
                WHERE Email = @Email
        END
END
GO
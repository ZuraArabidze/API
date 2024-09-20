using API.Data;
using API.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API.Helpers
{
    public class AuthHelper
    {
        private readonly IConfiguration _config;
        private readonly DataContextDapper _dapper;

        public AuthHelper(IConfiguration config)
        {
            _config = config;
            _dapper = new DataContextDapper(config);
        }
        public byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value
                                            + Convert.ToBase64String(passwordSalt);

            byte[] passwordHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 1000,
                numBytesRequested: 256 / 8
            );

            return passwordHash;
        }

        // This method creates a JWT (JSON Web Token) for a user based on their userId.
        public string CreateToken(int userId)
        {
            // 1. Define the claims (payload) of the JWT.
            // Claims are the information that will be encoded into the token. 
            // In this case, we're adding the user's ID as a claim.

            Claim[] claims = new Claim[]
            {
                new Claim("userId", userId.ToString())
            };

            // 2. Create a security key for signing the token.
            // The token key is retrieved from the configuration (Appsettings.json), and it's used
            // to securely sign the token. SymmetricSecurityKey uses a shared secret for signing

            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config.GetSection("Appsettings:TokenKey").Value)
            );

            // 3. Define the signing credentials.
            // These credentials tell the JWT how to sign the token (in this case using HMAC-SHA512).

            SigningCredentials credentials = new SigningCredentials(
                tokenKey, SecurityAlgorithms.HmacSha512Signature);

            // 4. Create the security token descriptor.
            // This object contains all the token's settings, like the subject (claims),
            // signing credentials, and the expiration date.

            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddDays(1)
            };

            // 5. Create the token handler that will generate the token.

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            // 6. Create the token based on the descriptor.
            // This step actually builds the JWT.

            SecurityToken token = tokenHandler.CreateToken(descriptor);

            // 7. Return the generated JWT as a string.
            // WriteToken serializes the token to a string, which can then be returned to the client.

            return tokenHandler.WriteToken(token);
        }

        public bool setPassword(UserForLoginDto userForSetPassword)
        {
            byte[] passwordSalt = new byte[128 / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);
            }

            byte[] passwordHash = GetPasswordHash(userForSetPassword.Password, passwordSalt);

            string sqlAddAuth = $"EXEC dbo.spRegistration_Upsert @Email = @EmailParam," +
                                $"@PasswordHash = @PasswordHashParam, @PasswordSalt = @PasswordSaltParam";

            List<SqlParameter> sqlParameters = new List<SqlParameter>();

            SqlParameter emailParameter = new SqlParameter("@EmailParam", SqlDbType.VarChar);
            emailParameter.Value = userForSetPassword.Email;
            sqlParameters.Add(emailParameter);

            SqlParameter passwordHashParameter = new SqlParameter("@PasswordHashParam", SqlDbType.VarBinary);
            passwordHashParameter.Value = passwordHash;
            sqlParameters.Add(passwordHashParameter);

            SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSaltParam", SqlDbType.VarBinary);
            passwordSaltParameter.Value = passwordSalt;
            sqlParameters.Add(passwordSaltParameter);

            return _dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters);
        }
    }
}

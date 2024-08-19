using System.Data;
using System.Security.Cryptography;
using System.Text;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DotnetAPI.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _config = config;
        }

        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                string sqlCheckUserExists = "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '" +
                    userForRegistration.Email + "'";

                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
                if (existingUsers.Count() == 0)
                {
                    byte[] password2 = new byte[128 / 8];
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(password2);
                    }

                    byte[] password1 = Getpassword1(userForRegistration.Password, password2);

                    string sqlAddAuth = @"
                        INSERT INTO TutorialAppSchema.Auth  ([Email],
                        [password1],
                        [password2]) VALUES ('" + userForRegistration.Email +
                        "', @password1, @password2)";

                    List<SqlParameter> sqlParameters = new List<SqlParameter>();

                    SqlParameter password2Parameter = new SqlParameter("@password2", SqlDbType.VarBinary);
                    password2Parameter.Value = password2;

                    SqlParameter password1Parameter = new SqlParameter("@password1", SqlDbType.VarBinary);
                    password1Parameter.Value = password1;

                    sqlParameters.Add(password2Parameter);
                    sqlParameters.Add(password1Parameter);

                    if (_dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters))
                    {
                        
                        string sqlAddUser = @"
                            INSERT INTO TutorialAppSchema.Users(
                                [FirstName],
                                [LastName],
                                [Email],
                                [Gender],
                                [Active]
                            ) VALUES (" +
                                "'" + userForRegistration.FirstName + 
                                "', '" + userForRegistration.LastName +
                                "', '" + userForRegistration.Email + 
                                "', '" + userForRegistration.Gender + 
                                "', 1)";
                        if (_dapper.ExecuteSql(sqlAddUser))
                        {
                            return Ok();
                        }
                        throw new Exception("Failed to add user.");
                    }
                    throw new Exception("Failed to register user.");
                }
                throw new Exception("User with this email already exists!");
            }
            throw new Exception("Passwords do not match!");
        }

        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            string sqlFor1And2 = @"SELECT 
                [password1],
                [password2] FROM TutorialAppSchema.Auth WHERE Email = '" +
                userForLogin.Email + "'";

            UserForLoginConfirmationDto userForConfirmation = _dapper
                .LoadDataSingle<UserForLoginConfirmationDto>(sqlFor1And2);

            byte[] password1 = Getpassword1(userForLogin.Password, userForConfirmation.password2);

            // if (password1 == userForConfirmation.password1) // Won't work

            for (int index = 0; index < password1.Length; index++)
            {
                if (password1[index] != userForConfirmation.password1[index]){
                    return StatusCode(401, "Incorrect password!");
                }
            }

            return Ok();
        }

        private byte[] Getpassword1(string password, byte[] password2)
        {
            string password2PlusString = _config.GetSection("AppSettings:PasswordKey").Value +
                Convert.ToBase64String(password2);

            return KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(password2PlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 1000000,
                numBytesRequested: 256 / 8
            );
        }

    }
}
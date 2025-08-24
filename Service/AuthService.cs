using eventManager.Dtos;
using eventManager.Model;
using eventManager.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MySqlX.XDevAPI.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace eventManager.Helper
{
    public class AuthService
    {
        private readonly IConfiguration _configuration;
        private readonly SessionConfig _config;
        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task AddUpdateUser(users user)
        {
            DataBaseUtil _db = new DataBaseUtil(_configuration);
            try
            {
                var result = new Response();
                Dictionary<string, string> values = new Dictionary<string, string>();
                Dictionary<string, string> where = new Dictionary<string, string>();
                string passwordKey = _configuration["password:Key"];
                if (user.id == 0)
                {
                    values.Add("name", user.name);
                    values.Add("role_id", user.role_id.ToString());
                    values.Add("email", user.email);
                    string hashedPassword = HashPassword(user.password_hash, passwordKey);
                    values.Add("password_hash", hashedPassword);
                    values.Add("phone", user.phone);
                    values.Add("status", user.Status.ToString());
                    var res = _db.SaveExecuteNonQuery<users>(values);
                    if (res == 1)
                    {
                        result.status = 1;
                        result.message = "Saved Succefully.";
                    }
                    else
                    {
                        result.status = 0;
                        result.message = "Error in inset user.";
                    }
                }
                else
                {
                    values.Add("name", user.name);
                    values.Add("role_id", user.role_id.ToString());
                    values.Add("email", user.email);
                    values.Add("password_hash", user.password_hash);
                    values.Add("phone", user.phone);
                    values.Add("status", user.Status.ToString());
                    where.Add("id", user.id.ToString());
                    var res = await _db.UpdateRecord<users>(values, where);
                    if (res == 1)
                    {
                        result.status = 1;
                        result.message = "Updated Succefully.";
                    }
                    else
                    {
                        result.status = 0;
                        result.message = "Error in update user.";
                    }
                }
            }
            catch (Exception ex) { }
        }
        public async Task<Response> Login(string email, string password)
        {
            DataBaseUtil _db = new DataBaseUtil(_configuration);
            var result = new Response();
            try
            {
                string passwordKey = _configuration["password:Key"];
                // Safely escape user input to avoid SQL injection
                string query = $"SELECT u.*,r.name as role_name FROM users u" +
                                " left join roles r on r.id = u.role_id WHERE email = '" + email + "'";

                var user = await _db.GetSingleRecordFromQuery<users>(query);

                if (user == null || string.IsNullOrEmpty(user.password_hash))
                {
                    result.status = 0;
                    result.message = "Invalid Password.";
                    result.data = null;
                    return result;
                }
                // user not found or hash missing
                // Verify hashed password
                bool isValid = VerifyPassword(password, user.password_hash, passwordKey);

                if (!isValid)
                {
                    // invalid password
                    result.status = 0;
                    result.message = "Invalid password.";
                    result.data = null;
                    return result;
                }


                // Generate JWT token
                var token = GenerateJwtToken(user);
                var userres = new users
                {
                    id = user.id,
                    email = email,
                    name = user.name,
                    phone = user.phone,
                    role_id = user.role_id,
                    Status = user.Status,
                    role_name = user.role_name,
                    token = token
                };
                result.status = 1;
                result.data = userres;
                result.redirect_url = "";
                result.message = "Login Successfully.";
            }
            catch (Exception ex)
            {

            }
            return result;
        }
        public static string HashPassword(string password, string key)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                string combined = password + key;
                byte[] bytes = Encoding.UTF8.GetBytes(combined);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        public static bool VerifyPassword(string inputPassword, string storedHash, string key)
        {
            string hashedInput = HashPassword(inputPassword, key);
            return hashedInput == storedHash;
        }
        private string GenerateJwtToken(users user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
            new Claim(ClaimTypes.Email, user.email)
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<Response> ChangePassword(ChangePassword changePassword)
        {
            DataBaseUtil _db = new DataBaseUtil(_configuration);
            var result = new Response();
            try
            {
                string passwordKey = _configuration["password:Key"];
                string hashedPassword = HashPassword(changePassword.NewPassword, passwordKey);
                Dictionary<string, string> values = new Dictionary<string, string>();
                Dictionary<string, string> where = new Dictionary<string, string>();
                values.Add("password_hash", hashedPassword);
                where.Add("id", changePassword.User_id.ToString());
                var res = await _db.UpdateRecord<users>(values, where);
                if (res == 1)
                {
                    result.status = 1;
                    result.message = "Password Changed Succefully.";
                }
                else
                {
                    result.status = 0;
                    result.message = "Failed to changed password.";
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }
    }
}

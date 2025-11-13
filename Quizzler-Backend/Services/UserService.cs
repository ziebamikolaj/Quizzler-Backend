using System.Net.Mail;
using System.Text;
using Isopoh.Cryptography.Argon2;
using MlkPwgen;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Models;
using Isopoh.Cryptography.SecureArray;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Quizzler_Backend.Controllers.Services
{
    public class UserService
    {
        private readonly QuizzlerDbContext _context;
        private readonly IConfiguration _configuration;

        public UserService(QuizzlerDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<bool> EmailExists(string email)
        {
            return await _context.User.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UsernameExists(string username)
        {
            return await _context.User.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> DoesExist(string usernameOrEmail)
        {
            try
            {
                usernameOrEmail = new MailAddress(usernameOrEmail).Address;
                return await _context.User.AnyAsync(u => u.Email == usernameOrEmail);
            }
            catch (FormatException)
            {
                return await _context.User.AnyAsync(u => u.Username == usernameOrEmail);
            }
        }

        public async Task<bool> AreCredentialsCorrect(LoginDto loginDto)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            string generatedPassword = HashPassword(loginDto.Password, user.LoginInfo.Salt);
            if (generatedPassword == user.LoginInfo.PasswordHash) return true;
            return false;
        }

        public bool IsEmailCorrect(string email)
        {
            try
            {
                email = new MailAddress(email).Address;
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public bool IsPasswordGoodEnough(string password)
        {
            return password.Length >= 8;
        }

        public async Task<User> CreateUser(UserRegisterDto userRegisterDto)
        {
            var user = new User
            {
                Email = userRegisterDto.Email,
                Username = userRegisterDto.Username,
                FirstName = userRegisterDto.FirstName,
                LastName = userRegisterDto.LastName,
                DateRegistered = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow,
                Media = new List<Media>(),
                Lesson = new List<Lesson>(),
                LoginInfo = new LoginInfo
                {
                    Salt = CreateSalt(),
                }

            };

            user.LoginInfo.PasswordHash = HashPassword(userRegisterDto.Password, user.LoginInfo.Salt);
            return user;
        }

        public string CreateSalt()
        {
            return PasswordGenerator.Generate(length: 16, allowed: Sets.Alphanumerics);
        }

        public string HashPassword(string password, string salt)
        {
            var config = new Argon2Config
            {
                Type = Argon2Type.DataIndependentAddressing,
                Version = Argon2Version.Nineteen,
                MemoryCost = 32768,
                Threads = Environment.ProcessorCount,
                Password = Encoding.UTF8.GetBytes(password),
                Salt = Encoding.UTF8.GetBytes(salt),
                HashLength = 60
            };

            var argon2 = new Argon2(config);

            using (SecureArray<byte> hash = argon2.Hash())
            {
                return Convert.ToBase64String(hash.Buffer);
            }
        }

        public string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            };

            var token = new JwtSecurityToken(_configuration["JwtIssuer"],
                _configuration["JwtIssuer"],
                claims,
                expires: DateTime.Now.AddMinutes(60 * 24 * 7),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}

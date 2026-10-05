using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Controllers;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Tests
{
    public class AuthControllerTests
    {
        private RaceDayDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(
                    databaseName: Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        private AuthController CreateControllerWithSession(
            RaceDayDbContext context)
        {
            var controller = new AuthController(context);

            var httpContext = new DefaultHttpContext();

            var session = new TestSession();

            httpContext.Session = session;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        // ==========================================
        // TEST 1
        // Valid Organiser registration
        // ==========================================
        [Fact]
        public async Task Register_ValidOrganiser_ReturnsCreated()
        {
            using var context = CreateContext();

            var controller = new AuthController(context);

            var request = new RegisterDto
            {
                FirstName = "Test",
                LastName = "Organiser",
                Email = "organiser@test.com",
                PhoneNumber = "0712345678",
                Password = "TestPassword123!",
                Role = "Organiser"
            };

            var result = await controller.Register(request);

            var createdResult =
                Assert.IsType<CreatedResult>(result);

            Assert.Equal(
                StatusCodes.Status201Created,
                createdResult.StatusCode);

            var savedUser = await context.Users
                .FirstOrDefaultAsync(
                    u => u.Email == "organiser@test.com");

            Assert.NotNull(savedUser);

            Assert.Equal(
                "Organiser",
                savedUser.Role);

            Assert.NotEqual(
                "TestPassword123!",
                savedUser.PasswordHash);

            Assert.False(
                string.IsNullOrWhiteSpace(
                    savedUser.PasswordHash));
        }

        // ==========================================
        // TEST 2
        // Invalid role
        // ==========================================
        [Fact]
        public async Task Register_InvalidRole_ReturnsBadRequest()
        {
            using var context = CreateContext();

            var controller = new AuthController(context);

            var request = new RegisterDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = "invalidrole@test.com",
                PhoneNumber = "0712345678",
                Password = "TestPassword123!",
                Role = "Admin"
            };

            var result = await controller.Register(request);

            var badRequest =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                StatusCodes.Status400BadRequest,
                badRequest.StatusCode);

            var savedUser = await context.Users
                .FirstOrDefaultAsync(
                    u => u.Email == "invalidrole@test.com");

            Assert.Null(savedUser);
        }

        // ==========================================
        // TEST 3
        // Duplicate email
        // ==========================================
        [Fact]
        public async Task Register_DuplicateEmail_ReturnsConflict()
        {
            using var context = CreateContext();

            context.Users.Add(new User
            {
                FirstName = "Existing",
                LastName = "User",
                Email = "existing@test.com",
                PhoneNumber = "0711111111",
                PasswordHash = "ExistingHash",
                Role = "Participant"
            });

            await context.SaveChangesAsync();

            var controller = new AuthController(context);

            var request = new RegisterDto
            {
                FirstName = "Another",
                LastName = "User",
                Email = "existing@test.com",
                PhoneNumber = "0722222222",
                Password = "AnotherPassword123!",
                Role = "Participant"
            };

            var result = await controller.Register(request);

            var conflict =
                Assert.IsType<ConflictObjectResult>(result);

            Assert.Equal(
                StatusCodes.Status409Conflict,
                conflict.StatusCode);

            var userCount = await context.Users
                .CountAsync(
                    u => u.Email == "existing@test.com");

            Assert.Equal(1, userCount);
        }

        // ==========================================
        // TEST 4
        // Valid login
        // Session stores UserID and Role
        // ==========================================
        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkAndStoresSession()
        {
            using var context = CreateContext();

            var user = new User
            {
                FirstName = "Login",
                LastName = "Organiser",
                Email = "login@test.com",
                PhoneNumber = "0712345678",
                Role = "Organiser"
            };

            var passwordHasher = new PasswordHasher<User>();

            user.PasswordHash =
                passwordHasher.HashPassword(
                    user,
                    "CorrectPassword123!");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller =
                CreateControllerWithSession(context);

            var request = new LoginDto
            {
                Email = "login@test.com",
                Password = "CorrectPassword123!"
            };

            var result = await controller.Login(request);

            var okResult =
                Assert.IsType<OkObjectResult>(result);

            Assert.Equal(
                StatusCodes.Status200OK,
                okResult.StatusCode);

            var sessionUserId =
                controller.HttpContext.Session
                    .GetInt32("UserID");

            var sessionRole =
                controller.HttpContext.Session
                    .GetString("Role");

            Assert.Equal(
                user.UserID,
                sessionUserId);

            Assert.Equal(
                "Organiser",
                sessionRole);
        }

        // ==========================================
        // TEST 5
        // Wrong password
        // ==========================================
        [Fact]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {
            using var context = CreateContext();

            var user = new User
            {
                FirstName = "Login",
                LastName = "Participant",
                Email = "participant@test.com",
                PhoneNumber = "0712345678",
                Role = "Participant"
            };

            var passwordHasher = new PasswordHasher<User>();

            user.PasswordHash =
                passwordHasher.HashPassword(
                    user,
                    "CorrectPassword123!");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller =
                CreateControllerWithSession(context);

            var request = new LoginDto
            {
                Email = "participant@test.com",
                Password = "WrongPassword123!"
            };

            var result = await controller.Login(request);

            var unauthorizedResult =
                Assert.IsType<UnauthorizedObjectResult>(result);

            Assert.Equal(
                StatusCodes.Status401Unauthorized,
                unauthorizedResult.StatusCode);

            Assert.Null(
                controller.HttpContext.Session
                    .GetInt32("UserID"));
        }

        // ==========================================
        // TEST 6
        // Unknown email
        // ==========================================
        [Fact]
        public async Task Login_UnknownEmail_ReturnsUnauthorized()
        {
            using var context = CreateContext();

            var controller =
                CreateControllerWithSession(context);

            var request = new LoginDto
            {
                Email = "unknown@test.com",
                Password = "Password123!"
            };

            var result = await controller.Login(request);

            var unauthorizedResult =
                Assert.IsType<UnauthorizedObjectResult>(result);

            Assert.Equal(
                StatusCodes.Status401Unauthorized,
                unauthorizedResult.StatusCode);
        }
    }

    // ==========================================
    // TEST SESSION
    //
    // Provides an in-memory implementation of
    // ASP.NET Core ISession for controller tests.
    // ==========================================
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage =
            new Dictionary<string, byte[]>();

        public IEnumerable<string> Keys =>
            _sessionStorage.Keys;

        public string Id =>
            Guid.NewGuid().ToString();

        public bool IsAvailable => true;

        public void Clear()
        {
            _sessionStorage.Clear();
        }

        public Task CommitAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task LoadAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _sessionStorage.Remove(key);
        }

        public void Set(
            string key,
            byte[] value)
        {
            _sessionStorage[key] = value;
        }

        public bool TryGetValue(
            string key,
            out byte[] value)
        {
            if (_sessionStorage.TryGetValue(
                key,
                out var storedValue))
            {
                value = storedValue;
                return true;
            }

            value = Array.Empty<byte>();
            return false;
        }
    }
}
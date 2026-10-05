using Microsoft.AspNetCore.Http;
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

        // ==========================================
        // TEST 1
        // Valid Organiser registration
        // ==========================================
        [Fact]
        public async Task Register_ValidOrganiser_ReturnsCreated()
        {
            // Arrange
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

            // Act
            var result = await controller.Register(request);

            // Assert
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

            // Password must be hashed.
            Assert.NotEqual(
                "TestPassword123!",
                savedUser.PasswordHash);

            Assert.False(
                string.IsNullOrWhiteSpace(
                    savedUser.PasswordHash));
        }

        // ==========================================
        // TEST 2
        // Invalid role must be rejected
        // ==========================================
        [Fact]
        public async Task Register_InvalidRole_ReturnsBadRequest()
        {
            // Arrange
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

            // Act
            var result = await controller.Register(request);

            // Assert
            var badRequest =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                StatusCodes.Status400BadRequest,
                badRequest.StatusCode);

            // Make sure invalid user was not saved.
            var savedUser = await context.Users
                .FirstOrDefaultAsync(
                    u => u.Email == "invalidrole@test.com");

            Assert.Null(savedUser);
        }

        // ==========================================
        // TEST 3
        // Duplicate email must be rejected
        // ==========================================
        [Fact]
        public async Task Register_DuplicateEmail_ReturnsConflict()
        {
            // Arrange
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

            // Act
            var result = await controller.Register(request);

            // Assert
            var conflict =
                Assert.IsType<ConflictObjectResult>(result);

            Assert.Equal(
                StatusCodes.Status409Conflict,
                conflict.StatusCode);

            // There must still only be one account
            // using this email address.
            var userCount = await context.Users
                .CountAsync(
                    u => u.Email == "existing@test.com");

            Assert.Equal(1, userCount);
        }
    }
}
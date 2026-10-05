using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Controllers;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;

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

            // Password must not be stored as plain text.
            Assert.NotEqual(
                "TestPassword123!",
                savedUser.PasswordHash);

            Assert.False(
                string.IsNullOrWhiteSpace(
                    savedUser.PasswordHash));
        }
    }
}
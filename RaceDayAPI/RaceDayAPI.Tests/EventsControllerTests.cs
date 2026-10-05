using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Controllers;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Tests
{
    public class EventsControllerTests
    {
        private RaceDayDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(
                    databaseName: Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        private EventsController CreateControllerWithSession(
            RaceDayDbContext context,
            int? userId,
            string? role)
        {
            var controller = new EventsController(context);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            if (userId.HasValue)
            {
                httpContext.Session.SetInt32(
                    "UserID",
                    userId.Value);
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                httpContext.Session.SetString(
                    "Role",
                    role);
            }

            controller.ControllerContext =
                new ControllerContext
                {
                    HttpContext = httpContext
                };

            return controller;
        }

        private Event CreateTestEvent()
        {
            return new Event
            {
                EventTypeID = 1,
                Name = "Polokwane Race Day",
                Description = "Test running event",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Polokwane",
                Distance = 10
            };
        }

        // ==========================================
        // TEST 1
        // Organiser can create an event
        // ==========================================
        [Fact]
        public async Task CreateEvent_Organiser_ReturnsCreated()
        {
            using var context = CreateContext();

            var controller = CreateControllerWithSession(
                context,
                10,
                "Organiser");

            var raceEvent = CreateTestEvent();

            var result =
                await controller.CreateEvent(raceEvent);

            var createdResult =
                Assert.IsType<CreatedAtActionResult>(
                    result.Result);

            Assert.Equal(
                StatusCodes.Status201Created,
                createdResult.StatusCode);

            var savedEvent =
                await context.Events.FirstOrDefaultAsync();

            Assert.NotNull(savedEvent);

            Assert.Equal(
                "Polokwane Race Day",
                savedEvent.Name);

            // OrganiserID must come from the session.
            Assert.Equal(
                10,
                savedEvent.OrganiserID);
        }

        // ==========================================
        // TEST 2
        // Participant cannot create an event
        // ==========================================
        [Fact]
        public async Task CreateEvent_Participant_ReturnsForbidden()
        {
            using var context = CreateContext();

            var controller = CreateControllerWithSession(
                context,
                20,
                "Participant");

            var raceEvent = CreateTestEvent();

            var result =
                await controller.CreateEvent(raceEvent);

            var objectResult =
                Assert.IsType<ObjectResult>(
                    result.Result);

            Assert.Equal(
                StatusCodes.Status403Forbidden,
                objectResult.StatusCode);

            Assert.Empty(context.Events);
        }

        // ==========================================
        // TEST 3
        // Unauthenticated user cannot create event
        // ==========================================
        [Fact]
        public async Task CreateEvent_NotLoggedIn_ReturnsUnauthorized()
        {
            using var context = CreateContext();

            var controller = CreateControllerWithSession(
                context,
                null,
                null);

            var raceEvent = CreateTestEvent();

            var result =
                await controller.CreateEvent(raceEvent);

            var unauthorizedResult =
                Assert.IsType<UnauthorizedObjectResult>(
                    result.Result);

            Assert.Equal(
                StatusCodes.Status401Unauthorized,
                unauthorizedResult.StatusCode);

            Assert.Empty(context.Events);
        }
    }
}
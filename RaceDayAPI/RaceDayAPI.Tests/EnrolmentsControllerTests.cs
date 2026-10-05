using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Controllers;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Tests
{
    public class EnrolmentsControllerTests
    {
        private RaceDayDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(
                    databaseName: Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        private EnrolmentsController CreateControllerWithSession(
            RaceDayDbContext context,
            int? userId,
            string? role)
        {
            var controller = new EnrolmentsController(context);

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

        private async Task AddEventAndCategory(
            RaceDayDbContext context)
        {
            var raceEvent = new Event
            {
                EventID = 1,
                OrganiserID = 100,
                EventTypeID = 1,
                Name = "Polokwane Race Day",
                Description = "Test event",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Polokwane",
                Distance = 10
            };

            var category = new Category
            {
                CategoryID = 1,
                EventID = 1,
                CategoryName = "10km",
                CategoryType = "Distance"
            };

            context.Events.Add(raceEvent);
            context.Categories.Add(category);

            await context.SaveChangesAsync();
        }

        // ==========================================
        // TEST 1
        // Participant can enrol
        // ==========================================
        [Fact]
        public async Task CreateEnrolment_Participant_ReturnsCreated()
        {
            using var context = CreateContext();

            await AddEventAndCategory(context);

            var controller = CreateControllerWithSession(
                context,
                50,
                "Participant");

            var enrolment = new Enrolment
            {
                EventID = 1,
                CategoryID = 1,

                // This deliberately uses the wrong value.
                // The API must replace it with the
                // logged-in Participant ID.
                ParticipantID = 999
            };

            var result =
                await controller.CreateEnrolment(enrolment);

            var createdResult =
                Assert.IsType<CreatedAtActionResult>(
                    result.Result);

            Assert.Equal(
                StatusCodes.Status201Created,
                createdResult.StatusCode);

            var savedEnrolment =
                await context.Enrolments.FirstOrDefaultAsync();

            Assert.NotNull(savedEnrolment);

            // ParticipantID must come from session.
            Assert.Equal(
                50,
                savedEnrolment.ParticipantID);

            Assert.Equal(
                1,
                savedEnrolment.EventID);

            Assert.Equal(
                1,
                savedEnrolment.CategoryID);
        }

        // ==========================================
        // TEST 2
        // Organiser cannot enrol
        // ==========================================
        [Fact]
        public async Task CreateEnrolment_Organiser_ReturnsForbidden()
        {
            using var context = CreateContext();

            await AddEventAndCategory(context);

            var controller = CreateControllerWithSession(
                context,
                100,
                "Organiser");

            var enrolment = new Enrolment
            {
                EventID = 1,
                CategoryID = 1
            };

            var result =
                await controller.CreateEnrolment(enrolment);

            var objectResult =
                Assert.IsType<ObjectResult>(
                    result.Result);

            Assert.Equal(
                StatusCodes.Status403Forbidden,
                objectResult.StatusCode);

            Assert.Empty(context.Enrolments);
        }

        // ==========================================
        // TEST 3
        // User must be logged in
        // ==========================================
        [Fact]
        public async Task CreateEnrolment_NotLoggedIn_ReturnsUnauthorized()
        {
            using var context = CreateContext();

            await AddEventAndCategory(context);

            var controller = CreateControllerWithSession(
                context,
                null,
                null);

            var enrolment = new Enrolment
            {
                EventID = 1,
                CategoryID = 1
            };

            var result =
                await controller.CreateEnrolment(enrolment);

            var unauthorizedResult =
                Assert.IsType<UnauthorizedObjectResult>(
                    result.Result);

            Assert.Equal(
                StatusCodes.Status401Unauthorized,
                unauthorizedResult.StatusCode);

            Assert.Empty(context.Enrolments);
        }

        // ==========================================
        // TEST 4
        // Duplicate enrolment is rejected
        // ==========================================
        [Fact]
        public async Task CreateEnrolment_Duplicate_ReturnsConflict()
        {
            using var context = CreateContext();

            await AddEventAndCategory(context);

            context.Enrolments.Add(new Enrolment
            {
                ParticipantID = 50,
                EventID = 1,
                CategoryID = 1,
                EnrolmentDate = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            var controller = CreateControllerWithSession(
                context,
                50,
                "Participant");

            var secondEnrolment = new Enrolment
            {
                EventID = 1,
                CategoryID = 1
            };

            var result =
                await controller.CreateEnrolment(
                    secondEnrolment);

            var conflictResult =
                Assert.IsType<ConflictObjectResult>(
                    result.Result);

            Assert.Equal(
                StatusCodes.Status409Conflict,
                conflictResult.StatusCode);

            Assert.Equal(
                1,
                await context.Enrolments.CountAsync());
        }

        // ==========================================
        // TEST 5
        // Category must belong to selected event
        // ==========================================
        [Fact]
        public async Task CreateEnrolment_CategoryFromDifferentEvent_ReturnsBadRequest()
        {
            using var context = CreateContext();

            await AddEventAndCategory(context);

            // Add a second event.
            context.Events.Add(new Event
            {
                EventID = 2,
                OrganiserID = 100,
                EventTypeID = 1,
                Name = "Second Race",
                Description = "Another test event",
                EventDate = DateTime.UtcNow.AddDays(60),
                Location = "Limpopo",
                Distance = 21
            });

            await context.SaveChangesAsync();

            var controller = CreateControllerWithSession(
                context,
                50,
                "Participant");

            // Category 1 belongs to Event 1,
            // but this request selects Event 2.
            var enrolment = new Enrolment
            {
                EventID = 2,
                CategoryID = 1
            };

            var result =
                await controller.CreateEnrolment(enrolment);

            var badRequest =
                Assert.IsType<BadRequestObjectResult>(
                    result.Result);

            Assert.Equal(
                StatusCodes.Status400BadRequest,
                badRequest.StatusCode);

            Assert.Empty(context.Enrolments);
        }
    }
}
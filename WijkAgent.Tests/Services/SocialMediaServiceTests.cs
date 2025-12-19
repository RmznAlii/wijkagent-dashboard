using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using WijkAgent.Core.Models;
using WijkAgent.Core.Services;

namespace WijkAgent.Tests.Services
{
    public class SocialMediaServiceTests
    {
        [Fact]
        public async Task TC4_02_AlleenBerichtenBinnen30MinutenWordenVertoond()
        {
            // Arrange
            var incidentTime = new DateTime(2025, 1, 10, 20, 0, 0);

            var posts = new List<SocialMediaPost>
            {
                new SocialMediaPost
                {
                    Content = "Overlast op het plein",
                    PostedAt = incidentTime.AddMinutes(10),
                    Platform = "X"
                },
                new SocialMediaPost
                {
                    Content = "Veel politie aanwezig",
                    PostedAt = incidentTime.AddMinutes(29),
                    Platform = "X"
                },
                new SocialMediaPost
                {
                    Content = "Rust weer terug",
                    PostedAt = incidentTime.AddMinutes(31), // buiten tijdvenster
                    Platform = "X"
                }
            };

            var service = new FakeSocialMediaService(posts);

            // Act
            var result = await service.GetPostsForIncidentAsync(
                incidentTime,
                new[] { "overlast", "politie" });

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, post =>
                Assert.InRange(
                    post.PostedAt,
                    incidentTime,
                    incidentTime.AddMinutes(30)));
        }
    }
}

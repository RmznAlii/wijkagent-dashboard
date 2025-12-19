using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using WijkAgent.Core.Models;
using WijkAgent.Core.Services;

namespace WijkAgent.Tests.Services
{
    public class SocialMediaServiceLinkingTests
    {
        [Fact]
        public async Task TC4_03_BerichtenZijnGekoppeldAanEenDelictGebaseerdOpHetTijdstipVanDelict()
        {
            // Arrange
            var crimeA_Time = new DateTime(2025, 1, 10, 20, 0, 0);
            var crimeB_Time = new DateTime(2025, 1, 11, 10, 0, 0);

            var posts = new List<SocialMediaPost>
            {
                new SocialMediaPost
                {
                    Content = "Overlast in de avond",
                    PostedAt = crimeA_Time.AddMinutes(15),
                    Platform = "X"
                },
                new SocialMediaPost
                {
                    Content = "Ochtendincident gemeld",
                    PostedAt = crimeB_Time.AddMinutes(10),
                    Platform = "X"
                }
            };

            var service = new FakeSocialMediaService(posts);

            // Act
            var resultForCrimeA = await service.GetPostsForIncidentAsync(
                crimeA_Time,
                new[] { "overlast" });

            var resultForCrimeB = await service.GetPostsForIncidentAsync(
                crimeB_Time,
                new[] { "incident" });

            // Assert
            Assert.Single(resultForCrimeA);
            Assert.Contains(resultForCrimeA, p => p.Content.Contains("avond"));

            Assert.Single(resultForCrimeB);
            Assert.Contains(resultForCrimeB, p => p.Content.Contains("Ochtend"));
        }
    }
}

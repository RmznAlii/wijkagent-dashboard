using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WijkAgent.Core.Models;

namespace WijkAgent.Core.Services
{
    public class MockSocialMediaService : ISocialMediaService
    {
        private readonly List<SocialMediaPost> _mockPosts;

        public MockSocialMediaService()
        {
            _mockPosts = new List<SocialMediaPost>
            {
                new SocialMediaPost
                {
                    Id = "1",
                    Username = "amsterdam_news",
                    Content = "Wat een chaos net bij het Gelderlandplein, politie overal.",
                    PostedAt = DateTime.Now.AddMinutes(-10),
                    PostUrl = "https://x.com/amsterdam_news/1",
                },
                new SocialMediaPost
                {
                    Id = "2",
                    Username = "eyewitness123",
                    Content = "Ik zag net een groep jongens wegrennen na een steekpartij.",
                    PostedAt = DateTime.Now.AddMinutes(-5),
                    PostUrl = "https://x.com/eyewitness123/2",
                },
                new SocialMediaPost
                {
                    Id = "3",
                    Username = "user323",
                    Content = "Lekker shoppen in Gelderlandplein!",
                    PostedAt = DateTime.Now.AddMinutes(-8),
                    PostUrl = "https://x.com/user323/3",
                },
                new SocialMediaPost
                {
                    Id = "4",
                    Username = "random_user",
                    Content = "Mooi weer vandaag in Amsterdam!",
                    PostedAt = DateTime.Now.AddMinutes(-12),
                    PostUrl = "https://x.com/random_user/4",
                },
                new SocialMediaPost
                {
                    Id = "5",
                    Username = "late_report",
                    Content = "Gisteren was het nog onrustig bij het plein.",
                    PostedAt = DateTime.Now.AddHours(-2),
                    PostUrl = "https://x.com/late_report/5",
                }
            };
        }

        public async Task<IReadOnlyList<SocialMediaPost>> GetPostsForIncidentAsync(
            DateTime incidentTime,
            IEnumerable<string> keywords,
            CancellationToken cancellationToken = default)
        {
            // Simuleer API latency (realistisch maar < 5 sec)
            await Task.Delay(800, cancellationToken);

            var keywordList = keywords
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Select(k => k.ToLowerInvariant())
                .ToList();

            var fromTime = incidentTime.AddMinutes(-30);
            var toTime = incidentTime.AddMinutes(30);

            var results = _mockPosts
                .Where(post => post.PostedAt >= fromTime && post.PostedAt <= toTime)
                .Where(post =>
                    !keywordList.Any() ||
                    keywordList.Any(keyword =>
                        post.Content.ToLowerInvariant().Contains(keyword)))
                .OrderByDescending(post => post.PostedAt)
                .ToList();

            return results;
        }
    }
}

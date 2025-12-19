using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WijkAgent.Core.Models;
using WijkAgent.Core.Services;

namespace WijkAgent.Tests.Services
{
    public class FakeSocialMediaService : ISocialMediaService
    {
        private readonly List<SocialMediaPost> _posts;

        public FakeSocialMediaService(List<SocialMediaPost> posts)
        {
            _posts = posts;
        }

        public Task<IReadOnlyList<SocialMediaPost>> GetPostsForIncidentAsync(
            DateTime incidentTime,
            IEnumerable<string> keywords)
        {
            var result = _posts
                .Where(p =>
                    p.PostedAt >= incidentTime &&
                    p.PostedAt <= incidentTime.AddMinutes(30))
                .ToList();

            return Task.FromResult<IReadOnlyList<SocialMediaPost>>(result);
        }

        public Task<IReadOnlyList<SocialMediaPost>> GetPostsForIncidentAsync(DateTime incidentTime, IEnumerable<string> keywords, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}

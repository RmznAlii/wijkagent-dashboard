using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WijkAgent.Core.Models;

namespace WijkAgent.Core.Services
{
    public class SocialMediaControlService : ISocialMediaService
    {
        private readonly SocialMediaService _apiService;
        private readonly MockSocialMediaService _mockService;

        public SocialMediaControlService(
            SocialMediaService apiService,
            MockSocialMediaService mockService)
        {
            _apiService = apiService;
            _mockService = mockService;
        }

        public async Task<IReadOnlyList<SocialMediaPost>> GetPostsForIncidentAsync(
            DateTime incidentTime,
            IEnumerable<string> keywords,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _apiService.GetPostsForIncidentAsync(
                    incidentTime, keywords, cancellationToken);
            }
            catch (Exception ex)
            {
                // Fallback bij bekende API-problemen (rate limit, bad request, netwerk)
                if (ex.Message.Contains("429") ||
                    ex.Message.Contains("Too Many Requests") ||
                    ex.Message.Contains("X API error"))
                {
                    return await _mockService.GetPostsForIncidentAsync(
                        incidentTime, keywords, cancellationToken);
                }

                // Onbekende fout → doorgeven
                throw;
            }
        }
    }
}

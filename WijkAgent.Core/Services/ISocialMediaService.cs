using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WijkAgent.Core.Models;

namespace WijkAgent.Core.Services
{
    public interface ISocialMediaService
    {
        Task<IReadOnlyList<SocialMediaPost>> GetPostsForIncidentAsync(
            DateTime incidentTime,
            IEnumerable<string> keywords,
            CancellationToken cancellationToken = default);
    }
}

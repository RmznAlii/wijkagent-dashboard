using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WijkAgent.Core.Models
{
    public class SocialMediaPost
    {
        public string Id { get; set; } = string.Empty;

        public string Platform { get; set; } = "X";

        public string Username { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime PostedAt { get; set; }

        public string PostUrl { get; set; } = string.Empty;
    }
}

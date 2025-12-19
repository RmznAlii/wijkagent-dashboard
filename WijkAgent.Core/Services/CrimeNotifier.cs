using System;
using WijkAgent.Core.Models;

namespace WijkAgent.Core.Services
{
    // Simple, thread-safe publish/subscribe for in-process notifications.
    public sealed class CrimeNotifier
    {
        public event Action<Crime>? CrimeAdded;

        public void Publish(Crime crime)
        {
            var handler = CrimeAdded;
            handler?.Invoke(crime);
        }
    }
}

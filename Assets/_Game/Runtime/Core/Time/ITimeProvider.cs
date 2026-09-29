using System;

namespace Gravivore.Core.Time
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }

    public sealed class SystemUtcTimeProvider : ITimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}

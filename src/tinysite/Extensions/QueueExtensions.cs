using System.Collections.Generic;

namespace TinySite.Extensions
{
    public static class QueueExtensions
    {
        public static Queue<T> EnqueueRange<T>(this Queue<T> q, IEnumerable<T> items)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    q.Enqueue(item);
                }
            }

            return q;
        }
    }
}

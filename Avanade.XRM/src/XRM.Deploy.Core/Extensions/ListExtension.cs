using System.Collections.Generic;

namespace XRM.Deploy.Core.Extensions
{
    internal static class ListExtension
    {
        public static void AddIfNotNull<T>(this List<T> list, T item)
        {
            if (item != null)
            {
                list.Add(item);
            }
        }
    }
}

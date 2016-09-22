using System.Collections.Generic;

namespace Avanade.XRM.Deployer.Extensions
{
	public static class ListExtension
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

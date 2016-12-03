using System;

namespace XRM.Deploy.Core.Models
{
	internal class SolutionDetailModel
	{
		public SolutionDetailModel(Guid id)
		{
			Id = id;
		}

		internal Guid Id { get; set; }
	}
}
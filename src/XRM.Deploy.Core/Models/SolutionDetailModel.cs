using System;

namespace XRM.Deploy.Core.Models
{
    internal class SolutionDetailModel
    {
        public SolutionDetailModel(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}
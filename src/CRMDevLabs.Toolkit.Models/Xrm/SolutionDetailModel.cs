using System;

namespace CRMDevLabs.Toolkit.Models.Xrm
{
    public class SolutionDetailModel
    {
        public SolutionDetailModel(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}
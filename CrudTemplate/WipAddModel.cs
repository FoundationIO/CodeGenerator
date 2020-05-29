using System;
using LogR.Common.Models.DBView;

namespace LogR.Common.Models.ViewModels
{
    public class EEntity123CreateModel : EEntity123Model
    {
        private string TenantName { get; set; }

        //%InputModelVariable%

        private long EEntity123Id { get; set; }

        private Guid EEntity123Guid { get; set; }

        private new bool IsDeleted { get; set; }

        private new DateTime CreatedDate { get; set; }

        private new string CreatedBy { get; set; }

        private new DateTime ModifiedDate { get; set; }

        private new string ModifiedBy { get; set; }
    }
}
/**
Copyright (c) 2016 Foundation.IO (https://github.com/foundationio). All rights reserved.

This work is licensed under the terms of the BSD license.
For a copy, see <https://opensource.org/licenses/BSD-3-Clause>.
**/
using System;
using LogR.Common.Models.DBView;

namespace LogR.Common.Models.ViewModels
{
    public class EEntity123UpdateModel : EEntity123Model
    {
        private string TenantName { get; set; }

        //%InputModelVariable%

        private new bool IsDeleted { get; set; }

        private new DateTime CreatedDate { get; set; }

        private new string CreatedBy { get; set; }

        private new DateTime ModifiedDate { get; set; }

        private new string ModifiedBy { get; set; }
    }
}
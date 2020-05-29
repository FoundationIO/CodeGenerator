/**
Copyright (c) 2016 Foundation.IO (https://github.com/foundationio). All rights reserved.

This work is licensed under the terms of the BSD license.
For a copy, see <https://opensource.org/licenses/BSD-3-Clause>.
**/
using LinqToDB.Mapping;
using LogR.Common.Models.DB;

namespace LogR.Common.Models.DBView
{
    public class EEntity123View : EEntity123
    {
        //%ViewAndModelVariable%
        [NotColumn]
        public long TenantId { get; set; }
        [NotColumn]
        public string TenantName { get; set; }
    }
}
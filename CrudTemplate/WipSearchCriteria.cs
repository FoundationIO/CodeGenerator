/**
Copyright (c) 2016 Foundation.IO (https://github.com/foundationio). All rights reserved.

This work is licensed under the terms of the BSD license.
For a copy, see <https://opensource.org/licenses/BSD-3-Clause>.
**/
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Framework.Infrastructure.Models.Search;

namespace LogR.Common.Models.SearchCriteria
{
    public class EEntity123SearchCriteria : BaseSearchCriteria
    {
        public long? TenantId { get; set; }
        public bool ShowDeleted { get; set; } = false;
        //%SearchVariable%
    }
}
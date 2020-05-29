/**
Copyright (c) 2016 Foundation.IO (https://github.com/foundationio). All rights reserved.

This work is licensed under the terms of the BSD license.
For a copy, see <https://opensource.org/licenses/BSD-3-Clause>.
**/
using Newtonsoft.Json;
using System;

namespace CodeGenerator.Models
{
    public class InputParam
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public InputTypes InputType { get; set; }
        [JsonIgnore]
        public string TypeFullName { get; set; }

    }
}

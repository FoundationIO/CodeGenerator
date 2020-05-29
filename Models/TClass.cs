/**
Copyright (c) 2016 Foundation.IO (https://github.com/foundationio). All rights reserved.

This work is licensed under the terms of the BSD license.
For a copy, see <https://opensource.org/licenses/BSD-3-Clause>.
**/
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeGenerator.Models
{
    [Serializable]
    public class TClass
    {
        public string ClassName { get; set; } = "";
        public string ApiClassName { get { if (ClassName == null) return ClassName; return ClassName.Replace("Controller", ""); } }
        public List<TMethod> Methods { get; set; } = new List<TMethod>();
    }
}

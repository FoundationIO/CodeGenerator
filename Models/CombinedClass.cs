/**
Copyright (c) 2016 Foundation.IO (https://github.com/foundationio). All rights reserved.

This work is licensed under the terms of the BSD license.
For a copy, see <https://opensource.org/licenses/BSD-3-Clause>.
**/
namespace CodeGenerator.Models
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class CombinedClass
    {
        public List<TypescriptModelClass> TypescriptModelClassInfoList { get; set; } = new List<TypescriptModelClass>();
        public List<TClass> FunctionInfoList { get; set; } = new List<TClass>();
    }
}

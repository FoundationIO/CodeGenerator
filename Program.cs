/**
Copyright (c) 2016 Foundation.IO (https://github.com/foundationio). All rights reserved.

This work is licensed under the terms of the BSD license.
For a copy, see <https://opensource.org/licenses/BSD-3-Clause>.
**/
namespace CodeGenerator
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
                throw new System.Exception("No Parameter sent");
            if (args[0] == "crud")
            {
                CrudGeneratorProgram.MainApp(args);
            }
            else if (args[0] == "migration")
            {
                MigrationProgram.MainApp();
            }
            else if (args[0] == "react")
            {
                ReactModelGeneratorProgram.MainApp();
            }
            else if (args[0] == "tapi")
            {
                ApiModelGeneratorProgram.MainApp(isTest: true);
            }
            else if (args[0] == "api")
            {
                ApiModelGeneratorProgram.MainApp(isTest: false);
                ApiModelGeneratorProgram.MainApp(isTest: true);

            }
            else
            {
                throw new System.Exception("No Parameter sent");
            }

        }
    }
}

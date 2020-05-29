/**
Copyright (c) 2016 Foundation.IO (https://github.com/foundationio). All rights reserved.

This work is licensed under the terms of the BSD license.
For a copy, see <https://opensource.org/licenses/BSD-3-Clause>.
**/
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Text;
using LinqToDB.Extensions;
using LinqToDB.Mapping;

namespace CodeGenerator
{
    public static class CrudGeneratorProgram
    {
        public static void MainApp(string[] args)
        {
            var tableName = args[1];
            var t = GetTableType(tableName);
            if (t == null)
                throw new Exception($"Unable to find the Type for table name - {tableName}");

            var modelDir = Directory.GetCurrentDirectory() + "\\..\\..\\..\\Backend\\Api\\All\\ViewModels\\WIP\\";
            var viewDir = Directory.GetCurrentDirectory() + "\\..\\..\\..\\Backend\\Api\\All\\Views\\WIP\\";
            var contrlDir = Directory.GetCurrentDirectory() + "\\..\\..\\..\\Backend\\Api\\All\\Controllers\\WIP\\";
            var searchDir = Directory.GetCurrentDirectory() + "\\..\\..\\..\\Backend\\Api\\All\\SearchModels\\WIP\\";
            var auditDir = Directory.GetCurrentDirectory() + "\\..\\..\\..\\Backend\\Api\\All\\Service\\Completed\\App\\";
            var permDir = Directory.GetCurrentDirectory() + "\\..\\..\\..\\Backend\\Api\\All\\Service\\Completed\\App\\";

            Generate(t, tableName, "WipController.cs", $"WipController.{tableName}.cs", contrlDir);
            Generate(t, tableName, "WipRepository.cs", $"WipRepository.{tableName}.cs", null);
            Generate(t, tableName, "WipAuditService.cs", $"AuditService.{tableName}.cs", auditDir);
            Generate(t, tableName, "WipPermissionService.cs", $"PermissionService.{tableName}.cs", permDir);
            Generate(t, tableName, "WipService.cs", $"WipService.{tableName}.cs", null);
            Generate(t, tableName, "WipModel.cs", tableName + "Model.cs", modelDir);
            Generate(t, tableName, "WipAddModel.cs", tableName + "CreateModel.cs", modelDir);
            Generate(t, tableName, "WipEditModel.cs", tableName + "UpdateModel.cs", modelDir);
            Generate(t, tableName, "WipView.cs", tableName + "View.cs", viewDir);
            Generate(t, tableName, "WipSearchCriteria.cs", tableName + "SearchCriteria.cs", searchDir);
        }

        static Dictionary<string, Type> cache = new Dictionary<string, Type>();
        static CrudGeneratorProgram()
        {
            var t = typeof(LogR.Common.Constants.ConfigurationConstants);

            var assembly = t.Assembly;
            foreach (var aClass in assembly.GetTypes())
            {
                if (!aClass.FullName.StartsWith("LogR.Common.Models.DB"))
                    continue;

                cache.Add(aClass.Name.Trim().ToLower(), aClass);
            }
        }


        public static string Generate(Type t, string tableName, string templateFile, string sourceFile, string folder)
        {
            var templateFileLocation = GetTemplateFileLocation(templateFile);
            var sourceFileLocation = GetSourceFileLocation(sourceFile, folder);

            var capsFirstLetterString = tableName[0].ToString().ToUpper() + tableName.Substring(1);
            var lowerFirstLetterString = tableName[0].ToString().ToLower() + tableName.Substring(1);

            var str = File.ReadAllText(templateFileLocation);
            str = str.Replace("//%RepoJoin%", GetRepoJoin(tableName, t), StringComparison.CurrentCulture);
            str = str.Replace("//%RepoAssignment%", GetRepoAssignment(t), StringComparison.CurrentCulture);
            str = str.Replace("//%RepoWhereCheck%", GetRepoWhereCheck(t), StringComparison.CurrentCulture);
            str = str.Replace("//%ServiceChecks%", GetServiceCheck(t), StringComparison.CurrentCulture);
            str = str.Replace("//%ViewAndModelVariable%", GetViewAndModelVariable(t), StringComparison.CurrentCulture);
            str = str.Replace("//%SearchVariable%", GetSearchVariable(t), StringComparison.CurrentCulture);
            str = str.Replace("//%RepoConditionalWhere%", GetRepoAssignmentWhere(t), StringComparison.CurrentCulture);
            str = str.Replace("//%RepoAddNameCheck%", GetRepoAddNameCheck(t), StringComparison.CurrentCulture);
            str = str.Replace("//%RepoEditNameCheck%", GetRepoEditNameCheck(t), StringComparison.CurrentCulture);
            str = str.Replace("//%ExistsCheck%", GetRepoEditNameCheckExistsCheckVariable(t), StringComparison.CurrentCulture);
            str = str.Replace("//%InputModelVariable%", GetInputModelVariable(t), StringComparison.CurrentCulture);

            str = str.Replace("EEntity123", capsFirstLetterString, StringComparison.CurrentCulture);
            str = str.Replace("eentity123", lowerFirstLetterString, StringComparison.CurrentCulture);

            if (File.Exists(sourceFileLocation))
            {
                File.Delete(sourceFileLocation);
            }

            File.WriteAllText(sourceFileLocation, str);

            //Process.Start("notepad.exe", sourceFileLocation);

            return sourceFileLocation;
        }

        private static string GetServiceCheck(Type t)
        {
            var sb = new StringBuilder();
            var i = 0;
            foreach (var p in t.GetProperties())
            {
                if (p.Name == t.Name + "Guid" || p.Name == t.Name + "Id" || p.Name == "CreatedBy" || p.Name == "ModifiedBy" || p.Name == "CreatedDate" || p.Name == "ModifiedDate")
                    continue;

                var isNotNull = false;
                var rlst = p.GetCustomAttributes(typeof(NotNullAttribute), true);
                if (!(rlst == null || rlst.Length == 0))
                {
                    isNotNull = true;
                }

                var isString = false;
                var sllst = p.GetCustomAttributes(typeof(StringLengthAttribute), true);
                if (!(sllst == null || sllst.Length == 0))
                {
                    isString = true;
                }

                var isLong = (p.PropertyType == typeof(long) || p.PropertyType == typeof(long?));

                var isDateTime = (p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

                var isGuid = (p.PropertyType == typeof(Guid) || p.PropertyType == typeof(Guid?));

                if (isLong && isNotNull)
                {
                    var space = i == 0 ? "" : "            ";
                    const string space2 = "            ";
                    sb.AppendLine(space + $"if (eentity123Model.{p.Name} == 0)");
                    sb.AppendLine(space2 + $"{{");
                    sb.AppendLine(space2 + $"    result.Add(new ReturnErrorItem(\"{p.Name}\", \"{p.Name} cannot be empty\"));");
                    sb.AppendLine(space2 + $"}}");

                    if (p.Name.EndsWith("TypeId"))
                    {
                        var fKeyType = GetFkeyType(t.Name, p);
                        if (fKeyType != null && CodeGenUtils.IsEnumAvailable(fKeyType.Name + "s"))
                        {
                            sb.AppendLine(space2 + $"else if (!ValidationUtils.IsEnumValid<{fKeyType.Name}s,long>(eentity123Model.{p.Name}))");
                            sb.AppendLine(space2 + $"{{");
                            sb.AppendLine(space2 + $"    result.Add(new ReturnErrorItem(\"{p.Name}\", \"{p.Name} is not a valid {fKeyType.Name}\"));");
                            sb.AppendLine(space2 + $"}}");
                        }
                    }

                    i++;
                }
                else if (isString)
                {
                    var space = i == 0 ? "" : "            ";
                    const string space2 = "            ";
                    sb.AppendLine(space + $"if (eentity123Model.{p.Name} != null && eentity123Model.{p.Name}.Length > EEntity123Model.COLUMN_SIZE_{p.Name})");
                    sb.AppendLine(space2 + $"{{");
                    sb.AppendLine(space2 + $"    result.Add(new ReturnErrorItem(\"{p.Name}\", $\"{p.Name} size should not exceed {{EEntity123Model.COLUMN_SIZE_{p.Name}}}\"));");
                    sb.AppendLine(space2 + $"}}");

                    if (isNotNull)
                    {
                        sb.AppendLine(space2 + $"if (eentity123Model.{p.Name} == null)");
                        sb.AppendLine(space2 + $"{{");
                        sb.AppendLine(space2 + $"    result.Add(new ReturnErrorItem(\"{p.Name}\", \"{p.Name} is reqired \"));");
                        sb.AppendLine(space2 + $"}}");
                    }
                    i++;
                }
                else if (isDateTime)
                {
                    var space = i == 0 ? "" : "            ";
                    const string space2 = "            ";
                    if (!isNotNull)
                    {
                        sb.AppendLine(space + $"if (eentity123Model.{p.Name} != null && !DateUtils.IsValidDate(eentity123Model.{p.Name}.Value))");
                        sb.AppendLine(space2 + $"{{");
                        sb.AppendLine(space2 + $"    result.Add(new ReturnErrorItem(\"{p.Name}\", $\"{p.Name} is not a validate datetime\"));");
                        sb.AppendLine(space2 + $"}}");
                    }
                    else
                    {
                        sb.AppendLine(space + $"if (!DateUtils.IsValidDate(eentity123Model.{p.Name}))");
                        sb.AppendLine(space2 + $"{{");
                        sb.AppendLine(space2 + $"    result.Add(new ReturnErrorItem(\"{p.Name}\", $\"{p.Name} is not a validate datetime\"));");
                        sb.AppendLine(space2 + $"}}");
                    }
                    i++;
                }
                else if (isGuid)
                {
                    var space = i == 0 ? "" : "            ";
                    const string space2 = "            ";
                    if (!isNotNull)
                    {
                        sb.AppendLine(space + $"if (eentity123Model.{p.Name} != null && Guid.Empty  == eentity123Model.{p.Name}.Value)");
                        sb.AppendLine(space2 + $"{{");
                        sb.AppendLine(space2 + $"    result.Add(new ReturnErrorItem(\"{p.Name}\", $\"{p.Name} is not a validate guid\"));");
                        sb.AppendLine(space2 + $"}}");
                    }
                    else
                    {
                        sb.AppendLine(space + $"if (Guid.Empty  == eentity123Model.{p.Name})");
                        sb.AppendLine(space2 + $"{{");
                        sb.AppendLine(space2 + $"    result.Add(new ReturnErrorItem(\"{p.Name}\", $\"{p.Name} is not a validate guid\"));");
                        sb.AppendLine(space2 + $"}}");
                    }
                    i++;
                }
            }
            return sb.ToString();
        }

        private static string GetRepoWhereCheck(Type t)
        {
            var sb = new StringBuilder();
            var i = 0;
            foreach (var p in t.GetProperties())
            {
                if (p.Name == t.Name + "Id" || p.Name == "CreatedBy" || p.Name == "ModifiedBy")
                    continue;
                var sllst = p.GetCustomAttributes(typeof(StringLengthAttribute), true);
                if (sllst == null || sllst.Length == 0)
                    continue;
                var space = i == 0 ? "" : "                                ";
                sb.AppendLine(space + $"|| x.{p.Name}.Trim().ToUpper() == search.Keyword.Trim().ToUpper()");
                i++;
            }
            return sb.ToString();
        }

        private static string GetPropertyNameOfName(Type pType)
        {
            foreach (var p in pType.GetProperties())
            {
                if (p.Name == pType.Name + "Name")
                    return p.Name;
            }
            return null;
        }

        private static string GetViewAndModelVariable(Type t)
        {
            var sb = new StringBuilder();
            var i = 0;
            foreach (var p in t.GetProperties())
            {
                if (p.Name == t.Name + "Id" || p.Name == "CreatedBy" || p.Name == "ModifiedBy")
                    continue;
                var fkeyType = GetFkeyType(t.Name, p);
                if (fkeyType == null)
                    continue;

                var fkeyName = GetPropertyNameOfName(fkeyType);
                if (fkeyName == null)
                    continue;
                var space = i == 0 ? "" : "        ";
                const string space2 = "        ";
                sb.AppendLine(space + "[NotColumn]");
                sb.AppendLine(space2 + $"public string {fkeyName} {{ get; set;}}");
                i++;
            }
            return sb.ToString();
        }

        private static string GetSearchVariable(Type t)
        {
            var sb = new StringBuilder();
            var i = 0;
            foreach (var p in t.GetProperties())
            {
                if (p.Name == t.Name + "Id")
                {
                    continue;
                }

                if (p.Name.EndsWith("TypeId"))
                {
                    var fKeyType = GetFkeyType(t.Name, p);
                    if (fKeyType == null)
                        continue;

                    var space = i == 0 ? "" : "        ";
                    sb.AppendLine(space + $"public List<long> {p.Name}s {{ set; get;}}");
                }
                else if (p.Name.EndsWith("Id"))
                {
                    if (!(p.PropertyType == typeof(long) || p.PropertyType == typeof(long?)))
                        continue;
                    var space = i == 0 ? "" : "        ";
                    sb.AppendLine(space + $"public long? {p.Name} {{ set; get;}}");
                }
                else
                {
                    continue;
                }
                ++i;
            }
            return sb.ToString();
        }

        private static string GetRepoAssignment(Type t)
        {
            var sb = new StringBuilder();
            var i = 0;
            foreach (var p in t.GetProperties())
            {
                var joinSyb = ((char)('a' + i)).ToString();

                var fKeyType = GetFkeyType(t.Name, p);
                if (fKeyType == null)
                    continue;
                var fkeyName = GetPropertyNameOfName(fKeyType);
                if (fkeyName == null)
                    continue;

                var space = i == 0 ? "" : "                       ";
                sb.AppendLine(space + $"{fkeyName} = {joinSyb}.{fkeyName},");
                ++i;
            }

            sb.AppendLine($"");
            foreach (var p in t.GetProperties())
            {
                var space = i == 0 ? "" : "                       ";
                sb.AppendLine(space + $"{p.Name} = item.{p.Name},");
            }
            return sb.ToString();
        }

        private static string GetRepoAssignmentWhere(Type t)
        {
            var sb = new StringBuilder();
            var i = 0;
            foreach (var p in t.GetProperties())
            {
                if (p.Name == t.Name + "Id")
                {
                    continue;
                }

                if (p.Name.EndsWith("TypeId"))
                {

                    var fKeyType = GetFkeyType(t.Name, p);
                    if (fKeyType == null)
                        continue;
                    var space = i == 0 ? "" : "                ";
                    const string space2 = "                ";
                    sb.AppendLine(space + $"if (search.{p.Name}s != null && search.{p.Name}s.Count > 0)");
                    sb.AppendLine(space2 + $"    sql1 = sql1.Where(x => search.{p.Name}s.Contains(x.{p.Name}));");
                    ++i;
                }
                else if (p.Name.EndsWith("Id"))
                {
                    if (!(p.PropertyType == typeof(long) || p.PropertyType == typeof(long?)))
                        continue;

                    var space = i == 0 ? "" : "                ";
                    const string space2 = "                ";
                    sb.AppendLine(space + $"if (search.{p.Name} != null && search.{p.Name} > 0)");
                    sb.AppendLine(space2 + $"    sql1 = sql1.Where(x => search.{p.Name} == x.{p.Name});");
                    ++i;
                }
            }
            return sb.ToString();
        }


        private static string GetRepoJoin(string tableName, Type t)
        {
            var sb = new StringBuilder();
            var i = 0;
            foreach (var p in t.GetProperties())
            {
                var joinSyb = ((char)('a' + i)).ToString();
                var fKeyType = GetFkeyType(tableName, p);
                if (fKeyType == null)
                    continue;
                var space = i == 0 ? "" : "                   ";
                const string space2 = "                   ";
                if (p.PropertyType.IsNullable())
                {
                    sb.AppendLine(space + $"join o{joinSyb} in dbMgr.GetTable<{fKeyType.Name}>() on item.{fKeyType.Name}Id equals o{joinSyb}.{fKeyType.Name}Id into {joinSyb}Temp");
                    sb.AppendLine(space2 + $"from {joinSyb} in {joinSyb}Temp.DefaultIfEmpty()");
                }
                else
                {
                    sb.AppendLine(space + $"join {joinSyb} in dbMgr.GetTable<{fKeyType.Name}>() on item.{fKeyType.Name}Id equals {joinSyb}.{fKeyType.Name}Id");
                }

                i++;
            }
            return sb.ToString();
        }

        public static string GetRepoEditNameCheckExistsCheckVariable(Type t)
        {
            var p = GetPropertyNameOfName(t);
            if (p == null)
                return "\"\"";

            return $"eentity123Model.EEntity123Name";
        }

        public static string GetInputModelVariable(Type t)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"");
            var i = 0;
            foreach (var p in t.GetProperties())
            {
                if (!p.Name.EndsWith("Id") || t.Name + "Id" == p.Name)
                    continue;

                var space = i == 0 ? "" : "        ";
                var varName = p.Name.Substring(0, p.Name.Length - 2) + "Name";
                sb.AppendLine($"{space}private string {varName} {{get; set;}}");
                i++;
            }
            return sb.ToString();
        }

        public static string GetRepoEditNameCheck(Type t)
        {
            var p = GetPropertyNameOfName(t);
            if (p == null)
                return "return Task.FromResult(false);";

            return $"return dbMgr.ExistsAsync<EEntity123>(x => x.EEntity123Id != eentity123Id && x.EEntity123Name.ToLower().Trim() == eentity123Name.ToLower().Trim() && x.TenantId == tenantId);";
        }

        public static string GetRepoAddNameCheck(Type t)
        {
            var p = GetPropertyNameOfName(t);
            if (p == null)
                return "return Task.FromResult(false);";

            return "return dbMgr.ExistsAsync<EEntity123>(x => x.EEntity123Name.ToLower().Trim() == eentity123Name.ToLower().Trim() && x.TenantId == tenantId);";
        }

        private static Type GetFkeyType(string tableName, PropertyInfo pi)
        {

            if (tableName + "Id" == pi.Name)
                return null;

            if (!pi.Name.EndsWith("Id"))
                return null;

            var fkeyTableName = pi.Name.Substring(0, pi.Name.Length - 2);

            var fKeyType = GetTableType(fkeyTableName);

            return fKeyType;
        }

        public static Type GetTableType(string tableName)
        {
            if (cache.TryGetValue(tableName.Trim().ToLower(), out Type t))
                return t;
            return null;
        }

        public static string GetTemplateFileLocation(string templateFile)
        {
            var str = Directory.GetCurrentDirectory() + "\\CrudTemplate\\";
            if (!Directory.Exists(str))
                throw new Exception($"Template directory does not exists in {str}");
            str = str + templateFile;
            if (!File.Exists(str))
                throw new Exception($"Template File does not exists in {str}");
            return str;
        }

        public static string GetSourceFileLocation(string sourceFile, string folder = null)
        {
            var str = folder;
            if (folder == null)
                str = Directory.GetCurrentDirectory() + "\\..\\..\\..\\Backend\\Api\\All\\WIP\\";

            if (!Directory.Exists(str))
                throw new Exception($"Source directory does not exists in {str}");
            return str + sourceFile;

        }
    }
}

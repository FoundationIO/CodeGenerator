/**
Copyright (c) 2016 Foundation.IO (https://github.com/foundationio). All rights reserved.

This work is licensed under the terms of the BSD license.
For a copy, see <https://opensource.org/licenses/BSD-3-Clause>.
**/
namespace CodeGenerator
{
    using CodeGenerator.Models;
    using LogR.Api.All.Controllers;
    using LogR.Common.Enums;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Reflection;
    using System.Text;


    public static class ApiModelGeneratorProgram
    {
        public static void MainApp(bool isTest = false)
        {
            var isTS = false;
            var sb = new StringBuilder();

            var t = typeof(AccountController);

            var assembly = t.Assembly;
            if (assembly == null)
            {
                Console.WriteLine("Unabel to find the LogR.Common assembly");
                return;
            }

            var combined = CodeGenUtils.GetParsedStructure(assembly);

            //Process all the Models to make sure they are not missed when generating the TS Models
            CodeGenUtils.AddDependentModels(ref combined);


            sb.AppendLine(System.Text.Json.JsonSerializer.Serialize<CombinedClass>(combined, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            CodeGenUtils.CleanAndWriteAndOpenFile(Directory.GetCurrentDirectory() + "\\TS_Code_Structure.json", sb);


            sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations;");
            if (isTest) { sb.AppendLine("using System.Text.Json.Serialization;"); } else sb.AppendLine("using Newtonsoft.Json;");
            sb.AppendLine("using Framework.Infrastructure.Attributes;");
            if (isTest) { sb.AppendLine("namespace LogR.Api.IntegrationTests"); } else sb.AppendLine($"namespace LogR.WordAddIn.Shared.Models");
            sb.AppendLine("{");

            foreach (var cInfo in combined.TypescriptModelClassInfoList)
            {
                if (cInfo.CanIgnore)
                    continue;

                sb.AppendLine($"    public class {cInfo.Name}");
                sb.AppendLine($"    {{");

                foreach (var prop in cInfo.ModelType.GetProperties())
                {
                    var requiredStr = "";
                    var rlst = prop.GetCustomAttributes(typeof(RequiredAttribute), true);
                    if (!(rlst == null || rlst.Length == 0))
                    {
                        requiredStr = "[Required]";
                    }
                    if (isTest) { sb.AppendLine($"        {requiredStr}[JsonPropertyName(\"{CodeGenUtils.LowerFirstCharacter(prop.Name)}\")]"); } else sb.AppendLine($"        {requiredStr}[JsonProperty(\"{CodeGenUtils.LowerFirstCharacter(prop.Name)}\")]");
                    sb.AppendLine($"        public {CodeGenUtils.GetModelProperty(isTS, prop.PropertyType)} {prop.Name} {{set; get;}} = {CodeGenUtils.GetModelPropertyDefaultValue(isTS, prop.PropertyType)};");
                }

                sb.AppendLine($"        // Property Names");
                foreach (var prop in cInfo.ModelType.GetProperties())
                {
                    sb.AppendLine($"        public const string {prop.Name + "__Name"} = \"{prop.Name}\";");
                }

                var sSb = new List<string>();
                foreach (var prop in cInfo.ModelType.GetProperties())
                {
                    var slst = prop.GetCustomAttributes(typeof(StringLengthAttribute), true);
                    if (slst == null || slst.Length == 0)
                    {
                        continue;
                    }
                    sSb.Add($"        public const int {prop.Name + "__Size"} = {((StringLengthAttribute)slst[0]).MaximumLength};");
                }

                if (sSb.Count > 0)
                {
                    sb.AppendLine($"        // String Column Sizes");
                    foreach (var item in sSb)
                    {
                        sb.AppendLine(item);
                    }
                }

                sb.AppendLine($"    }}");
                sb.AppendLine($"");
            }

            sb.AppendLine("}");

            if (isTest)
                CodeGenUtils.CleanAndWriteAndOpenFile(GetTestModelDirectory() + "\\ApiModels.cs", sb);
            else
                CodeGenUtils.CleanAndWriteAndOpenFile(GetReactModelDirectory() + "\\ApiModels.cs", sb);

            var isb = new StringBuilder();
            isb.AppendLine("using System;");
            isb.AppendLine("using Framework.Infrastructure.Models.Result;");
            isb.AppendLine("using Framework.Infrastructure.Utils;");

            if (isTest) { isb.Append(""); } else isb.AppendLine("using LogR.WordAddIn.Shared.Models;");
            if (isTest) { isb.Append(""); } else isb.AppendLine("using LogR.WordAddIn.Shared.Services;");
            isb.AppendLine();
            if (isTest) { isb.AppendLine("namespace LogR.Api.IntegrationTests"); } else isb.AppendLine($"namespace LogR.WordAddIn.Shared.Interfaces");
            isb.AppendLine("{");
            isb.AppendLine($"    public interface IHttpService");
            isb.AppendLine($"    {{");

            sb = new StringBuilder();
            sb.AppendLine("using System;");
            if (isTest) { sb.AppendLine("using System.Net.Http;"); }
            sb.AppendLine("using Framework.Infrastructure.Models.Result;");
            sb.AppendLine("using Framework.Infrastructure.Utils;");
            if (isTest) { sb.AppendLine("using Framework.TestUtilies;"); }
            if (isTest) { sb.AppendLine("using LogR.Api.All;"); }
            if (isTest) { sb.Append(""); } else sb.AppendLine("using LogR.WordAddIn.Shared.Interfaces;");
            if (isTest) { sb.Append(""); } else sb.AppendLine("using LogR.WordAddIn.Shared.Models;");
            if (isTest) { sb.Append(""); } else sb.AppendLine("using LogR.WordAddIn.Shared.Services;");
            sb.AppendLine();
            if (isTest) { sb.AppendLine("namespace LogR.Api.IntegrationTests"); } else sb.AppendLine($"namespace LogR.WordAddIn.Shared.Services");
            sb.AppendLine("{");

            if (isTest) { sb.AppendLine("    public class HttpService : IDisposable"); } else sb.AppendLine($"    public class HttpService : IHttpService");
            sb.AppendLine($"    {{");
            if (isTest) { sb.AppendLine("        private readonly string site;"); } else sb.AppendLine($"        private readonly AppConfigService appConfigService;");
            if (isTest) { sb.AppendLine("        private readonly FrameworkTestEnvironment<Startup> env;"); }
            if (isTest) { sb.AppendLine("        private bool disposedValue;"); }

            if (isTest) { sb.AppendLine("        public HttpService(string site, FrameworkTestEnvironment<Startup> env)"); } else sb.AppendLine($"        public HttpService(AppConfigService appConfigService)");
            sb.AppendLine($"        {{");
            if (isTest) { sb.AppendLine("            this.site = site ?? \"\";"); } else sb.AppendLine($"            this.appConfigService = appConfigService;");
            if (isTest) { sb.AppendLine("            this.env = env;"); }
            sb.AppendLine($"        }}");
            sb.AppendLine("");
            if (isTest) { sb.AppendLine("        protected virtual void Dispose(bool disposing)"); }
            if (isTest) { sb.AppendLine("        {"); }
            if (isTest) { sb.AppendLine("            if (!disposedValue)"); }
            if (isTest) { sb.AppendLine("            {"); }
            if (isTest) { sb.AppendLine("                disposedValue = true;"); }
            if (isTest) { sb.AppendLine("            }"); }
            if (isTest) { sb.AppendLine("        }"); }
            if (isTest) { sb.AppendLine(""); }
            if (isTest) { sb.AppendLine("        public void Dispose()"); }
            if (isTest) { sb.AppendLine("        {"); }
            if (isTest) { sb.AppendLine("            Dispose(disposing: true);"); }
            if (isTest) { sb.AppendLine("            GC.SuppressFinalize(this);"); }
            if (isTest) { sb.AppendLine("        }"); }
            sb.AppendLine($"         public string Url(string fragment)");
            sb.AppendLine($"         {{");
            if (isTest) { sb.AppendLine("             return RestClientUtils.Url(site, fragment);"); } else sb.AppendLine($"             return RestClientUtils.Url(appConfigService.Site, fragment);");
            sb.AppendLine($"         }}");
            sb.AppendLine("");

            var tokenVar = "token: appConfigService.CurrentToken, headers : appConfigService.CurrentHeaders";
            if (isTest)
                tokenVar = "appHeader = appHeader";

            string lineValue;
            string iLineValue;

            foreach (var funcInfo in combined.FunctionInfoList)
            {

                foreach (var method in funcInfo.Methods)
                {
                    if (method.CanIgnore)
                        continue;
                    //var returnModel = "ReturnModel";
                    var listSuffix = "";
                    if (method.GetOutputParameterTypeWithModuleNamePrefix(isTS).Contains("ReturnListModel"))
                    {
                        listSuffix = "List";
                        //returnModel = "ReturnListModel";
                    }

                    var (modelName, outerClass) = CodeGenUtils.GetModelPrefixedClassNameInfo(isTS, method.OutputParameterType);
                    if (modelName == null)
                        modelName = "";

                    if (isTest)
                        lineValue = ($"        public virtual (HttpResponseMessage response, {method.GetOutputParameterTypeWithModuleNamePrefix(isTS)} result) {method.MethodName}({CodeGenUtils.GetTSFunctionParameter(isTS, method.InputParameters, isTest)})");
                    else
                        lineValue = ($"        public virtual {method.GetOutputParameterTypeWithModuleNamePrefix(isTS)} {method.MethodName}({CodeGenUtils.GetTSFunctionParameter(isTS, method.InputParameters, isTest)})");

                    sb.AppendLine(lineValue);
                    lineValue = ($"        {{");
                    sb.AppendLine(lineValue);

                    iLineValue = ($"        {method.GetOutputParameterTypeWithModuleNamePrefix(isTS)} {method.MethodName}({CodeGenUtils.GetTSFunctionParameter(isTS, method.InputParameters, isTest)});");
                    isb.AppendLine(iLineValue);

                    var space = "           ";
                    if (method.CallType == MethodCallType.Get)
                    {
                        if (method.InputParameters.Count == 0)
                        {
                            if (isTest)
                                lineValue = ($"{space}return env.GetAndReadAsJsonWithNewTokenClientAsync<Return{listSuffix}Model<{modelName}>,Object>(token,Url(\"/api/v1/{funcInfo.ApiClassName.ToLower()}/{method.ApiMethodName.ToLower()}\"),null,appHeader).Result;");
                            else
                                lineValue = ($"{space}return RestClientUtils.Get{listSuffix}<{modelName}>(Url(\"/api/v1/{funcInfo.ApiClassName.ToLower()}/{method.ApiMethodName.ToLower()}\"),{tokenVar});");
                            sb.AppendLine(lineValue);
                        }
                        else
                        {
                            var inputParamType = assembly.GetType(method.InputParameters[0].TypeFullName);

                            if (inputParamType == null)
                                inputParamType = Type.GetType(method.InputParameters[0].TypeFullName);

                            if (method.InputParameters[0].Key == "Nullable`1")
                            {
                                if (method.InputParameters[0].Value == "123~!@#")
                                    continue;
                            }

                            var qry = CodeGenUtils.GetTSFunctionQueryParametersAsString(isTS, method.InputParameters);
                            if (method.InputParameters.Count == 1)
                            {
                                var p = method.InputParameters[0].Value;
                                var s = "";
                                var k = method.InputParameters[0].Key;
                                if (!CodeGenUtils.IsBasicType(method.InputParameters[0].Key))
                                    s = "";
                                else
                                {
                                    p = "new { " + p + " }";
                                    k = "Object";
                                }

                                if (isTest)
                                    lineValue = ($"{space}return env.GetAndReadAsJsonWithNewTokenClientAsync<Return{listSuffix}Model<{modelName}>,{k}>(token,Url(\"/api/v1/{funcInfo.ApiClassName.ToLower()}/{method.ApiMethodName.ToLower()}\"),{p},appHeader).Result;");
                                else
                                    lineValue = ($"{space}return RestClientUtils.Get{listSuffix}<{modelName}{s}>(Url(\"/api/v1/{funcInfo.ApiClassName.ToLower()}/{method.ApiMethodName.ToLower()}\"),{p},{tokenVar});");
                                sb.AppendLine(lineValue);
                            }
                            else
                            {
                                if (isTest)
                                    lineValue = ($"{space}return env.GetAndReadAsJsonWithNewTokenClientAsync<Return{listSuffix}Model<{modelName}>,Object>(token,Url(\"/api/v1/{funcInfo.ApiClassName.ToLower()}/{method.ApiMethodName.ToLower()}\"),{qry},appHeader).Result;");
                                else
                                    lineValue = ($"{space}return RestClientUtils.Get{listSuffix}<{modelName}>(Url(\"/api/v1/{funcInfo.ApiClassName.ToLower()}/{method.ApiMethodName.ToLower()}\"),{qry},{tokenVar});");
                                sb.AppendLine(lineValue);
                            }
                        }
                    }
                    else if (method.CallType == MethodCallType.Post)
                    {
                        if (method.InputParameters.Count > 1)
                            throw new Exception("Post Not supported with multiple input parameter");
                        if (method.InputParameters.Count == 0)
                        {
                            if (isTest)
                                lineValue = ($"{space}return env.PostAndReadAsJsonWithNewTokenClientAsync<Return{listSuffix}Model<{modelName}>,Object>(token,Url(\"/api/v1/{funcInfo.ApiClassName.ToLower()}/{method.ApiMethodName.ToLower()}\"),null,appHeader).Result;");
                            else
                                lineValue = ($"{space}return RestClientUtils.Post{listSuffix}<{modelName}>(Url(\"/api/v1/{funcInfo.ApiClassName.ToLower()}/{method.ApiMethodName.ToLower()}\"),{tokenVar});");
                            sb.AppendLine(lineValue);
                        }
                        else
                        {
                            var p = method.InputParameters[0].Value;
                            var s = ",object";
                            var k = method.InputParameters[0].Key;

                            if (!CodeGenUtils.IsBasicType(method.InputParameters[0].Key))
                                s = "," + method.InputParameters[0].Key;
                            else
                            {
                                p = "new { " + p + " }";
                                k = "Object";
                            }
                            if (isTest)
                                lineValue = ($"{space}return env.PostAndReadAsJsonWithNewTokenClientAsync<Return{listSuffix}Model<{modelName}>,{k}>(token,Url(\"/api/v1/{funcInfo.ApiClassName.ToLower()}/{method.ApiMethodName.ToLower()}\"),{p},appHeader).Result;");
                            else
                                lineValue = ($"{space}return RestClientUtils.Post{listSuffix}WithModel<{modelName}{s}>(Url(\"/api/v1/{funcInfo.ApiClassName.ToLower()}/{method.ApiMethodName.ToLower()}\"),{p},{tokenVar});");
                            sb.AppendLine(lineValue);
                        }
                    }
                    else if (method.CallType == MethodCallType.Put)
                    {
                        if (method.InputParameters.Count > 1)
                        {
                                //CodeGenUtils.MethodParamsCount(method.InputParameters);
                                if(!CodeGenUtils.IsAllAppParamBasicDataType(method.InputParameters))
                                    throw new Exception("Post Not supported with multiple complex input parameter");
                        }

                        if (method.InputParameters.Count == 0)
                        {
                            if (isTest)
                                lineValue = ($"{space}return env.PutAndReadAsJsonWithNewTokenClientAsync<Return{listSuffix}Model<{modelName}>,Object>(token,Url(\"/api/v1/{funcInfo.ApiClassName.ToLower()}/{method.ApiMethodName.ToLower()}\"),null,appHeader).Result;");
                            else
                                lineValue = ($"{space}return RestClientUtils.Put{listSuffix}<{modelName}>(Url(\"/api/v1/{funcInfo.ApiClassName.ToLower()}/{method.ApiMethodName.ToLower()}\"),{tokenVar});");
                            sb.AppendLine(lineValue);
                        }
                        else
                        {
                            var p = "";
                            var s = "";
                            var k = "";
                            var pLst = new List<string>();
                            foreach (var ip in method.InputParameters)
                            {
                                if (!CodeGenUtils.IsBasicType(ip.Key))
                                {
                                    k = ip.Key;
                                    p = ip.Value;
                                    s = "," + ip.Key;
                                }
                                else
                                {
                                    if (method.InputParameters.Count == 1)
                                    {
                                        p = "new { " + ip.Value + " }";
                                    }
                                    else
                                    {
                                        pLst.Add(ip.Value);
                                    }
                                    k = "Object";
                                    s = ",object";
                                }
                            }
                            
                            if (method.InputParameters.Count  > 1)
                            {
                                p = "new { " + String.Join(",", pLst) + " }";
                            }

                            if (isTest)
                                lineValue = ($"{space}return env.PutAndReadAsJsonWithNewTokenClientAsync<Return{listSuffix}Model<{modelName}>,{k}>(token,Url(\"/api/v1/{funcInfo.ApiClassName.ToLower()}/{method.ApiMethodName.ToLower()}\"),{p},appHeader).Result;");
                            else
                                lineValue = ($"{space}return RestClientUtils.Put{listSuffix}WithModel<{modelName}{s}>(Url(\"/api/v1/{funcInfo.ApiClassName.ToLower()}/{method.ApiMethodName.ToLower()}\"),{p},{tokenVar});");
                            sb.AppendLine(lineValue);
                        }
                    }
                    else if (method.CallType == MethodCallType.Delete)
                    {
                        if (method.InputParameters.Count > 1)
                            throw new Exception("Delte Not supported with multiple input parameter");
                        if (method.InputParameters.Count == 0)
                        {
                            if (isTest)
                                lineValue = ($"{space}return env.DeleteAndReadAsJsonWithNewTokenClientAsync<Return{listSuffix}Model<{modelName}>>(token,Url(\"/api/v1/{funcInfo.ApiClassName.ToLower()}/{method.ApiMethodName.ToLower()}\"),appHeader).Result;");
                            else
                                lineValue = ($"{space}return RestClientUtils.Delete{listSuffix}<{modelName}>(Url(\"/api/v1/{funcInfo.ApiClassName.ToLower()}/{method.ApiMethodName.ToLower()}\"),{tokenVar});");

                            sb.AppendLine(lineValue);
                        }
                        else
                        {
                            var p = method.InputParameters[0].Value;
                            var s = "";
                            if (!CodeGenUtils.IsBasicType(method.InputParameters[0].Key))
                                s = "," + method.InputParameters[0].Key;
                            else
                                p = "new { " + p + " }";

                            if (isTest)
                                lineValue = ($"{space}return env.DeleteAndReadAsJsonWithNewTokenClientAsync<Return{listSuffix}Model<{modelName}>>(token,Url(\"/api/v1/{funcInfo.ApiClassName.ToLower()}/{method.ApiMethodName.ToLower()}\"),{p},appHeader).Result;");
                            else
                                lineValue = ($"{space}return RestClientUtils.Delete{listSuffix}<{modelName}{s}>(Url(\"/api/v1/{funcInfo.ApiClassName.ToLower()}/{method.ApiMethodName.ToLower()}\"),{p},{tokenVar});");
                            sb.AppendLine(lineValue);
                        }
                    }
                    lineValue = ($"        }}");
                    sb.AppendLine(lineValue);

                    lineValue = ($"");
                    sb.AppendLine(lineValue);
                }
            }

            lineValue = ($"    }}");
            sb.AppendLine(lineValue);
            lineValue = ($"}}");
            sb.AppendLine(lineValue);

            iLineValue = ($"    }}");
            isb.AppendLine(iLineValue);
            iLineValue = ($"}}");
            isb.AppendLine(iLineValue);

            if (isTest)
            {
                CodeGenUtils.CleanAndWriteAndOpenFile(GetTestHttpApiDirectory() + "\\HttpService.cs", sb);
            }
            else
            {
                CodeGenUtils.CleanAndWriteAndOpenFile(GetReactHttpApiDirectory() + "\\HttpService.cs", sb);
                CodeGenUtils.CleanAndWriteAndOpenFile(GetReactInterfaceDirectory() + "\\IHttpService.cs", isb);
            }
        }

        private static string GetTestHttpApiDirectory()
        {
            return Directory.GetCurrentDirectory() + "\\..\\..\\..\\Backend\\Tests\\IntegrationTests\\Api\\";
        }

        private static string GetTestModelDirectory()
        {
            return Directory.GetCurrentDirectory() + "\\..\\..\\..\\Backend\\Tests\\IntegrationTests\\Api\\";
        }

        private static string GetReactInterfaceDirectory()
        {
            return Directory.GetCurrentDirectory() + "\\..\\..\\..\\..\\..\\logr-app-word\\Source\\WordApp\\Shared\\Interfaces\\";
        }

        private static string GetReactHttpApiDirectory()
        {
            return Directory.GetCurrentDirectory() + "\\..\\..\\..\\..\\..\\logr-app-word\\Source\\WordApp\\Shared\\Services\\";
        }

        private static string GetReactModelDirectory()
        {
            return Directory.GetCurrentDirectory() + "\\..\\..\\..\\..\\..\\logr-app-word\\Source\\WordApp\\Shared\\Models\\";
        }



    }
}

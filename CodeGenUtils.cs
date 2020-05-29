/**
Copyright (c) 2016 Foundation.IO (https://github.com/foundationio). All rights reserved.

This work is licensed under the terms of the BSD license.
For a copy, see <https://opensource.org/licenses/BSD-3-Clause>.
**/
using CodeGenerator.Models;
using Framework.Infrastructure.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace CodeGenerator
{
    public static class CodeGenUtils
    {
        public static void CleanAndWriteAndOpenFile(string fileName, StringBuilder sb)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            File.AppendAllText(fileName, sb.ToString());
            Process.Start("notepad.exe", fileName);
        }


        public static CombinedClass GetParsedStructure(Assembly assembly)
        {
            var combined = new CombinedClass();

            foreach (var aClass in assembly.GetTypes())
            {
                if (!typeof(Microsoft.AspNetCore.Mvc.Controller).IsAssignableFrom(aClass))
                    continue;

                var cItem = new TClass { ClassName = aClass.Name };
                combined.FunctionInfoList.Add(cItem);

                var methodInfos = aClass.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public);

                if (methodInfos.Length == 0)
                    continue;

                foreach (var method in methodInfos)
                {
                    MethodCallType callType;

                    var methodParams = method.GetParameters();
                    var returnType = method.ReturnType;

                    if (method.GetCustomAttribute(typeof(HttpPostAttribute)) != null)
                    {
                        if (!DoesMethodParamsOneOrNoBody(methodParams))
                        {
                            throw new NotImplementedException($"This Post scenario is not implemented for {aClass.Name} - {method.Name}");
                        }

                        callType = MethodCallType.Post;
                    }
                    else if (method.GetCustomAttribute(typeof(HttpGetAttribute)) != null)
                    {
                        callType = MethodCallType.Get;
                        if (DoesMethodParamsAnyBody(methodParams))
                            throw new NotImplementedException($"This get scenario is not implemented for {aClass.Name} - {method.Name}");
                    }
                    else if (method.GetCustomAttribute(typeof(HttpPutAttribute)) != null)
                    {
                        if (!DoesMethodParamsOneOrNoBody(methodParams))
                        {
                            throw new NotImplementedException($"This put scenario is not implemented for {aClass.Name} - {method.Name}");
                        }
                        callType = MethodCallType.Put;
                    }
                    else if (method.GetCustomAttribute(typeof(HttpDeleteAttribute)) != null)
                    {
                        if (DoesMethodParamsAnyBody(methodParams))
                            throw new NotImplementedException($"This delete scenario is not implemented for {aClass.Name} - {method.Name}");
                        callType = MethodCallType.Delete;
                    }
                    else
                    {
                        throw new NotImplementedException("This scenario is not implemented");
                    }

                    var rName = returnType.Name;
                    var underlysingReturnType = returnType;

                    if (returnType.BaseType == typeof(System.Threading.Tasks.Task))
                    {
                        (rName, underlysingReturnType) = CodeGenUtils.ProcessGenericVariable(combined, returnType);
                    }
                    else
                    {
                        if (returnType.IsGenericType)
                            (rName, underlysingReturnType) = CodeGenUtils.ProcessGenericVariable(combined, returnType);
                    }

                    var mItem = new TMethod { MethodName = method.Name, CallType = callType, OutputParameterType = rName, CanIgnore = CodeGenUtils.CanIgnoreReturnType(underlysingReturnType) };
                    cItem.Methods.Add(mItem);

                    //var inputParam = "";
                    foreach (var param in methodParams)
                    {
                        if (!combined.TypescriptModelClassInfoList.Any(x => x.Name == param.ParameterType.Name))
                            combined.TypescriptModelClassInfoList.Add(new TypescriptModelClass { Name = param.ParameterType.Name, ModelType = param.ParameterType, CanIgnore = CanIgnoreReturnType(param.ParameterType) });

                        mItem.InputParameters.Add(new InputParam { Key = param.ParameterType.Name, Value = param.Name, TypeFullName = param.ParameterType.FullName, InputType =  GetInputType(param) });
                    }
                }
            }
            return combined;

        }

        private static InputTypes GetInputType(ParameterInfo param)
        {
            var lst = param.GetCustomAttributes(typeof(FromBodyAttribute));
            if (lst != null || lst.Count() > 0)
                return InputTypes.Body;

            lst = param.GetCustomAttributes(typeof(FromQueryAttribute));
            if (lst != null || lst.Count() > 0)
                return InputTypes.Body;

            throw new Exception($"Unable to find suitable Parameter type for {param.Name}");
        }

        public static (int QryCount, int BdyCount)  MethodParamsCount(ParameterInfo[] methodParams)
        {
            if (methodParams == null || methodParams.Length == 0)
            {
                return (0,0);
            }

            var bodyCount = 0;
            var queryCount = 0;

            foreach (var met in methodParams)
            {
                var lst = met.GetCustomAttributes(typeof(FromQueryAttribute));
                if (lst != null)
                    queryCount = lst.Count();

                lst = met.GetCustomAttributes(typeof(FromBodyAttribute));
                if (lst != null)
                    bodyCount = lst.Count();
            }

            return (queryCount ,bodyCount);
        }

        public static bool DoesMethodParamsAnyBody(ParameterInfo[] methodParams)
        {
            return (MethodParamsCount(methodParams).BdyCount > 0);
        }

        public static bool DoesMethodParamsOneOrNoBody(ParameterInfo[] methodParams)
        {
            if (MethodParamsCount(methodParams).BdyCount <= 1)
            {
                return true;
            }

            return false;
        }

        public static object GetModelPropertyDefaultValue(bool isTS, Type propertyType)
        {
            if (Nullable.GetUnderlyingType(propertyType) != null)
            {
                return GetTSTypeDefaultValue(isTS, propertyType.GetGenericArguments()[0].Name);
            }
            else
            {
                if (propertyType.IsGenericType)
                {
                    var genicClass = GetGenericClassName(propertyType.Name);
                    if (genicClass == "List" || genicClass == "Array" || genicClass == "IEnumerable")
                    {
                        if (isTS)
                            return "[]";
                        else
                            return "null";
                    }
                    else
                        return GetTSTypeDefaultValue(isTS, propertyType.GetGenericArguments()[0].Name);
                }
                else if (propertyType.IsArray)
                {
                    var pName = propertyType.Name.Substring(0, propertyType.Name.Length - 2);
                    if (pName.ToLower() == "byte")
                        return "null";

                    if (isTS)
                        return "[]";
                    else
                        return "null";
                }
                else
                {
                    return GetTSTypeDefaultValue(isTS, propertyType.Name);
                }
            }
        }

        public static void ProcessDependentModelSubProperty(Type modelType, ref CombinedClass combined, PropertyInfo prop, ref List<TypescriptModelClass> missingClasses)
        {
            Console.WriteLine($"Inspecting :  Model = {modelType.Name} , Prop Reflected - {prop.ReflectedType.FullName} , Prop Name - {prop.Name} , Prop Type - {prop.PropertyType.FullName}");

            if (modelType.Name == "WordClauseListModel")
            {
                if (prop.PropertyType.IsGenericType)
                {
                    if (prop.PropertyType.Name == "")
                    {
                        return;
                    }
                }
            }
            if (prop.PropertyType.IsGenericType)
            {
                if (prop.PropertyType.Name == "List`1")
                {
                    var innerType = prop.PropertyType.GetGenericArguments()[0];
                    if (innerType.Name == "")
                        return;
                }
            }
            /*
            if (modelType.Name == prop.PropertyType.Name)
            {
                return;
            }

            if (prop.PropertyType.IsGenericType)
            {
                var innerTypeName = GetGenericInnerType(prop.PropertyType.Name);
                if (innerTypeName == modelType.Name)
                    return;

                if (prop.PropertyType.GetGenericArguments().Count == 1)
                {
                    var innerType = prop.PropertyType.GetGenericArguments()[0];

                    if (IsBasicType(innerType.Name))
                    {
                        return;
                    }
                }
            }
            else
            {
                if (IsBasicType(prop.PropertyType.Name))
                {
                    return;
                }
            }
            */

            if (IsBasicType(prop.PropertyType.Name))
            {
                return;
            }

            if (missingClasses.Any(x => x.Name == prop.PropertyType.Name) || combined.TypescriptModelClassInfoList.Any(x => x.Name == prop.PropertyType.Name))
            {
                foreach (var subprop in prop.PropertyType.GetProperties())
                {
                    if (modelType.Name == subprop.PropertyType.Name)
                    {
                        continue;
                    }

                    if (subprop.PropertyType.IsGenericType && (subprop.PropertyType.GetGenericArguments()[0].Name == modelType.Name))
                    {
                        continue;
                    }

                    ProcessDependentModelSubProperty(modelType, ref combined, subprop, ref missingClasses);
                }
                return;
            }

            if (IsArrayType(prop.PropertyType) || IsNullable(prop.PropertyType) || prop.PropertyType.IsGenericType)
            {
                var actualPropName = "";
                Type actualPropType = null;
                if (prop.PropertyType.IsArray)
                {
                    if (!prop.PropertyType.Name.EndsWith("[]"))
                    {
                        throw new Exception($"Array type does not end with [] for property {prop.PropertyType.Name}");
                    }
                    actualPropName = prop.PropertyType.Name.Substring(0, prop.PropertyType.Name.Length - 2);
                    actualPropType = prop.PropertyType;
                }
                else
                {
                    var newPropType = prop.PropertyType.GetGenericArguments()[0];
                    actualPropName = newPropType.Name;
                    actualPropType = newPropType;
                }

                if (IsBasicType(actualPropName))
                {
                    return;
                }

                if (missingClasses.Any(x => x.Name == actualPropName) || combined.TypescriptModelClassInfoList.Any(x => x.Name == actualPropName))
                {
                    foreach (var subprop in actualPropType.GetProperties())
                    {
                        if (modelType.Name == subprop.PropertyType.Name)
                        {
                            continue;
                        }

                        if (subprop.PropertyType.IsGenericType && (subprop.PropertyType.GetGenericArguments()[0].Name == modelType.Name))
                        {
                            continue;
                        }

                        ProcessDependentModelSubProperty(modelType, ref combined, subprop, ref missingClasses);
                    }
                    return;
                }

                foreach (var subprop in actualPropType.GetProperties())
                {
                    if (modelType.Name == subprop.PropertyType.Name)
                    {
                        continue;
                    }

                    if (subprop.PropertyType.IsGenericType && (subprop.PropertyType.GetGenericArguments()[0].Name == modelType.Name))
                    {
                        continue;
                    }

                    ProcessDependentModelSubProperty(modelType, ref combined, subprop, ref missingClasses);
                }

                missingClasses.Add(new TypescriptModelClass { CanIgnore = false, Name = actualPropName, ModelType = actualPropType });
                return;
            }

            foreach (var subprop in prop.PropertyType.GetProperties())
            {
                if (modelType.Name == subprop.PropertyType.Name)
                {
                    continue;
                }

                if (subprop.PropertyType.IsGenericType && (subprop.PropertyType.GetGenericArguments()[0].Name == modelType.Name))
                {
                    continue;
                }

                ProcessDependentModelSubProperty(modelType, ref combined, subprop, ref missingClasses);
            }

            missingClasses.Add(new TypescriptModelClass { CanIgnore = false, Name = prop.PropertyType.Name, ModelType = prop.PropertyType });

        }


        public static void AddDependentModels(ref CombinedClass combined)
        {
            var missingClasses = new List<TypescriptModelClass>();
            foreach (var cls in combined.TypescriptModelClassInfoList)
            {
                foreach (var prop in cls.ModelType.GetProperties())
                {
                    if (prop.Name == "ProfileImage")
                    {
                        if (prop.PropertyType.Name == "")
                            continue;
                    }

                    Console.WriteLine($"Processing Property : {cls.ModelType.Name + "." + prop.Name}");
                    if (cls.ModelType.Name == prop.PropertyType.Name)
                    {
                        continue;
                    }

                    if (prop.PropertyType.IsGenericType && (prop.PropertyType.GetGenericArguments()[0].Name == cls.ModelType.Name))
                    {
                        continue;
                    }

                    ProcessDependentModelSubProperty(cls.ModelType, ref combined, prop, ref missingClasses);
                }
            }

            combined.TypescriptModelClassInfoList.AddRange(missingClasses);
        }

        public static bool IsArrayType(Type propertyType)
        {
            if (!(propertyType.IsGenericType || propertyType.IsArray))
                return false;

            if (IsNullable(propertyType))
                return false;

            var gen = GetGenericClassName(propertyType.Name).ToLower();
            if (gen.StartsWith("list`") || gen == "list" || gen == "array" || gen == "ienumerable" || gen.EndsWith("[]"))
            {
                return true;
            }

            throw new NotImplementedException();
        }

        public static (string, Type) ProcessGenericVariable(CombinedClass combined, Type returnType)
        {
            string rName;
            var args = returnType.GenericTypeArguments;
            if (args.Length != 1)
            {
                throw new NotImplementedException("This scenario is not implemented");
            }

            var innerType = "";
            rName = args[0].Name;

            if (args[0].IsGenericType)
            {
                var innerArg = args[0].GenericTypeArguments;
                if (innerArg.Length != 1)
                {
                    throw new NotImplementedException("This scenario is not implemented");
                }
                innerType = innerArg[0].Name ?? "";

                if (!combined.TypescriptModelClassInfoList.Any(x => x.Name == innerType))
                    combined.TypescriptModelClassInfoList.Add(new TypescriptModelClass { Name = innerType, ModelType = innerArg[0], CanIgnore = CanIgnoreReturnType(innerArg[0]) });

                rName = args[0].GetGenericTypeDefinition().Name;
                rName = GetGenericInnerType(rName);
            }
            else if (args[0].IsArray)
            {
                if (args[0].Name == "")
                    throw new Exception("This will never be caught");
            }

            if (!innerType.IsNullOrEmpty())
                rName = rName + "<" + innerType + ">";
            return (rName, args[0]);
        }

        public static string GetGenericInnerType(string str)
        {
            var iBacktick = str.IndexOf('`');
            if (iBacktick > 0)
            {
                str = str.Remove(iBacktick);
            }
            return str;
        }

        public static bool IsAllAppParamBasicDataType(List<InputParam> inputParameters)
        {
            foreach (var item in inputParameters)
            {
                if (!CodeGenUtils.IsBasicType(item.Key))
                    return false;
            }

            return true;
        }

        public static string GetGenericClassName(string str)
        {
            var iBacktick = str.IndexOf('`');
            if (iBacktick > 0)
            {
                str = str.Substring(0, iBacktick);
            }
            return str;
        }

        public static string GetTSType(bool isTS, string csTypeName)
        {
            if (!isTS)
                return csTypeName;

            var lowerTypeName = csTypeName.ToLower();
            if (lowerTypeName == "string" || lowerTypeName == "guid" || lowerTypeName == "char")
                return "string";
            else if (lowerTypeName == "int" || lowerTypeName == "long" || lowerTypeName == "int32" || lowerTypeName == "int64" || lowerTypeName == "float" || lowerTypeName == "double" || lowerTypeName == "decimal")
                return "number";
            if (lowerTypeName == "bool" || lowerTypeName == "boolean")
                return "boolean";
            if (lowerTypeName == "datetime")
                return "Date";
            if (lowerTypeName == "byte")
                return "string";
            return csTypeName;
        }

        public static string GetTSTypeDefaultValue(bool isTS, string csTypeName)
        {
            if (!isTS)
                return "default(" + csTypeName + ")";

            var lowerTypeName = csTypeName.ToLower();
            if (lowerTypeName == "string" || lowerTypeName == "guid" || lowerTypeName == "char" || lowerTypeName == "byte")
                return "\'\'";
            else if (lowerTypeName == "int" || lowerTypeName == "long" || lowerTypeName == "int32" || lowerTypeName == "int64" || lowerTypeName == "float" || lowerTypeName == "double" || lowerTypeName == "decimal")
                return "0";
            if (lowerTypeName == "bool" || lowerTypeName == "boolean")
                return "false";
            if (lowerTypeName == "datetime")
                return "new Date()";
            return "new " + csTypeName + "()";
        }


        public static string GetTSFunctionQueryParametersAsString(bool isTS, List<InputParam> inputParameters)
        {
            var stLst = new List<string>();
            foreach (var item in inputParameters)
            {
                if (isTS)
                    stLst.Add($"{item.Value} : {item.Value}");
                else
                    stLst.Add($"{item.Value}");
            }
            var p = "{" + string.Join(",", stLst) + "}";
            if (isTS)
                return p;
            else
                return "new " + p;
        }

        public static object GetTSFunctionParameter(bool isTS, List<InputParam> tsParameters, bool isTest = false)
        {
            var stLst = new List<string>();
            foreach (var item in tsParameters)
            {
                if (isTS)
                    stLst.Add($"{item.Value}:{ModelPrefixedClassName(isTS, GetTSType(isTS, item.Key))}");
                else
                    stLst.Add($"{ModelPrefixedClassName(isTS, GetTSType(isTS, item.Key))} {item.Value}");
            }

            if (!isTS)
            {
                if (isTest)
                {
                    stLst.Add("string token = null");
                    stLst.Add("AppHeaderModel appHeader = null");
                }
            }

            return string.Join(", ", stLst);
        }

        public static bool CanIgnoreReturnType(Type returnType)
        {
            if (returnType.IsValueType)
                return true;
            if (returnType == typeof(string))
                return true;
            if (returnType == typeof(String))
                return true;
            if (returnType == typeof(ActionResult))
                return true;

            if (returnType == typeof(ActionResult<>))
                return true;

            if (returnType == typeof(HttpResponseMessage))
                return true;

            if (returnType.IsAssignableFrom(typeof(IActionResult)))
                return true;

            return false;
        }

        public static string GetModelProperty(bool isTS, Type propertyType)
        {
            if (Nullable.GetUnderlyingType(propertyType) != null)
            {
                var r = GetTSType(isTS, propertyType.GetGenericArguments()[0].Name);
                if (isTS)
                    return r + " | null";
                else
                    return r + "?";
            }
            else
            {
                if (propertyType.IsGenericType)
                {
                    var genicClass = GetGenericClassName(propertyType.Name);
                    if (genicClass == "List" || genicClass == "Array" || genicClass == "IEnumerable")
                    {
                        var r = GetTSType(isTS, propertyType.GetGenericArguments()[0].Name);
                        if (isTS)
                            return r + "[]";
                        else
                            return "List<" + r + ">";
                    }
                    else
                        return GetTSType(isTS, propertyType.GetGenericArguments()[0].Name);
                }
                else if (propertyType.IsArray)
                {
                    var pName = propertyType.Name.Substring(0, propertyType.Name.Length - 2);
                    if (pName.ToLower() == "byte")
                        return "string";
                    var r = GetTSType(isTS, propertyType.GetGenericArguments()[0].Name);

                    if (isTS)
                        return r + "[]";
                    else
                        return "List<" + r + ">";
                }
                else
                {
                    return GetTSType(isTS, propertyType.Name);
                }
            }
        }

        public static bool IsNullable(Type propertyType)
        {
            return (Nullable.GetUnderlyingType(propertyType) != null);
        }

        public static bool IsBasicType(string name)
        {
            name = name.ToLower();
            return (name == "short" || name == "byte" || name == "boolean" || name == "timespan" || name == "datetime" || name == "string" || name == "char" || name == "guid" || name == "long" || name == "int32" || name == "int64" || name == "int" || name == "int16" || name == "decimal" || name == "double" || name == "float" || name == "short" || name == "boolean" || name == "bool");
        }

        public static bool IsTSBasicType(string name)
        {
            name = name.ToLower();
            return (name == "date" || name == "number" || name == "string" || name == "boolean" || name == "boolean");
        }

        public static string LowerFirstCharacter(string str)
        {
            if (str == null || str.Trim() == "")
                return str;
            return str[0].ToString().ToLower() + str.Substring(1);
        }


        public static (string, string) GetModelPrefixedClassNameInfo(bool isTS, string pType)
        {
            if (pType == null || pType.Trim() == "")
                return (pType, null);

            var modelName = pType;
            var outerClassName = "";
            if (pType.StartsWith("ReturnModel<"))
            {
                pType = pType.Substring("ReturnModel<".Length);
                modelName = pType.Substring(0, pType.Length - 1);
                outerClassName = "ReturnModel";
            }
            else if (pType.StartsWith("ReturnListModel<"))
            {
                pType = pType.Substring("ReturnListModel<".Length);
                modelName = pType.Substring(0, pType.Length - 1);
                outerClassName = "ReturnListModel";
            }

            if (!IsBasicType(modelName))
            {
                if (isTS)
                {
                    if (!IsTSBasicType(modelName))
                        modelName = "models." + modelName;
                }
            }

            //if (outerClassName != "")
            //    modelName = outerClassName + "<" + modelName + ">";

            return (modelName, outerClassName);
        }

        public static string ModelPrefixedClassName(bool isTS, string pType)
        {
            var (modelName, outerClass) = GetModelPrefixedClassNameInfo(isTS, pType);
            if (modelName == null)
                return null;

            if (outerClass != "")
                modelName = outerClass + "<" + modelName + ">";

            return modelName;
        }

        public static bool IsEnumAvailable(string enumName)
        {
            var t = Type.GetType(enumName);
            if (t == null)
                return false;
            return t.IsEnum;
        }
    }

    public enum InputTypes
    {
        Body,
        Query
    }
}

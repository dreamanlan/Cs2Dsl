﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using Dsl;

namespace Generator
{
    internal static partial class LuaGenerator
    {
        private static void GenerateLua(Dsl.DslFile dslFile, string outputFile, string fileName)
        {
            var calculator = InitCalculator();

            var myselfNewRequire = GetMergedFile(fileName);
            FileMergeInfo mergedInfo = null;
            if (!string.IsNullOrEmpty(myselfNewRequire)) {
                s_FileMergeInfos.TryGetValue(myselfNewRequire, out mergedInfo);
            }

            StringBuilder sb = new StringBuilder();
            string entryClass = string.Empty;
            HashSet<string> requires = new HashSet<string>();
            List<string> requireList = new List<string>();
            Stack<string> classDefineStack = new Stack<string>();
            string prestr = string.Empty;
            int indent = 0;
            bool firstRequire = true;
            bool firstAttrs = true;
            foreach (var dslInfo in dslFile.DslInfos) {
                s_CurSyntax = dslInfo;
                string id = dslInfo.GetId();
                Dsl.FunctionData funcData = dslInfo as Dsl.FunctionData;
                Dsl.FunctionData callData = funcData;
                if (null != funcData && funcData.IsHighOrder) {
                    callData = funcData.LowerOrderFunction;
                }
                if (id == "require") {
                    string requireFileName = callData.GetParamId(0).Replace("cs2dsl__", "cs2lua__");
                    string srcPath = Path.Combine(s_ExePath, string.Format("lualib/{0}.lua", requireFileName));
                    string destPath = Path.Combine(s_OutPath, string.Format("{0}.{1}", requireFileName, s_Ext));
                    bool srcExist = File.Exists(srcPath);
                    bool destExist = File.Exists(destPath);
                    if (!destExist && srcExist) {
                        File.Copy(srcPath, destPath, true);
                    }
                    if ((srcExist || requireFileName.StartsWith("cs2lua__")) && null != mergedInfo && mergedInfo.CodeBuilder.Length > 0) {
                        //合并的文件，只有第一个文件require基础库
                    }
                    else {
                        if (firstRequire) {
                            sb.AppendLine("require \"cs2lua__namespaces\"; ");
                            firstRequire = false;
                        }
                        if (requireFileName != "cs2lua__namespaces" && !DontRequire(fileName, requireFileName)) {
                            var newRequire = GetMergedFile(requireFileName);
                            if (null != newRequire) {
                                if (newRequire != myselfNewRequire && !requires.Contains(newRequire)) {
                                    requires.Add(newRequire);
                                    requireList.Add(newRequire);
                                }
                            }
                            else if (!requires.Contains(requireFileName)) {
                                requires.Add(requireFileName);
                                requireList.Add(requireFileName);
                            }
                        }
                    }
                }
                else if (id == "defineentry") {
                    entryClass = CalcTypeString(callData.GetParam(0));
                }
                else if (id == "attributes") {
                    if (firstAttrs) {
                        sb.AppendLine("__cs2lua__AllAttrs = {};");
                        firstAttrs = false;
                    }
                    string className = CalcTypeString(callData.GetParam(0));
                    sb.AppendFormatLine("{0}__cs2lua__AllAttrs[\"{1}\"] = {{", GetIndentString(indent), className);
                    ++indent;
                    var classAttr = FindStatement(funcData, "class") as Dsl.FunctionData;
                    if (null != classAttr) {
                        sb.AppendFormatLine("{0}Class = {{", GetIndentString(indent));
                        ++indent;
                        foreach (var comp in classAttr.Params) {
                            GenerateAttribute(comp, sb, indent);
                        }
                        --indent;
                        sb.AppendFormatLine("{0}}};", GetIndentString(indent));
                    }
                    foreach (var mattr in funcData.Params) {
                        if (mattr.GetId() == "class")
                            continue;
                        var fd = mattr as Dsl.FunctionData;
                        if (null != fd) {
                            var fcd = fd;
                            if (fd.IsHighOrder)
                                fcd = fd.LowerOrderFunction;
                            var mid = fcd.GetParamId(0);
                            sb.AppendFormatLine("{0}{1} = {{", GetIndentString(indent), mid);
                            ++indent;
                            foreach (var comp in fd.Params) {
                                GenerateAttribute(comp, sb, indent);
                            }
                            --indent;
                            sb.AppendFormatLine("{0}}};", GetIndentString(indent));
                        }
                    }
                    --indent;
                    sb.AppendFormatLine("{0}}};", GetIndentString(indent));
                }
                else if (id == "namespace") {
                    string ns = CalcTypeString(callData.GetParam(0));
                    sb.AppendFormatLine("{0}{1} = {1} or {{}};", GetIndentString(indent), ns, ns);
                }
                else if (id == "interface") {
                    string intfName = CalcTypeString(callData.GetParam(0));
                    sb.AppendFormatLine("{0}{1} = {{", GetIndentString(indent), intfName);
                    ++indent;
                    sb.AppendFormatLine("{0}__cs2lua_defined = true, ", GetIndentString(indent));
                    sb.AppendFormatLine("{0}__cs2lua_fullname = \"{1}\", ", GetIndentString(indent), intfName);
                    sb.AppendFormatLine("{0}__cs2lua_typename = \"{1}\", ", GetIndentString(indent), GetLastName(intfName));
                    sb.AppendFormatLine("{0}__interfaces = {{", GetIndentString(indent));
                    ++indent;
                    var intfs = FindStatement(funcData, "interfaces") as Dsl.FunctionData;
                    if (null != intfs) {
                        foreach (var comp in intfs.Params) {
                            sb.AppendFormatLine("{0}\"{1}\"", prestr, comp.GetId());
                            prestr = ", ";
                        }
                    }
                    --indent;
                    sb.AppendFormatLine("{0}}}, ", GetIndentString(indent));
                    sb.AppendFormatLine("{0}__exist = function(k) return false; end", GetIndentString(indent));
                    --indent;
                    sb.AppendFormatLine("{0}}};", GetIndentString(indent));
                }
                else if (id == "defineentry") {
                    string className = CalcTypeString(callData.GetParam(0));
                    sb.AppendFormatLine("{0}defineentry({1});", GetIndentString(indent), className);
                }
                else if (id == "enum") {
                    string fullName = CalcTypeString(callData.GetParam(0));
                    var baseClass = callData.GetParam(1);
                    string baseClassName = string.Empty;
                    if (null != baseClass) {
                        baseClassName = CalcTypeString(baseClass);
                    }

                    sb.AppendLine();

                    int dotIx = fullName.LastIndexOf('.');
                    if (dotIx > 0) {
                        string ns = fullName.Substring(0, dotIx);
                        string className = fullName.Substring(dotIx + 1);

                        sb.AppendFormatLine("{0}rawset({1}, \"{2}\", {{", GetIndentString(indent), ns, className);
                        ++indent;
                        foreach (var comp in funcData.Params) {
                            var cd = comp as Dsl.FunctionData;
                            if (null != cd) {
                                sb.AppendFormatLine("{0}[\"{1}\"] = {2},", GetIndentString(indent), cd.GetParamId(0), cd.GetParamId(1));
                            }
                        }
                        --indent;
                        sb.AppendFormatLine("{0}}});", GetIndentString(indent));
                    }
                    else {
                        sb.AppendFormatLine("{0}{1} = {{", GetIndentString(indent), fullName);
                        ++indent;
                        foreach (var comp in funcData.Params) {
                            var cd = comp as Dsl.FunctionData;
                            if (null != cd) {
                                sb.AppendFormatLine("{0}[\"{1}\"] = {2},", GetIndentString(indent), cd.GetParamId(0), cd.GetParamId(1));
                            }
                        }
                        --indent;
                        sb.AppendFormatLine("{0}}};", GetIndentString(indent));
                    }

                    sb.AppendLine();

                    sb.AppendFormatLine("{0}rawset({1}, \"Value2String\", {{", GetIndentString(indent), fullName);
                    ++indent;
                    foreach (var comp in funcData.Params) {
                        var cd = comp as Dsl.FunctionData;
                        if (null != cd) {
                            sb.AppendFormatLine("{0}[{1}] = \"{2}\",", GetIndentString(indent), cd.GetParamId(1), cd.GetParamId(0));
                        }
                    }
                    --indent;
                    sb.AppendFormatLine("{0}}});", GetIndentString(indent));
                }
                else if (id == "class" || id == "struct") {
                    string className = CalcTypeString(callData.GetParam(0));
                    var baseClass = callData.GetParam(1);
                    string baseClassName = string.Empty;
                    if (null != baseClass) {
                        baseClassName = CalcTypeString(baseClass);
                    }
                    bool isValueType = id == "struct";

                    sb.AppendLine();

                    sb.AppendFormatLine("{0}{1} = {{", GetIndentString(indent), className);
                    ++indent;

                    var staticMethods = FindStatement(funcData, "static_methods") as Dsl.FunctionData;
                    if (null != staticMethods && staticMethods.GetParamNum() > 0) {
                        sb.AppendFormatLine("{0}-------------------------------", GetIndentString(indent));
                        sb.AppendFormatLine("{0}-------- class methods --------", GetIndentString(indent));
                        sb.AppendFormatLine("{0}-------------------------------", GetIndentString(indent));
                        bool firstMethod = true;
                        foreach (var def in staticMethods.Params) {
                            if (!firstMethod)
                                sb.AppendLine();
                            firstMethod = false;

                            var mdef = def as Dsl.FunctionData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                var mtemp = mdef.GetParam(1);
                                var cdef = mtemp as Dsl.FunctionData;
                                var fdef = mtemp as Dsl.StatementData;
                                if (null != cdef) {
                                    fdef = cdef.GetParam(0) as Dsl.StatementData;
                                }
                                if (null != fdef && fdef.GetFunctionNum() >= 2) {
                                    var first = fdef.First;
                                    var second = fdef.Second;
                                    var funcOpts = new FunctionOptions();
                                    if (fdef.GetFunctionNum() >= 3) {
                                        var opts = fdef.Third;
                                        ParseFunctionOptions(opts, funcOpts);
                                    }
                                    int rct;
                                    int.TryParse(first.GetParamId(0), out rct);
                                    if (second.HaveStatement()) {
                                        var fcall = second;
                                        if (second.IsHighOrder)
                                            fcall = second.LowerOrderFunction;
                                        sb.AppendFormat("{0}{1} = ", GetIndentString(indent), mname);
                                        if (null != cdef) {
                                            sb.Append("wrapenumerable(");
                                        }
                                        sb.Append("function(");
                                        bool haveParams = GenerateFunctionParams(fcall, sb);
                                        sb.AppendLine(")");
                                        ++indent;
                                        var logInfo = GetPrologueAndEpilogue(className, mname);
                                        if (null != logInfo.PrologueInfo) {
                                            sb.AppendFormatLine("{0}{1};", GetIndentString(indent), CalcLogInfo(logInfo.PrologueInfo, className, mname));
                                        }
                                        if (null != logInfo.EpilogueInfo) {
                                            if (rct > 0) {
                                                sb.AppendFormat("{0}local ", GetIndentString(indent));
                                                GenerateFunctionRetVars(second, sb, rct, "__retval_");
                                                sb.AppendFormat(" = {0}.__real_{1}(", className, mname);
                                                GenerateFunctionParams(fcall, sb);
                                                sb.AppendLine(");");
                                                sb.AppendFormatLine("{0}{1};", GetIndentString(indent), CalcLogInfo(logInfo.EpilogueInfo, className, mname));
                                                sb.AppendFormat("{0}return ", GetIndentString(indent));
                                                GenerateFunctionRetVars(second, sb, rct, "__retval_");
                                                sb.AppendLine(";");
                                            }
                                            else {
                                                sb.AppendFormat("{0}{1}.__real_{2}(", GetIndentString(indent), className, mname);
                                                GenerateFunctionParams(fcall, sb);
                                                sb.AppendLine(");");
                                                sb.AppendFormatLine("{0}{1};", GetIndentString(indent), CalcLogInfo(logInfo.EpilogueInfo, className, mname));
                                            }
                                            --indent;
                                            if (null != cdef)
                                                sb.AppendFormatLine("{0}end),", GetIndentString(indent));
                                            else
                                                sb.AppendFormatLine("{0}end,", GetIndentString(indent));
                                            sb.AppendFormat("{0}__real_{1} = ", GetIndentString(indent), mname);
                                            sb.Append("function(");
                                            GenerateFunctionParams(fcall, sb);
                                            sb.AppendLine(")");
                                            ++indent;
                                        }
                                        if (IsParamTypeCheckMethod(className, mname)) {
                                            foreach (var p in fcall.Params) {
                                                var pname = p.GetId();
                                                TypeInfo ti;
                                                if (funcOpts.ParamTypes.TryGetValue(pname, out ti)) {
                                                    if (ti.TypeKind != "TypeKind.TypeParameter") {
                                                        sb.AppendFormatLine("{0}{1} = paramtypecheck({2}, {3});", GetIndentString(indent), pname, pname, ti.Type);
                                                    }
                                                }
                                            }
                                        }
                                        if (funcOpts.NeedFuncInfo) {
                                            sb.AppendFormatLine("{0}local __cs2lua_func_info = luainitialize();", GetIndentString(indent));
                                        }
                                        GenerateStatements(second, sb, indent, funcOpts, calculator);
                                        bool lastIsNotReturn = true;
                                        int snum = second.GetParamNum();
                                        for (; snum > 0; --snum) {
                                            var cid = second.GetParamId(snum - 1);
                                            if (cid != "comment" && cid != "comments")
                                                break;
                                        }
                                        if (snum > 0) {
                                            lastIsNotReturn = second.GetParamId(snum - 1) != "return";
                                        }
                                        if (rct <= 0 && lastIsNotReturn) {
                                            if (funcOpts.NeedFuncInfo) {
                                                sb.AppendFormatLine("{0}__cs2lua_func_info = luafinalize(__cs2lua_func_info);", GetIndentString(indent));
                                            }
                                        }
                                        --indent;
                                        if (null != cdef && null == logInfo.EpilogueInfo)
                                            sb.AppendFormatLine("{0}end),", GetIndentString(indent));
                                        else
                                            sb.AppendFormatLine("{0}end,", GetIndentString(indent));
                                    }
                                }
                            }
                        }
                        sb.AppendLine();
                    }

                    var staticFields = FindStatement(funcData, "static_fields") as Dsl.FunctionData;
                    if (null != staticFields && staticFields.GetParamNum() > 0) {
                        sb.AppendFormatLine("{0}-------------------------------", GetIndentString(indent));
                        sb.AppendFormatLine("{0}-------- class fields --------", GetIndentString(indent));
                        sb.AppendFormatLine("{0}-------------------------------", GetIndentString(indent));
                        var funcOpts = new FunctionOptions();
                        foreach (var def in staticFields.Params) {
                            var mdef = def as Dsl.FunctionData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                var comp = mdef.GetParam(1);
                                if (comp.GetId() != "null") {
                                    sb.AppendFormat("{0}{1} = ", GetIndentString(indent), mname);
                                    GenerateFieldValueComponent(comp, sb, indent, false, funcOpts, calculator);
                                    sb.AppendLine(",");
                                }
                            }
                        }
                        sb.AppendLine();
                    }

                    sb.AppendFormatLine("{0}-------------------------------", GetIndentString(indent));
                    sb.AppendFormatLine("{0}--- define class and object ---", GetIndentString(indent));
                    sb.AppendFormatLine("{0}-------------------------------", GetIndentString(indent));
                    var logInfoForDefineClass = GetPrologueAndEpilogue(className, "__define_class");
                    sb.AppendFormatLine("{0}__define_class = function()", GetIndentString(indent));
                    ++indent;
                    if (null != logInfoForDefineClass.PrologueInfo) {
                        sb.AppendFormatLine("{0}{1};", GetIndentString(indent), CalcLogInfo(logInfoForDefineClass.PrologueInfo, className, "__define_class"));
                    }
                    sb.AppendLine();

                    sb.AppendFormatLine("{0}local class = {1};", GetIndentString(indent), className);

                    sb.AppendLine();

                    var instMethods = FindStatement(funcData, "instance_methods") as Dsl.FunctionData;
                    if (null != instMethods && instMethods.GetParamNum() > 0) {
                        sb.AppendFormatLine("{0}local obj_methods = {{", GetIndentString(indent));
                        ++indent;
                        bool firstMethod = true;
                        foreach (var def in instMethods.Params) {
                            if (!firstMethod)
                                sb.AppendLine();
                            firstMethod = false;

                            var mdef = def as Dsl.FunctionData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                var mtemp = mdef.GetParam(1);
                                var cdef = mtemp as Dsl.FunctionData;
                                var fdef = mtemp as Dsl.StatementData;
                                if (null != cdef) {
                                    fdef = cdef.GetParam(0) as Dsl.StatementData;
                                }
                                if (null != fdef && fdef.GetFunctionNum() >= 2) {
                                    var first = fdef.First;
                                    var second = fdef.Second;
                                    var funcOpts = new FunctionOptions();
                                    if (fdef.GetFunctionNum() >= 3) {
                                        var opts = fdef.Third;
                                        ParseFunctionOptions(opts, funcOpts);
                                    }
                                    int rct;
                                    int.TryParse(first.GetParamId(0), out rct);
                                    if (second.HaveStatement()) {
                                        var fcall = second;
                                        if (second.IsHighOrder)
                                            fcall = second.LowerOrderFunction;
                                        sb.AppendFormat("{0}{1} = ", GetIndentString(indent), mname);
                                        if (null != cdef) {
                                            sb.Append("wrapenumerable(");
                                        }
                                        sb.Append("function(");
                                        bool haveParams = GenerateFunctionParams(fcall, sb);
                                        sb.AppendLine(")");
                                        ++indent;
                                        var logInfo = GetPrologueAndEpilogue(className, mname);
                                        if (null != logInfo.PrologueInfo) {
                                            sb.AppendFormatLine("{0}{1};", GetIndentString(indent), CalcLogInfo(logInfo.PrologueInfo, className, mname));
                                        }
                                        if (null != logInfo.EpilogueInfo) {
                                            if (rct > 0) {
                                                sb.AppendFormat("{0}local ", GetIndentString(indent));
                                                GenerateFunctionRetVars(second, sb, rct, "__retval_");
                                                sb.AppendFormat(" = {0}.__real_{1}(", "this", mname);
                                                GenerateFunctionParams(fcall, sb);
                                                sb.AppendLine(");");
                                                sb.AppendFormatLine("{0}{1};", GetIndentString(indent), CalcLogInfo(logInfo.EpilogueInfo, className, mname));
                                                sb.AppendFormat("{0}return ", GetIndentString(indent));
                                                GenerateFunctionRetVars(second, sb, rct, "__retval_");
                                                sb.AppendLine(";");
                                            }
                                            else {
                                                sb.AppendFormat("{0}{1}.__real_{2}(", GetIndentString(indent), "this", mname);
                                                GenerateFunctionParams(fcall, sb);
                                                sb.AppendLine(");");
                                                sb.AppendFormatLine("{0}{1};", GetIndentString(indent), CalcLogInfo(logInfo.EpilogueInfo, className, mname));
                                            }
                                            --indent;
                                            if (null != cdef)
                                                sb.AppendFormatLine("{0}end),", GetIndentString(indent));
                                            else
                                                sb.AppendFormatLine("{0}end,", GetIndentString(indent));
                                            sb.AppendFormat("{0}__real_{1} = ", GetIndentString(indent), mname);
                                            sb.Append("function(");
                                            GenerateFunctionParams(fcall, sb);
                                            sb.AppendLine(")");
                                            ++indent;
                                        }
                                        if (IsParamTypeCheckMethod(className, mname)) {
                                            foreach (var p in fcall.Params) {
                                                var pname = p.GetId();
                                                TypeInfo ti;
                                                if (funcOpts.ParamTypes.TryGetValue(pname, out ti)) {
                                                    if (ti.TypeKind != "TypeKind.TypeParameter") {
                                                        sb.AppendFormatLine("{0}{1} = paramtypecheck({2}, {3});", GetIndentString(indent), pname, pname, ti.Type);
                                                    }
                                                }
                                            }
                                        }
                                        if (funcOpts.NeedFuncInfo) {
                                            sb.AppendFormatLine("{0}local __cs2lua_func_info = luainitialize();", GetIndentString(indent));
                                        }
                                        GenerateStatements(second, sb, indent, funcOpts, calculator);
                                        bool lastIsNotReturn = true;
                                        int snum = second.GetParamNum();
                                        for (; snum > 0; --snum) {
                                            var cid = second.GetParamId(snum - 1);
                                            if (cid != "comment" && cid != "comments")
                                                break;
                                        }
                                        if (snum > 0) {
                                            lastIsNotReturn = second.GetParamId(snum - 1) != "return";
                                        }
                                        if (rct <= 0 && lastIsNotReturn) {
                                            if (funcOpts.NeedFuncInfo) {
                                                sb.AppendFormatLine("{0}__cs2lua_func_info = luafinalize(__cs2lua_func_info);", GetIndentString(indent));
                                            }
                                        }
                                        --indent;
                                        if (null != cdef && null == logInfo.EpilogueInfo)
                                            sb.AppendFormatLine("{0}end),", GetIndentString(indent));
                                        else
                                            sb.AppendFormatLine("{0}end,", GetIndentString(indent));
                                    }
                                }
                            }
                        }
                        --indent;
                        sb.AppendFormatLine("{0}}};", GetIndentString(indent));
                    }
                    else {
                        sb.AppendFormatLine("{0}local obj_methods = nil;", GetIndentString(indent));
                    }

                    sb.AppendLine();

                    sb.AppendFormatLine("{0}local obj_build = function()", GetIndentString(indent));
                    ++indent;
                    var instFields = FindStatement(funcData, "instance_fields") as Dsl.FunctionData;
                    if (null != instFields && instFields.GetParamNum() > 0) {
                        sb.AppendFormatLine("{0}return {{", GetIndentString(indent));
                        ++indent;

                        var funcOpts = new FunctionOptions();
                        foreach (var def in instFields.Params) {
                            var mdef = def as Dsl.FunctionData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                var comp = mdef.GetParam(1);
                                if (comp.GetId() != "null") {
                                    sb.AppendFormat("{0}{1} = ", GetIndentString(indent), mname);
                                    GenerateFieldValueComponent(comp, sb, indent, false, funcOpts, calculator);
                                    sb.AppendLine(",");
                                }
                            }
                        }

                        --indent;
                        sb.AppendFormatLine("{0}}};", GetIndentString(indent));
                    }
                    else {
                        sb.AppendFormatLine("{0}return nil;", GetIndentString(indent));
                    }
                    --indent;
                    sb.AppendFormatLine("{0}end;", GetIndentString(indent));

                    sb.AppendLine();
                    ///备忘：
                    ///这个对象模型分为class、obj_methods, obj_build三块
                    ///1、class表包含了静态方法与静态字段
                    ///2、obj_methods表独立有2个作用，一个方法放到obj上可能占内存会比较多，另外为了支持方法换名以支持虚函数机制
                    ///，参见lualib的defineclass实现。
                    ///3、obj_build是个函数，用来生成每个对象实例，已经包含了对应的字段表与初始值（因为每个实例都需要一份不同的数
                    ///据，所以是一个返回字段表的函数）
                    ///4、考虑继承机制需要在当前类不存在成员时往父类查找，而字段值可能是nil，此时相当于类实例里删除了这个字段，为
                    ///了仍然能正确查到当前类是否存在字段，在类信息里额外保存了2个字段信息表，一个静态的，一个实例的。
                    if (null != logInfoForDefineClass.EpilogueInfo) {
                        sb.AppendFormatLine("{0}local __defineclass_return = defineclass({1}, \"{2}\", \"{3}\", class, obj_methods, obj_build, {4});", GetIndentString(indent), null == baseClass || !baseClass.IsValid() ? "nil" : baseClassName, className, GetLastName(className), isValueType ? "true" : "false");
                        sb.AppendFormatLine("{0}{1};", GetIndentString(indent), CalcLogInfo(logInfoForDefineClass.EpilogueInfo, className, "__define_class"));
                        sb.AppendFormatLine("{0}return __defineclass_return;", GetIndentString(indent));
                    }
                    else {
                        sb.AppendFormatLine("{0}return defineclass({1}, \"{2}\", \"{3}\", class, obj_methods, obj_build, {4});", GetIndentString(indent), null == baseClass || !baseClass.IsValid() ? "nil" : baseClassName, className, GetLastName(className), isValueType ? "true" : "false");
                    }
                    --indent;
                    sb.AppendFormatLine("{0}end,", GetIndentString(indent));

                    sb.AppendLine();

                    sb.AppendFormatLine("{0}-------------------------------", GetIndentString(indent));
                    sb.AppendFormatLine("{0}--------- fields info ---------", GetIndentString(indent));
                    sb.AppendFormatLine("{0}-------------------------------", GetIndentString(indent));
                    if (null != staticFields && staticFields.GetParamNum() > 0) {
                        sb.AppendFormatLine("{0}__class_fields = {{", GetIndentString(indent));
                        ++indent;
                        foreach (var def in staticFields.Params) {
                            var mdef = def as Dsl.FunctionData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                sb.AppendFormatLine("{0}{1} = true,", GetIndentString(indent), mname);
                            }
                        }
                        --indent;
                        sb.AppendFormatLine("{0}}},", GetIndentString(indent));
                    }

                    sb.AppendLine();

                    if (null != instFields && instFields.GetParamNum() > 0) {
                        sb.AppendFormatLine("{0}__obj_fields = {{", GetIndentString(indent));
                        ++indent;
                        foreach (var def in instFields.Params) {
                            var mdef = def as Dsl.FunctionData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                sb.AppendFormatLine("{0}{1} = true,", GetIndentString(indent), mname);
                            }
                        }
                        --indent;
                        sb.AppendFormatLine("{0}}},", GetIndentString(indent));
                    }

                    sb.AppendLine();

                    sb.AppendFormatLine("{0}-------------------------------", GetIndentString(indent));
                    sb.AppendFormatLine("{0}-------- metadata info --------", GetIndentString(indent));
                    sb.AppendFormatLine("{0}-------------------------------", GetIndentString(indent));
                    var interfaces = FindStatement(funcData, "interfaces") as Dsl.FunctionData;
                    if (null != interfaces && interfaces.GetParamNum() > 0) {
                        sb.AppendFormatLine("{0}__interfaces = {{", GetIndentString(indent));
                        ++indent;
                        foreach (var def in interfaces.Params) {
                            var mdef = def as Dsl.ValueData;
                            sb.AppendFormatLine("{0}\"{1}\",", GetIndentString(indent), mdef.GetId());
                        }
                        --indent;
                        sb.AppendFormatLine("{0}}},", GetIndentString(indent));
                    }

                    bool sealedClass = false;
                    bool staticClass = false;
                    var classInfo = FindStatement(funcData, "class_info") as Dsl.FunctionData;
                    if (null != classInfo) {
                        foreach (var def in classInfo.Params) {
                            var mdef = def as Dsl.FunctionData;
                            string key = mdef.GetId();
                            string val = mdef.GetParamId(0);
                            bool bv;
                            bool.TryParse(val, out bv);
                            if (key == "sealed") {
                                sealedClass = true;
                                break;
                            }
                            else if (key == "static") {
                                staticClass = true;
                                break;
                            }
                        }
                    }
                    if (s_GenClassInfo && null != classInfo) {
                        var fcall = classInfo;
                        if (classInfo.IsHighOrder)
                            fcall = classInfo.LowerOrderFunction;
                        sb.AppendFormatLine("{0}__class_info = {{", GetIndentString(indent));
                        ++indent;
                        var kind = CalcTypeString(fcall.GetParam(0));
                        var accessibility = CalcTypeString(fcall.GetParam(1));
                        sb.AppendFormatLine("{0}Kind = {1},", GetIndentString(indent), kind);
                        if (accessibility == "Accessibility.Private") {
                            sb.AppendFormatLine("{0}private = true,", GetIndentString(indent));
                        }
                        foreach (var def in classInfo.Params) {
                            var mdef = def as Dsl.FunctionData;
                            string key = mdef.GetId();
                            string val = mdef.GetParamId(0);
                            sb.AppendFormatLine("{0}{1} = {2},", GetIndentString(indent), key, val);
                        }
                        --indent;
                        sb.AppendFormatLine("{0}}},", GetIndentString(indent));
                    }

                    var methodInfo = FindStatement(funcData, "method_info") as Dsl.FunctionData;
                    if ((!sealedClass && !staticClass || s_GenMethodInfo) && null != methodInfo && methodInfo.GetParamNum() > 0) {
                        sb.AppendFormatLine("{0}__method_info = {{", GetIndentString(indent));
                        ++indent;
                        foreach (var def in methodInfo.Params) {
                            var mfunc = def as Dsl.FunctionData;
                            if (null != mfunc) {
                                var fcall = mfunc;
                                if (mfunc.IsHighOrder)
                                    fcall = mfunc.LowerOrderFunction;
                                bool gen = s_GenMethodInfo;
                                if (!gen) {
                                    if (mfunc.GetId().StartsWith("ctor")) {
                                        gen = true;
                                    }
                                    else {
                                        var accessibility = CalcTypeString(fcall.GetParam(1));
                                        if (accessibility != "Accessibility.Private") {
                                            gen = true;
                                        }
                                        foreach (var minfo in mfunc.Params) {
                                            var mdef = minfo as Dsl.FunctionData;
                                            string key = mdef.GetId();
                                            string val = mdef.GetParamId(0);
                                            bool bv;
                                            bool.TryParse(val, out bv);
                                            if (key == "sealed") {
                                                gen = false;
                                                break;
                                            }
                                            else if (key == "abstract" || key == "virtual" || key == "override") {
                                                gen = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (gen) {
                                    sb.AppendFormatLine("{0}{1} = {{", GetIndentString(indent), mfunc.GetId());
                                    ++indent;
                                    var kind = CalcTypeString(fcall.GetParam(0));
                                    var accessibility = CalcTypeString(fcall.GetParam(1));
                                    sb.AppendFormatLine("{0}Kind = {1},", GetIndentString(indent), kind);
                                    if (accessibility == "Accessibility.Private") {
                                        sb.AppendFormatLine("{0}private = true,", GetIndentString(indent));
                                    }
                                    foreach (var minfo in mfunc.Params) {
                                        var mdef = minfo as Dsl.FunctionData;
                                        string key = mdef.GetId();
                                        string val = mdef.GetParamId(0);
                                        sb.AppendFormatLine("{0}{1} = {2},", GetIndentString(indent), key, val);
                                    }
                                    --indent;
                                    sb.AppendFormatLine("{0}}},", GetIndentString(indent));
                                }
                            }
                        }
                        --indent;
                        sb.AppendFormatLine("{0}}},", GetIndentString(indent));
                    }

                    var propertyInfo = FindStatement(funcData, "property_info") as Dsl.FunctionData;
                    if (s_GenPropertyInfo && null != propertyInfo && propertyInfo.GetParamNum() > 0) {
                        sb.AppendFormatLine("{0}__property_info = {{", GetIndentString(indent));
                        ++indent;
                        foreach (var def in propertyInfo.Params) {
                            var mfunc = def as Dsl.FunctionData;
                            if (null != mfunc) {
                                var fcall = mfunc;
                                if (mfunc.IsHighOrder)
                                    fcall = mfunc.LowerOrderFunction;
                                sb.AppendFormatLine("{0}{1} = {{", GetIndentString(indent), mfunc.GetId());
                                ++indent;
                                var accessibility = CalcTypeString(fcall.GetParam(0));
                                if (accessibility == "Accessibility.Private") {
                                    sb.AppendFormatLine("{0}private = true,", GetIndentString(indent));
                                }
                                foreach (var minfo in mfunc.Params) {
                                    var mdef = minfo as Dsl.FunctionData;
                                    string key = mdef.GetId();
                                    string val = mdef.GetParamId(0);
                                    sb.AppendFormatLine("{0}{1} = {2},", GetIndentString(indent), key, val);
                                }
                                --indent;
                                sb.AppendFormatLine("{0}}},", GetIndentString(indent));
                            }
                        }
                        --indent;
                        sb.AppendFormatLine("{0}}},", GetIndentString(indent));
                    }

                    var eventInfo = FindStatement(funcData, "event_info") as Dsl.FunctionData;
                    if (s_GenEventInfo && null != eventInfo && eventInfo.GetParamNum() > 0) {
                        sb.AppendFormatLine("{0}__event_info = {{", GetIndentString(indent));
                        ++indent;
                        foreach (var def in eventInfo.Params) {
                            var mfunc = def as Dsl.FunctionData;
                            if (null != mfunc) {
                                var fcall = mfunc;
                                if (mfunc.IsHighOrder)
                                    fcall = mfunc.LowerOrderFunction;
                                sb.AppendFormatLine("{0}{1} = {{", GetIndentString(indent), mfunc.GetId());
                                ++indent;
                                var accessibility = CalcTypeString(fcall.GetParam(0));
                                if (accessibility == "Accessibility.Private") {
                                    sb.AppendFormatLine("{0}private = true,", GetIndentString(indent));
                                }
                                foreach (var minfo in mfunc.Params) {
                                    var mdef = minfo as Dsl.FunctionData;
                                    string key = mdef.GetId();
                                    string val = mdef.GetParamId(0);
                                    sb.AppendFormatLine("{0}{1} = {2},", GetIndentString(indent), key, val);
                                }
                                --indent;
                                sb.AppendFormatLine("{0}}},", GetIndentString(indent));
                            }
                        }
                        --indent;
                        sb.AppendFormatLine("{0}}},", GetIndentString(indent));
                    }

                    var fieldInfo = FindStatement(funcData, "field_info") as Dsl.FunctionData;
                    if (s_GenFieldInfo && null != fieldInfo && fieldInfo.GetParamNum() > 0) {
                        sb.AppendFormatLine("{0}__field_info = {{", GetIndentString(indent));
                        ++indent;
                        foreach (var def in fieldInfo.Params) {
                            var mfunc = def as Dsl.FunctionData;
                            if (null != mfunc) {
                                var fcall = mfunc;
                                if (mfunc.IsHighOrder)
                                    fcall = mfunc.LowerOrderFunction;
                                sb.AppendFormatLine("{0}{1} = {{", GetIndentString(indent), mfunc.GetId());
                                ++indent;
                                var accessibility = CalcTypeString(fcall.GetParam(0));
                                foreach (var minfo in mfunc.Params) {
                                    var mdef = minfo as Dsl.FunctionData;
                                    string key = mdef.GetId();
                                    string val = mdef.GetParamId(0);
                                    sb.AppendFormatLine("{0}{1} = {2},", GetIndentString(indent), key, val);
                                }
                                --indent;
                                sb.AppendFormatLine("{0}}},", GetIndentString(indent));
                            }
                        }
                        --indent;
                        sb.AppendFormatLine("{0}}},", GetIndentString(indent));
                    }

                    --indent;
                    sb.AppendFormatLine("{0}}};", GetIndentString(indent));

                    classDefineStack.Push(className);
                }
            }
            sb.AppendLine();
            if (null != mergedInfo) {
                lock (mergedInfo) {
                    foreach (string req in requireList) {
                        if (!mergedInfo.Requires.Contains(req)) {
                            mergedInfo.Requires.Add(req);
                            mergedInfo.RequireList.Add(req);
                        }
                    }
                    mergedInfo.CodeBuilder.AppendLine(sb.ToString());
                    while (classDefineStack.Count > 0) {
                        var className = classDefineStack.Pop();
                        mergedInfo.DefinedClasses.Add(className);
                    }
                    if (!string.IsNullOrEmpty(entryClass)) {
                        mergedInfo.EntryClass = entryClass;
                    }
                }
            }
            else {
                StringBuilder newSb = new StringBuilder();
                newSb.AppendLine(sb.ToString());
                foreach (string req in requireList) {
                    newSb.AppendFormatLine("{0}require \"{1}\";", GetIndentString(indent), req);
                }
                while (classDefineStack.Count > 0) {
                    var className = classDefineStack.Pop();
                    newSb.AppendFormatLine("{0}settempmetatable({1});", GetIndentString(indent), className);
                }
                if (!string.IsNullOrEmpty(entryClass)) {
                    newSb.AppendFormatLine("{0}defineentry({1});", GetIndentString(indent), entryClass);
                }
                File.WriteAllText(outputFile, newSb.ToString());
            }
        }
        private static DslExpression.DslCalculator InitCalculator()
        {
            var scpfile = Path.Combine(s_ExePath, "generator.dsl");
            DslExpression.DslCalculator calculator = new DslExpression.DslCalculator();
            calculator.OnLog = (string s) => {
                Console.WriteLine(s);
                Log(scpfile, s);
            };
            calculator.Init();
            calculator.Register("getargument", new DslExpression.ExpressionFactoryHelper<DslExpression.GetArgmentExp>());
            calculator.Register("writeindent", new DslExpression.ExpressionFactoryHelper<DslExpression.WriteIndentExp>());
            calculator.Register("writesymbol", new DslExpression.ExpressionFactoryHelper<DslExpression.WriteSymbolExp>());
            calculator.Register("writestring", new DslExpression.ExpressionFactoryHelper<DslExpression.WriteStringExp>());
            calculator.Register("writearguments", new DslExpression.ExpressionFactoryHelper<DslExpression.WriteArgumentsExp>());
            calculator.Register("usefunc", new DslExpression.ExpressionFactoryHelper<DslExpression.UseFunctionExp>());
            calculator.LoadDsl(scpfile);
            return calculator;
        }
        private static bool CallDslHook(DslExpression.DslCalculator calculator, string id, Dsl.FunctionData data, FunctionOptions funcOpts, StringBuilder sb, int indent)
        {
            bool ret = false;
            var args = calculator.NewCalculatorValueList();
            args.Add(DslExpression.CalculatorValue.FromObject(data));
            args.Add(DslExpression.CalculatorValue.FromObject(funcOpts));
            args.Add(DslExpression.CalculatorValue.FromObject(sb));
            args.Add(indent);
            var r = calculator.Calc(id, args);
            calculator.RecycleCalculatorValueList(args);
            if (!r.IsNullObject) {
                ret = r;
            }
            return ret;
        }
        private static bool GenerateFunctionParams(Dsl.FunctionData fcall, StringBuilder sb)
        {
            bool haveParams = false;
            string prestr = string.Empty;
            for (int ix = 0; ix < fcall.Params.Count; ++ix) {
                var param = fcall.Params[ix];
                sb.Append(prestr);
                prestr = ", ";
                var pv = param as Dsl.ValueData;
                if (null != pv) {
                    var pname = pv.GetId();
                    if (ix == fcall.Params.Count - 1 && pname == "...")
                        haveParams = true;
                    sb.Append(pname);
                }
                else {
                    var pc = param as Dsl.FunctionData;
                    if (null != pc) {
                        var pname = pc.GetParamId(0);
                        if (ix == fcall.Params.Count - 1 && pname == "...")
                            haveParams = true;
                        sb.Append(pname);
                    }
                }
            }
            return haveParams;
        }
        private static void GenerateFunctionRetVars(Dsl.FunctionData fdef, StringBuilder sb, int ct, string prefix)
        {
            for (int i = 0; i < ct; ++i) {
                if (i > 0)
                    sb.Append(", ");
                sb.AppendFormat("{0}{1}", prefix, i + 1);
            }
        }
        private static void GenerateFieldValueComponent(Dsl.ISyntaxComponent comp, StringBuilder sb, int indent, bool firstLineUseIndent, FunctionOptions funcOpts, DslExpression.DslCalculator calculator)
        {
            s_CurSyntax = comp;
            var valData = comp as Dsl.ValueData;
            if (null != valData) {
                GenerateConcreteSyntax(valData, sb, indent, firstLineUseIndent, funcOpts);
            }
            else {
                var funcData = comp as Dsl.FunctionData;
                if (null != funcData) {
                    GenerateConcreteSyntax(funcData, sb, indent, firstLineUseIndent, funcOpts, calculator);
                }
                else {
                    var statementData = comp as Dsl.StatementData;
                    if (null != statementData) {
                        GenerateConcreteSyntax(statementData, sb, indent, firstLineUseIndent, funcOpts, calculator);
                    }
                    else {
                        System.Diagnostics.Debugger.Break();
                        throw new Exception(string.Format("GenerateFieldValueComponent ISyntaxComponent exception:{0}", sb.ToString()));
                    }
                }
            }
        }
        private static void GenerateStatement(Dsl.ISyntaxComponent comp, StringBuilder sb, int indent, FunctionOptions funcOpts, DslExpression.DslCalculator calculator)
        {
            var func = comp as Dsl.FunctionData;
            if (null != func && !func.HaveStatement()) {
                Dsl.FunctionData oper;
                bool prefix;
                if (CanSplitPrefixPostfixOperator(comp, out oper, out prefix)) {
                    sb.AppendFormat("{0}", GetIndentString(indent));
                    GeneratePrefixPostfixOperator(oper, sb, true, funcOpts, calculator);
                    sb.AppendLine(";");
                    var p = oper.GetParam(0) as Dsl.ValueData;
                    p.SetId("false");
                    GenerateSyntaxComponent(comp, sb, indent, true, funcOpts, calculator);
                }
                else {
                    GenerateSyntaxComponent(comp, sb, indent, true, funcOpts, calculator);
                }
            }
            else {
                GenerateSyntaxComponent(comp, sb, indent, true, funcOpts, calculator);
            }
        }
        private static void GenerateSyntaxComponent(Dsl.ISyntaxComponent comp, StringBuilder sb, int indent, bool firstLineUseIndent, FunctionOptions funcOpts, DslExpression.DslCalculator calculator)
        {
            s_CurSyntax = comp;
            var valData = comp as Dsl.ValueData;
            if (null != valData) {
                GenerateConcreteSyntax(valData, sb, indent, firstLineUseIndent, funcOpts);
            }
            else {
                var funcData = comp as Dsl.FunctionData;
                if (null != funcData) {
                    GenerateConcreteSyntax(funcData, sb, indent, firstLineUseIndent, funcOpts, calculator);
                }
                else {
                    var statementData = comp as Dsl.StatementData;
                    GenerateConcreteSyntax(statementData, sb, indent, firstLineUseIndent, funcOpts, calculator);
                }
            }
        }
        private static void GenerateConcreteSyntax(Dsl.ValueData data, StringBuilder sb, int indent, bool firstLineUseIndent, FunctionOptions funcOpts)
        {
            s_CurSyntax = data;
            if (firstLineUseIndent) {
                sb.AppendFormat("{0}", GetIndentString(indent));
            }
            string id = data.GetId();
            switch (data.GetIdType()) {
                case (int)Dsl.ValueData.ID_TOKEN:
                case (int)Dsl.ValueData.NUM_TOKEN:
                    if (id == "null") {
                        id = "nil";
                    }
                    else if (id == "__cs2dsl_out") {
                        id = "__cs2lua_out";
                    }
                    sb.Append(id);
                    break;
                case (int)Dsl.ValueData.STRING_TOKEN:
                    sb.AppendFormat("\"{0}\"", Escape(id));
                    break;
            }
        }
        private static void GenerateConcreteSyntaxForCall(Dsl.FunctionData data, StringBuilder sb, int indent, bool firstLineUseIndent, FunctionOptions funcOpts, DslExpression.DslCalculator calculator)
        {
            //这个方法生成函数名与参数表部分，需要注意的是data可能是函数名+函数体，在这个方法里不应生成函数体部分
            s_CurSyntax = data;
            string id = string.Empty;
            if (data.IsHighOrder) {
                GenerateConcreteSyntaxForCall(data.LowerOrderFunction, sb, indent, firstLineUseIndent, funcOpts, calculator);
            }
            else {
                id = data.GetId();
            }
            if (data.GetParamClass() == (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_OPERATOR) {
                id = ConvertOperator(id);
                int paramNum = data.GetParamNum();
                if (paramNum == 1) {
                    if (firstLineUseIndent) {
                        sb.AppendFormat("{0}", GetIndentString(indent));
                    }
                    var param1 = data.GetParam(0);
                    sb.AppendFormat("({0} ", id);
                    GenerateSyntaxComponent(param1, sb, indent, false, funcOpts, calculator);
                    sb.Append(")");
                }
                else if (paramNum == 2) {
                    var param1 = data.GetParam(0);
                    var param2 = data.GetParam(1);
                    bool handled = false;
                    var fd = param1 as Dsl.FunctionData;
                    var sd = param1 as Dsl.StatementData;
                    var fd2 = param2 as Dsl.FunctionData;
                    var sd2 = param2 as Dsl.StatementData;
                    string leftParamId = string.Empty;
                    string rightParamId = string.Empty;
                    if (null != fd && !fd.IsHighOrder && fd.HaveParam()) {
                        leftParamId = fd.GetId();
                    }
                    else if (null != sd) {
                        leftParamId = sd.GetId();
                    }
                    if (null != fd2 && !fd2.IsHighOrder && fd2.HaveParam()) {
                        rightParamId = fd2.GetId();
                    }
                    else if (null != sd2) {
                        rightParamId = sd2.GetId();
                    }
                    if (id == "=" && leftParamId == "multiassign") {
                        var cd = param1 as Dsl.FunctionData;
                        var _sd = param1 as Dsl.StatementData;
                        Dsl.FunctionData preCodeBlock = null;
                        Dsl.FunctionData postCodeBlock = null;
                        if (null != _sd) {
                            //带有前置与后置代码块的情形（代码块内容可能为空）
                            var codeBlocks = _sd.First;
                            preCodeBlock = codeBlocks.GetParam(0) as Dsl.FunctionData;
                            postCodeBlock = codeBlocks.GetParam(1) as Dsl.FunctionData;
                            cd = _sd.Second;
                        }
                        if (null != preCodeBlock) {
                            GenerateStatements(preCodeBlock, sb, indent, funcOpts, calculator);
                        }
                        if (null != cd) {
                            if (firstLineUseIndent) {
                                sb.AppendFormat("{0}", GetIndentString(indent));
                            }
                            if (cd.GetParamNum() > 1) {
                                int varNum = cd.GetParamNum();
                                for (int i = 0; i < varNum; ++i) {
                                    var parami = cd.GetParam(i);
                                    GenerateSyntaxComponent(parami, sb, indent, false, funcOpts, calculator);
                                    if (i < varNum - 1) {
                                        sb.Append(",");
                                    }
                                }
                                sb.AppendFormat(" {0} ", id);
                                GenerateSyntaxComponent(param2, sb, indent, false, funcOpts, calculator);
                            }
                            else {
                                GenerateSyntaxComponent(cd.GetParam(0), sb, indent, false, funcOpts, calculator);
                                sb.AppendFormat(" {0} ", id);
                                GenerateSyntaxComponent(param2, sb, indent, false, funcOpts, calculator);
                            }
                            handled = true;
                        }
                        if (null != postCodeBlock) {
                            if (postCodeBlock.GetParamNum() > 0) {
                                sb.AppendLine(";");
                            }
                            bool first = true;
                            foreach (var comp in postCodeBlock.Params) {
                                if (first)
                                    first = false;
                                else
                                    sb.AppendLine(";");
                                GenerateStatement(comp, sb, indent, funcOpts, calculator);
                            }
                        }
                    }
                    else if (id == "=" && (leftParamId == "getstatic"
                         || leftParamId == "getexternstatic"
                         || leftParamId == "getinstance"
                         || leftParamId == "getexterninstance")) {
                        var cd = param1 as Dsl.FunctionData;
                        if (null != cd.Name) {
                            cd.Name.SetId("s" + leftParamId.Substring(1));
                            cd.AddParam(param2);
                            GenerateConcreteSyntax(cd, sb, indent, firstLineUseIndent, funcOpts, calculator);
                            handled = true;
                        }
                    }
                    else if (id == "=" && leftParamId == "getstaticindexer") {
                        var cd = param1 as Dsl.FunctionData;
                        if (null != cd.Name) {
                            var member = cd.GetParam(1) as Dsl.ValueData;
                            var pct = cd.GetParam(2) as Dsl.ValueData;
                            if (null != member && null != pct) {
                                member.SetId("s" + member.GetId().Substring(1));
                                int ct;
                                int.TryParse(pct.GetId(), out ct);
                                pct.SetId((ct + 1).ToString());

                                cd.Name.SetId("s" + leftParamId.Substring(1));
                                cd.AddParam(param2);
                                GenerateConcreteSyntax(cd, sb, indent, firstLineUseIndent, funcOpts, calculator);
                                handled = true;
                            }
                        }
                    }
                    else if (id == "=" && leftParamId == "getinstanceindexer") {
                        var cd = param1 as Dsl.FunctionData;
                        if (null != cd.Name) {
                            var member = cd.GetParam(3) as Dsl.ValueData;
                            var pct = cd.GetParam(4) as Dsl.ValueData;
                            if (null != member && null != pct) {
                                member.SetId("s" + member.GetId().Substring(1));
                                int ct;
                                int.TryParse(pct.GetId(), out ct);
                                pct.SetId((ct + 1).ToString());

                                cd.Name.SetId("s" + leftParamId.Substring(1));
                                cd.AddParam(param2);
                                GenerateConcreteSyntax(cd, sb, indent, firstLineUseIndent, funcOpts, calculator);
                                handled = true;
                            }
                        }
                    }
                    else if (id == "=" && leftParamId == "getexternstaticindexer") {
                        var cd = param1 as Dsl.FunctionData;
                        if (null != cd.Name) {
                            var member = cd.GetParam(4) as Dsl.ValueData;
                            var pct = cd.GetParam(5) as Dsl.ValueData;
                            if (null != member && null != pct) {
                                member.SetId("s" + member.GetId().Substring(1));
                                int ct;
                                int.TryParse(pct.GetId(), out ct);
                                pct.SetId((ct + 1).ToString());

                                cd.Name.SetId("s" + leftParamId.Substring(1));
                                cd.AddParam(param2);
                                GenerateConcreteSyntax(cd, sb, indent, firstLineUseIndent, funcOpts, calculator);
                                handled = true;
                            }
                        }
                    }
                    else if (id == "=" && leftParamId == "getexterninstanceindexer") {
                        var cd = param1 as Dsl.FunctionData;
                        if (null != cd.Name) {
                            var member = cd.GetParam(6) as Dsl.ValueData;
                            var pct = cd.GetParam(7) as Dsl.ValueData;
                            if (null != member && null != pct) {
                                member.SetId("s" + member.GetId().Substring(1));
                                int ct;
                                int.TryParse(pct.GetId(), out ct);
                                pct.SetId((ct + 1).ToString());

                                cd.Name.SetId("s" + leftParamId.Substring(1));
                                cd.AddParam(param2);
                                GenerateConcreteSyntax(cd, sb, indent, firstLineUseIndent, funcOpts, calculator);
                                handled = true;
                            }
                        }
                    }
                    else if (id == "=" && leftParamId == "getexternstaticstructmember") {
                        var cd = param1 as Dsl.FunctionData;
                        if (null != cd.Name) {
                            cd.Name.SetId("setexternstatic");
                            cd.AddParam(param2);
                            GenerateConcreteSyntax(cd, sb, indent, firstLineUseIndent, funcOpts, calculator);
                            handled = true;
                        }
                    }
                    else if (id == "=" && leftParamId == "getexterninstancestructmember") {
                        var cd = param1 as Dsl.FunctionData;
                        if (null != cd.Name) {
                            cd.Name.SetId("setexterninstance");
                            cd.AddParam(param2);
                            GenerateConcreteSyntax(cd, sb, indent, firstLineUseIndent, funcOpts, calculator);
                            handled = true;
                        }
                    }
                    else if (id == "=" && rightParamId == "condexp" && null != fd2) {
                        GenerateAssignmentCondExp(param1, fd2, sb, indent, firstLineUseIndent, funcOpts, calculator);
                        handled = true;
                    }
                    if (!handled) {
                        if (firstLineUseIndent) {
                            sb.AppendFormat("{0}", GetIndentString(indent));
                        }
                        if (id != "=")
                            sb.Append("(");
                        GenerateSyntaxComponent(param1, sb, indent, false, funcOpts, calculator);
                        sb.AppendFormat(" {0} ", id);
                        GenerateSyntaxComponent(param2, sb, indent, false, funcOpts, calculator);
                        if (id != "=")
                            sb.Append(")");
                    }
                }
            }
            else {
                if (firstLineUseIndent) {
                    sb.AppendFormat("{0}", GetIndentString(indent));
                }
                //这里对委托到syslib的翻译提供一个基于dsl脚本翻译的机会，理论上可以提升运行效率                
                if (!CallDslHook(calculator, id, data, funcOpts, sb, indent)) {
                    if (id == "condexp") {
                        var p1 = data.GetParam(0);
                        var p2 = data.GetParamId(1);
                        var p3 = data.GetParam(2);
                        var p4 = data.GetParamId(3);
                        var p5 = data.GetParam(4);

                        if (p2 == "true" && p4 == "true") {
                            sb.Append("simplecondexp(");
                            GenerateSyntaxComponent(p1, sb, indent, false, funcOpts, calculator);
                            sb.Append(", ");
                            GenerateSyntaxComponent(p3, sb, indent, false, funcOpts, calculator);
                            sb.Append(", ");
                            GenerateSyntaxComponent(p5, sb, indent, false, funcOpts, calculator);
                            sb.Append(")");
                        }
                        else {
                            sb.Append("(function() ");
                            GenerateOtherCondExp(data, sb, indent, funcOpts, calculator);
                            sb.Append(" end)()");
                        }
                    }
                    else if (id == "condaccess") {
                        var p1 = data.GetParam(0);
                        var p2 = data.GetParam(1);
                        var p2Func = p2 as Dsl.FunctionData;
                        var isSimple = false;
                        if (null != p2Func && !ExistEmbedFunctionObject(p2Func)) {
                            var func = p2Func.GetParam(0) as Dsl.FunctionData;
                            if (null != func && !func.HaveId() && func.GetParamNum() == 1) {
                                func = func.GetParam(0) as Dsl.FunctionData;
                            }
                            if (null != func && func.GetId() == "funcobjret") {
                                isSimple = true;
                                p2 = func.GetParam(0);
                            }
                        }
                        if (isSimple) {
                            sb.Append("((");
                            GenerateSyntaxComponent(p1, sb, indent, false, funcOpts, calculator);
                            sb.Append(") and (");
                            GenerateSyntaxComponent(p2, sb, indent, false, funcOpts, calculator);
                            sb.Append(") or nil)");
                        }
                        else {
                            sb.Append("condaccess(");
                            GenerateSyntaxComponent(p1, sb, indent, false, funcOpts, calculator);
                            sb.Append(", ");
                            sb.Append(p2);
                            sb.Append(")");
                        }
                    }
                    else if (id == "nullcoalescing") {
                        var p1 = data.GetParam(0);
                        var p2 = data.GetParamId(1);
                        var p3 = data.GetParam(2);
                        var p3Func = p3 as Dsl.FunctionData;
                        if (p2 == "false" && null != p3Func && !ExistEmbedFunctionObject(p3Func)) {
                            var func = p3Func.GetParam(0) as Dsl.FunctionData;
                            if (null != func && !func.HaveId() && func.GetParamNum() == 1) {
                                func = func.GetParam(0) as Dsl.FunctionData;
                            }
                            if (null != func && func.GetId() == "funcobjret") {
                                p2 = "true";
                                p3 = func.GetParam(0);
                            }
                        }
                        if (p2 == "true") {
                            sb.Append("((");
                            GenerateSyntaxComponent(p1, sb, indent, false, funcOpts, calculator);
                            sb.Append(") or (");
                            GenerateSyntaxComponent(p3, sb, indent, false, funcOpts, calculator);
                            sb.Append("))");
                        }
                        else {
                            sb.Append("nullcoalescing(");
                            GenerateSyntaxComponent(p1, sb, indent, false, funcOpts, calculator);
                            sb.Append(", ");
                            GenerateSyntaxComponent(p3, sb, indent, false, funcOpts, calculator);
                            sb.Append(")");
                        }
                    }
                    else if (id == "comment") {
                        sb.AppendFormat("--{0}", data.GetParamId(0));
                    }
                    else if (id == "local") {
                        sb.Append("local ");
                        GenerateArguments(data, sb, indent, 0, funcOpts, calculator);
                    }
                    else if (id == "return") {
                        if (funcOpts.NeedFuncInfo) {
                            sb.AppendLine("__cs2lua_func_info = luafinalize(__cs2lua_func_info);");
                            sb.AppendFormat("{0}return", GetIndentString(indent));
                        }
                        else {
                            sb.Append("return");
                        }
                        if (data.GetParamNum() > 0)
                            sb.Append(" ");
                        GenerateArguments(data, sb, indent, 0, funcOpts, calculator);
                    }
                    else if (id == "funcobjret") {
                        sb.AppendFormat("{0}return", GetIndentString(indent));
                        if (data.GetParamNum() > 0)
                            sb.Append(" ");
                        GenerateArguments(data, sb, indent, 0, funcOpts, calculator);
                    }
                    else if (id == "prefixoperator" || id == "postfixoperator") {
                        GeneratePrefixPostfixOperator(data, sb, false, funcOpts, calculator);
                    }
                    else if (id == "execunary") {
                        string op = data.GetParamId(0);
                        var p1 = data.GetParam(1);
                        string type1 = CalcTypeString(data.GetParam(2));
                        string typeKind1 = CalcTypeString(data.GetParam(3));
                        string intOp = op;
                        if (op == "++")
                            intOp = "+";
                        else if (op == "--")
                            intOp = "-";
                        int intOpIndex;
                        if (IsIntegerType(type1, typeKind1) && TryGetSpecialIntegerOperatorIndex(intOp, out intOpIndex)) {
                            if (op == "++" || op == "--") {
                                if (s_IntegerTypes.Contains(type1)) {
                                    sb.Append("(");
                                    GenerateSyntaxComponent(p1, sb, indent, false, funcOpts, calculator);
                                    sb.AppendFormat(" {0} 1)", intOp);
                                }
                                else {
                                    sb.AppendFormat("invokeintegeroperator({0}, \"{1}\", ", intOpIndex, intOp);
                                    GenerateSyntaxComponent(p1, sb, indent, false, funcOpts, calculator);
                                    sb.AppendFormat(", 1, {0}, {1})", type1, type1);
                                }
                            }
                            if (s_IntegerTypes.Contains(type1) && (op == "+" || op == "-")) {
                                sb.AppendFormat("( {0} ", intOp);
                                GenerateSyntaxComponent(p1, sb, indent, false, funcOpts, calculator);
                                sb.Append(")");
                            }
                            else {
                                sb.AppendFormat("invokeintegeroperator({0}, \"{1}\", nil, ", intOpIndex, intOp);
                                GenerateSyntaxComponent(p1, sb, indent, false, funcOpts, calculator);
                                sb.AppendFormat(", nil, {0})", type1, type1);
                            }
                        }
                        else {
                            string functor;
                            if (s_UnaryFunctor.TryGetValue(op, out functor)) {
                                sb.AppendFormat("{0}(", functor);
                                GenerateSyntaxComponent(data.GetParam(1), sb, indent, false, funcOpts, calculator);
                                sb.Append(")");
                            }
                            else {
                                op = ConvertOperator(op);
                                sb.AppendFormat("({0} ", op);
                                GenerateSyntaxComponent(data.GetParam(1), sb, indent, false, funcOpts, calculator);
                                sb.Append(")");
                            }
                        }
                    }
                    else if (id == "execbinary") {
                        string op = data.GetParamId(0);
                        var p1 = data.GetParam(1);
                        var p2 = data.GetParam(2);
                        string type1 = CalcTypeString(data.GetParam(3));
                        string type2 = CalcTypeString(data.GetParam(4));
                        string typeKind1 = CalcTypeString(data.GetParam(5));
                        string typeKind2 = CalcTypeString(data.GetParam(6));
                        int intOpIndex;
                        if (IsIntegerType(type1, typeKind1) && IsIntegerType(type2, typeKind2) && TryGetSpecialIntegerOperatorIndex(op, out intOpIndex)) {
                            if (s_IntegerTypes.Contains(type1) && s_IntegerTypes.Contains(type2) && (op == "+" || op == "-" || op == "%")) {
                                sb.Append("(");
                                GenerateSyntaxComponent(p1, sb, indent, false, funcOpts, calculator);
                                sb.AppendFormat(" {0} ", op);
                                GenerateSyntaxComponent(p2, sb, indent, false, funcOpts, calculator);
                                sb.Append(")");
                            }
                            else {
                                sb.AppendFormat("invokeintegeroperator({0}, \"{1}\", ", intOpIndex, op);
                                GenerateSyntaxComponent(p1, sb, indent, false, funcOpts, calculator);
                                sb.Append(", ");
                                GenerateSyntaxComponent(p2, sb, indent, false, funcOpts, calculator);
                                sb.AppendFormat(", {0}, {1})", type1, type2);
                            }
                        }
                        else if (op == "+" && (type1 == "System.String" || type2 == "System.String")) {
                            bool tostr1 = type1 != "System.String";
                            bool tostr2 = type2 != "System.String";
                            sb.Append("stringconcat(");
                            if (tostr1)
                                sb.Append("tostring(");
                            GenerateSyntaxComponent(p1, sb, indent, false, funcOpts, calculator);
                            if (tostr1)
                                sb.Append(")");
                            sb.Append(", ");
                            if (tostr2)
                                sb.Append("tostring(");
                            GenerateSyntaxComponent(p2, sb, indent, false, funcOpts, calculator);
                            if (tostr2)
                                sb.Append(")");
                            sb.Append(")");
                        }
                        else if ((op == "==" || op == "!=") && type1 == "System.String" && type2 == "System.String") {
                            if (op == "==") {
                                sb.Append("stringisequal(");
                                GenerateSyntaxComponent(p1, sb, indent, false, funcOpts, calculator);
                                sb.Append(", ");
                                GenerateSyntaxComponent(p2, sb, indent, false, funcOpts, calculator);
                                sb.Append(")");
                            }
                            else {
                                sb.Append("(not stringisequal(");
                                GenerateSyntaxComponent(p1, sb, indent, false, funcOpts, calculator);
                                sb.Append(", ");
                                GenerateSyntaxComponent(p2, sb, indent, false, funcOpts, calculator);
                                sb.Append("))");
                            }
                        }
                        else if ((op == "==" || op == "!=") && !IsBasicType(type1, typeKind1, true) && !IsBasicType(type2, typeKind2, true)) {
                            if (op == "==") {
                                sb.Append("isequal(");
                                GenerateSyntaxComponent(p1, sb, indent, false, funcOpts, calculator);
                                sb.Append(", ");
                                GenerateSyntaxComponent(p2, sb, indent, false, funcOpts, calculator);
                                sb.Append(")");
                            }
                            else {
                                sb.Append("(not isequal(");
                                GenerateSyntaxComponent(p1, sb, indent, false, funcOpts, calculator);
                                sb.Append(", ");
                                GenerateSyntaxComponent(p2, sb, indent, false, funcOpts, calculator);
                                sb.Append("))");
                            }
                        }
                        else {
                            string functor;
                            if (s_BinaryFunctor.TryGetValue(op, out functor)) {
                                sb.AppendFormat("{0}(", functor);
                                GenerateSyntaxComponent(p1, sb, indent, false, funcOpts, calculator);
                                sb.Append(", ");
                                GenerateSyntaxComponent(p2, sb, indent, false, funcOpts, calculator);
                                sb.Append(")");
                            }
                            else {
                                op = ConvertOperator(op);
                                sb.Append("(");
                                GenerateSyntaxComponent(p1, sb, indent, false, funcOpts, calculator);
                                sb.AppendFormat(" {0} ", op);
                                GenerateSyntaxComponent(p2, sb, indent, false, funcOpts, calculator);
                                sb.Append(")");
                            }
                        }
                    }
                    else if (id == "getstatic") {
                        var kind = CalcTypeString(data.GetParam(0));
                        var obj = data.Params[1];
                        var member = data.Params[2];
                        GenerateSyntaxComponent(obj, sb, indent, false, funcOpts, calculator);
                        if (kind == "SymbolKind.Property") {
                            sb.AppendFormat(".get_{0}()", member.GetId());
                        }
                        else {
                            sb.AppendFormat(".{0}", member.GetId());
                        }
                    }
                    else if (id == "getinstance") {
                        var kind = CalcTypeString(data.GetParam(0));
                        var obj = data.Params[1];
                        var className = CalcTypeString(data.GetParam(2));
                        var member = data.Params[3];
                        GenerateSyntaxComponent(obj, sb, indent, false, funcOpts, calculator);
                        if (kind == "SymbolKind.Property") {
                            sb.AppendFormat(":get_{0}()", member.GetId());
                        }
                        else {
                            sb.AppendFormat(".{0}", member.GetId());
                        }
                    }
                    else if (id == "setstatic") {
                        var kind = CalcTypeString(data.GetParam(0));
                        var obj = data.Params[1];
                        var member = data.Params[2];
                        var val = data.Params[3];
                        GenerateSyntaxComponent(obj, sb, indent, false, funcOpts, calculator);
                        if (kind == "SymbolKind.Property") {
                            sb.AppendFormat(".set_{0}(", member.GetId());
                            GenerateSyntaxComponent(val, sb, indent, false, funcOpts, calculator);
                            sb.Append(")");
                        }
                        else {
                            sb.AppendFormat(".{0}", member.GetId());
                            sb.Append(" = ");
                            GenerateSyntaxComponent(val, sb, indent, false, funcOpts, calculator);
                        }
                    }
                    else if (id == "setinstance") {
                        var kind = CalcTypeString(data.GetParam(0));
                        var obj = data.Params[1];
                        var className = CalcTypeString(data.GetParam(2));
                        var member = data.Params[3];
                        var val = data.Params[4];
                        GenerateSyntaxComponent(obj, sb, indent, false, funcOpts, calculator);
                        if (kind == "SymbolKind.Property") {
                            sb.AppendFormat(":set_{0}(", member.GetId());
                            GenerateSyntaxComponent(val, sb, indent, false, funcOpts, calculator);
                            sb.Append(")");
                        }
                        else {
                            sb.AppendFormat(".{0}", member.GetId());
                            sb.Append(" = ");
                            GenerateSyntaxComponent(val, sb, indent, false, funcOpts, calculator);
                        }
                    }
                    else if (id == "getexternstatic") {
                        var kind = CalcTypeString(data.GetParam(0));
                        var obj = data.Params[1];
                        var member = data.Params[2];
                        GenerateSyntaxComponent(obj, sb, indent, false, funcOpts, calculator);
                        sb.AppendFormat(".{0}", member.GetId());
                    }
                    else if (id == "getexterninstance") {
                        var kind = CalcTypeString(data.GetParam(0));
                        var obj = data.Params[1];
                        var className = CalcTypeString(data.GetParam(2));
                        var member = data.Params[3];
                        GenerateSyntaxComponent(obj, sb, indent, false, funcOpts, calculator);
                        sb.AppendFormat(".{0}", member.GetId());
                    }
                    else if (id == "setexternstatic") {
                        var kind = CalcTypeString(data.GetParam(0));
                        var obj = data.Params[1];
                        var member = data.Params[2];
                        var val = data.Params[3];
                        GenerateSyntaxComponent(obj, sb, indent, false, funcOpts, calculator);
                        sb.AppendFormat(".{0}", member.GetId());
                        sb.Append(" = ");
                        GenerateSyntaxComponent(val, sb, indent, false, funcOpts, calculator);
                    }
                    else if (id == "setexterninstance") {
                        var kind = CalcTypeString(data.GetParam(0));
                        var obj = data.Params[1];
                        var className = CalcTypeString(data.GetParam(2));
                        var member = data.Params[3];
                        var val = data.Params[4];
                        GenerateSyntaxComponent(obj, sb, indent, false, funcOpts, calculator);
                        sb.AppendFormat(".{0}", member.GetId());
                        sb.Append(" = ");
                        GenerateSyntaxComponent(val, sb, indent, false, funcOpts, calculator);
                    }
                    else if (id == "callstatic" || id == "callexternstatic") {
                        var obj = data.Params[0];
                        var member = data.Params[1];
                        var mid = member.GetId();
                        GenerateSyntaxComponent(obj, sb, indent, false, funcOpts, calculator);
                        sb.AppendFormat(".{0}", mid);
                        sb.Append("(");
                        int start = 2;
                        GenerateArguments(data, sb, indent, start, funcOpts, calculator);
                        sb.Append(")");
                    }
                    else if (id == "callinstance" || id == "callexterninstance") {
                        var obj = data.Params[0];
                        var className = CalcTypeString(data.GetParam(1));
                        var member = data.Params[2];
                        var mid = member.GetId();
                        var objCd = obj as Dsl.FunctionData;
                        GenerateSyntaxComponent(obj, sb, indent, false, funcOpts, calculator);
                        if (null != objCd && objCd.GetId() == "getinstance" && objCd.GetParamNum() == 4 && objCd.GetParamId(3) == "base") {
                            sb.AppendFormat(":__self__{0}", mid);
                        }
                        else {
                            sb.AppendFormat(":{0}", mid);
                        }
                        sb.Append("(");
                        int start = 3;
                        GenerateArguments(data, sb, indent, start, funcOpts, calculator);
                        sb.Append(")");
                    }
                    else if (id == "calldelegation" || id == "callexterndelegation") {
                        var obj = data.Params[0];
                        var className = CalcTypeString(data.GetParam(1));
                        GenerateSyntaxComponent(obj, sb, indent, false, funcOpts, calculator);
                        sb.Append("(");
                        int start = 2;
                        GenerateArguments(data, sb, indent, start, funcOpts, calculator);
                        sb.Append(")");
                    }
                    else if (id == "callextension") {
                        var obj = data.Params[0];
                        var member = data.Params[1];
                        var mid = member.GetId();
                        GenerateSyntaxComponent(obj, sb, indent, false, funcOpts, calculator);
                        sb.AppendFormat(".{0}", mid);
                        sb.Append("(");
                        int start = 2;
                        GenerateArguments(data, sb, indent, start, funcOpts, calculator);
                        sb.Append(")");
                    }
                    else if (id == "callexternextension") {
                        sb.Append(id);
                        sb.Append('(');
                        GenerateArguments(data, sb, indent, 0, funcOpts, calculator);
                        sb.Append(')');
                    }
                    else if (id == "getstaticindexer") {
                        var _class = data.Params[0];
                        var _member = data.Params[1].GetId();
                        var _pct = data.Params[2].GetId();
                        int ct;
                        int.TryParse(_pct, out ct);
                        if (ct == 1) {
                            var strClass = CalcTypeString(_class);
                            var strMember = _member;
                            int indexerType;
                            bool isIndexerByLua = IndexerByLualib("@@internal", string.Empty, strClass, strMember, out indexerType);

                            var _index = data.Params[3];
                            GenerateSyntaxComponent(_class, sb, 0, false, funcOpts, calculator);
                            sb.Append('[');
                            GenerateSyntaxComponent(_index, sb, 0, false, funcOpts, calculator);
                            if (isIndexerByLua && indexerType == (int)IndexerTypeEnum.LikeArray) {
                                sb.Append(" + 1");
                            }
                            sb.Append(']');
                        }
                        else {
                            int start = 3;
                            GenerateSyntaxComponent(_class, sb, 0, false, funcOpts, calculator);
                            sb.Append('.');
                            sb.Append(_member);
                            sb.Append('(');
                            GenerateArguments(data, sb, indent, start, funcOpts, calculator);
                            sb.Append(')');
                        }
                    }
                    else if (id == "getinstanceindexer") {
                        var _obj = data.Params[0];
                        var _class = data.Params[1];
                        var _member = data.Params[2].GetId();
                        var _pct = data.Params[3].GetId();
                        int ct;
                        int.TryParse(_pct, out ct);
                        if (ct == 1) {
                            var strObj = CalcExpressionString(_obj, funcOpts, calculator);
                            var strClass = CalcTypeString(_class);
                            var strMember = _member;
                            int indexerType;
                            bool isIndexerByLua = IndexerByLualib("@@internal", strObj, strClass, strMember, out indexerType);

                            var _index = data.Params[4];
                            GenerateSyntaxComponent(_obj, sb, indent, false, funcOpts, calculator);
                            sb.Append('[');
                            GenerateSyntaxComponent(_index, sb, 0, false, funcOpts, calculator);
                            if (isIndexerByLua && indexerType == (int)IndexerTypeEnum.LikeArray) {
                                sb.Append(" + 1");
                            }
                            sb.Append(']');
                        }
                        else {
                            int start = 4;
                            GenerateSyntaxComponent(_obj, sb, indent, false, funcOpts, calculator);
                            sb.Append(':');
                            sb.Append(_member);
                            sb.Append('(');
                            GenerateSyntaxComponent(_obj, sb, indent, false, funcOpts, calculator);
                            sb.Append(", ");
                            GenerateArguments(data, sb, indent, start, funcOpts, calculator);
                            sb.Append(')');
                        }
                    }
                    else if (id == "setstaticindexer") {
                        var _class = data.Params[0];
                        var _member = data.Params[1].GetId();
                        var _pct = data.Params[2].GetId();
                        var _toplevel = data.Params[3].GetId();
                        int ct;
                        int.TryParse(_pct, out ct);
                        if (ct == 2 && _toplevel == "true") {
                            var strClass = CalcTypeString(_class);
                            var strMember = _member;
                            int indexerType;
                            bool isIndexerByLua = IndexerByLualib("@@internal", string.Empty, strClass, strMember, out indexerType);

                            var _index = data.Params[4];
                            var _val = data.Params[5];
                            GenerateSyntaxComponent(_class, sb, 0, false, funcOpts, calculator);
                            sb.Append('[');
                            GenerateSyntaxComponent(_index, sb, 0, false, funcOpts, calculator);
                            if (isIndexerByLua && indexerType == (int)IndexerTypeEnum.LikeArray) {
                                sb.Append(" + 1");
                            }
                            sb.Append(']');
                            sb.Append(" = ");
                            GenerateSyntaxComponent(_val, sb, 0, false, funcOpts, calculator);
                        }
                        else {
                            int start = 4;
                            GenerateSyntaxComponent(_class, sb, 0, false, funcOpts, calculator);
                            sb.Append('.');
                            sb.Append(_member);
                            sb.Append('(');
                            GenerateArguments(data, sb, indent, start, funcOpts, calculator);
                            sb.Append(')');
                        }
                    }
                    else if (id == "setinstanceindexer") {
                        var _obj = data.Params[0];
                        var _class = data.Params[1];
                        var _member = data.Params[2].GetId();
                        var _pct = data.Params[3].GetId();
                        var _toplevel = data.Params[4].GetId();
                        int ct;
                        int.TryParse(_pct, out ct);
                        if (ct == 2 && _toplevel == "true") {
                            var strObj = CalcExpressionString(_obj, funcOpts, calculator);
                            var strClass = CalcTypeString(_class);
                            var strMember = _member;
                            int indexerType;
                            bool isIndexerByLua = IndexerByLualib("@@internal", strObj, strClass, strMember, out indexerType);

                            var _index = data.Params[5];
                            var _val = data.Params[6];
                            GenerateSyntaxComponent(_obj, sb, indent, false, funcOpts, calculator);
                            sb.Append('[');
                            GenerateSyntaxComponent(_index, sb, 0, false, funcOpts, calculator);
                            if (isIndexerByLua && indexerType == (int)IndexerTypeEnum.LikeArray) {
                                sb.Append(" + 1");
                            }
                            sb.Append(']');
                            sb.Append(" = ");
                            GenerateSyntaxComponent(_val, sb, 0, false, funcOpts, calculator);
                        }
                        else {
                            int start = 5;
                            GenerateSyntaxComponent(_obj, sb, indent, false, funcOpts, calculator);
                            sb.Append(':');
                            sb.Append(_member);
                            sb.Append('(');
                            GenerateSyntaxComponent(_obj, sb, indent, false, funcOpts, calculator);
                            sb.Append(", ");
                            GenerateArguments(data, sb, indent, start, funcOpts, calculator);
                            sb.Append(')');
                        }
                    }
                    else if (id == "getexternstaticindexer") {
                        var _callerClass = data.Params[0];
                        var _class = data.Params[1];
                        var _member = data.Params[2];
                        var strCallerClass = CalcTypeString(_callerClass);
                        var strClass = CalcTypeString(_class);
                        var strMember = CalcTypeString(_member);
                        int indexerType;
                        if (IndexerByLualib(strCallerClass, string.Empty, strClass, strMember, out indexerType)) {
                            var _pct = data.Params[3].GetId();
                            int ct;
                            int.TryParse(_pct, out ct);
                            if (ct == 1) {
                                var _index = data.Params[4];
                                sb.Append(strClass);
                                sb.Append('[');
                                GenerateSyntaxComponent(_index, sb, 0, false, funcOpts, calculator);
                                if (indexerType == (int)IndexerTypeEnum.LikeArray) {
                                    sb.Append(" + 1");
                                }
                                sb.Append(']');
                            }
                            else {
                                int start = 4;
                                sb.Append(strClass);
                                sb.Append('[');
                                sb.Append('"');
                                sb.Append(strMember);
                                sb.Append('"');
                                sb.Append(']');
                                sb.Append('(');
                                GenerateArguments(data, sb, indent, start, funcOpts, calculator);
                                sb.Append(')');
                            }
                        }
                        else {
                            sb.Append(id);
                            sb.Append('(');
                            GenerateArguments(data, sb, indent, 0, funcOpts, calculator);
                            sb.Append(')');
                        }
                    }
                    else if (id == "getexterninstanceindexer") {
                        var _callerClass = data.Params[0];
                        var _obj = data.Params[1];
                        var _class = data.Params[2];
                        var _member = data.Params[3];
                        var strCallerClass = CalcTypeString(_callerClass);
                        var strObj = CalcExpressionString(_obj, funcOpts, calculator);
                        var strClass = CalcTypeString(_class);
                        var strMember = CalcTypeString(_member);
                        int indexerType;
                        if (IndexerByLualib(strCallerClass, strObj, strClass, strMember, out indexerType)) {
                            var _pct = data.Params[4].GetId();
                            int ct;
                            int.TryParse(_pct, out ct);
                            if (ct == 1) {
                                var _index = data.Params[5];
                                sb.Append(strObj);
                                sb.Append('[');
                                GenerateSyntaxComponent(_index, sb, 0, false, funcOpts, calculator);
                                if (indexerType == (int)IndexerTypeEnum.LikeArray) {
                                    sb.Append(" + 1");
                                }
                                sb.Append(']');
                            }
                            else {
                                int start = 5;
                                GenerateSyntaxComponent(_obj, sb, indent, false, funcOpts, calculator);
                                sb.Append('[');
                                sb.Append('"');
                                sb.Append(strMember);
                                sb.Append('"');
                                sb.Append(']');
                                sb.Append('(');
                                GenerateSyntaxComponent(_obj, sb, indent, false, funcOpts, calculator);
                                sb.Append(", ");
                                GenerateArguments(data, sb, indent, start, funcOpts, calculator);
                                sb.Append(')');
                            }
                        }
                        else {
                            sb.Append(id);
                            sb.Append('(');
                            GenerateArguments(data, sb, indent, 0, funcOpts, calculator);
                            sb.Append(')');
                        }
                    }
                    else if (id == "setexternstaticindexer") {
                        var _callerClass = data.Params[0];
                        var _class = data.Params[1];
                        var _member = data.Params[2];
                        var strCallerClass = CalcTypeString(_callerClass);
                        var strClass = CalcTypeString(_class);
                        var strMember = CalcTypeString(_member);
                        int indexerType;
                        if (IndexerByLualib(strCallerClass, string.Empty, strClass, strMember, out indexerType)) {
                            var _pct = data.Params[3].GetId();
                            var _toplevel = data.Params[4].GetId();
                            int ct;
                            int.TryParse(_pct, out ct);
                            if (ct == 2 && _toplevel == "true") {
                                var _index = data.Params[5];
                                var _val = data.Params[6];
                                sb.Append(strClass);
                                sb.Append('[');
                                GenerateSyntaxComponent(_index, sb, 0, false, funcOpts, calculator);
                                if (indexerType == (int)IndexerTypeEnum.LikeArray) {
                                    sb.Append(" + 1");
                                }
                                sb.Append(']');
                                sb.Append(" = ");
                                GenerateSyntaxComponent(_val, sb, 0, false, funcOpts, calculator);
                            }
                            else {
                                int start = 5;
                                sb.Append(strClass);
                                sb.Append('[');
                                sb.Append('"');
                                sb.Append(strMember);
                                sb.Append('"');
                                sb.Append(']');
                                sb.Append('(');
                                GenerateArguments(data, sb, indent, start, funcOpts, calculator);
                                sb.Append(')');
                            }
                        }
                        else {
                            sb.Append(id);
                            sb.Append('(');
                            GenerateArguments(data, sb, indent, 0, funcOpts, calculator);
                            sb.Append(')');
                        }
                    }
                    else if (id == "setexterninstanceindexer") {
                        var _callerClass = data.Params[0];
                        var _obj = data.Params[1];
                        var _class = data.Params[2];
                        var _member = data.Params[3];
                        var strCallerClass = CalcTypeString(_callerClass);
                        var strObj = CalcExpressionString(_obj, funcOpts, calculator);
                        var strClass = CalcTypeString(_class);
                        var strMember = CalcTypeString(_member);
                        int indexerType;
                        if (IndexerByLualib(strCallerClass, strObj, strClass, strMember, out indexerType)) {
                            var _pct = data.Params[4].GetId();
                            var _toplevel = data.Params[5].GetId();
                            int ct;
                            int.TryParse(_pct, out ct);
                            if (ct == 2 && _toplevel == "true") {
                                var _index = data.Params[6];
                                var _val = data.Params[7];
                                sb.Append(strObj);
                                sb.Append('[');
                                GenerateSyntaxComponent(_index, sb, 0, false, funcOpts, calculator);
                                if (indexerType == (int)IndexerTypeEnum.LikeArray) {
                                    sb.Append(" + 1");
                                }
                                sb.Append(']');
                                sb.Append(" = ");
                                GenerateSyntaxComponent(_val, sb, 0, false, funcOpts, calculator);
                            }
                            else {
                                int start = 6;
                                GenerateSyntaxComponent(_obj, sb, indent, false, funcOpts, calculator);
                                sb.Append('[');
                                sb.Append('"');
                                sb.Append(strMember);
                                sb.Append('"');
                                sb.Append(']');
                                sb.Append('(');
                                GenerateSyntaxComponent(_obj, sb, indent, false, funcOpts, calculator);
                                sb.Append(", ");
                                GenerateArguments(data, sb, indent, start, funcOpts, calculator);
                                sb.Append(')');
                            }
                        }
                        else {
                            sb.Append(id);
                            sb.Append('(');
                            GenerateArguments(data, sb, indent, 0, funcOpts, calculator);
                            sb.Append(')');
                        }
                    }
                    else if (id == "typeof") {
                        var typeStr = CalcTypeString(data.GetParam(0));
                        sb.AppendFormat("{0}", typeStr);
                    }
                    else if (id == "params") {
                        var typeStr = CalcTypeString(data.GetParam(0));
                        var typeKind = CalcTypeString(data.GetParam(1));
                        sb.AppendFormat("wrapparams({{...}}, {0}, {1})", typeStr, typeKind);
                    }
                    else if (id == "paramsremove") {
                        sb.AppendFormat("table.remove({0})", data.GetParamId(0));
                    }
                    else if (id == "typeargs") {
                        if (data.GetParamNum() > 0) {
                            sb.Append("{");
                            GenerateArguments(data, sb, indent, 0, funcOpts, calculator);
                            sb.Append("}");
                        }
                        else {
                            sb.Append("nil");
                        }
                    }
                    else if (id == "typekinds") {
                        if (data.GetParamNum() > 0) {
                            sb.Append("{");
                            GenerateArguments(data, sb, indent, 0, funcOpts, calculator);
                            sb.Append("}");
                        }
                        else {
                            sb.Append("nil");
                        }
                    }
                    else if (id == "builddelegation") {
                        var srcPos = data.GetParamId(0);
                        var delegationKey = data.GetParamId(1);
                        var objOrClass = data.GetParam(2);
                        var methodName = data.GetParamId(3);
                        var isStatic = data.GetParamId(4) == "true";

                        sb.AppendLine("(function()");
                        ++indent;
                        sb.AppendFormat("{0}local __obj_{1} = ", GetIndentString(indent), srcPos);
                        GenerateSyntaxComponent(objOrClass, sb, 0, false, funcOpts, calculator);
                        sb.AppendLine(";");
                        sb.AppendFormatLine("{0}local __fk_{1} = calcdelegationkey(\"{2}\", __obj_{1});", GetIndentString(indent), srcPos, delegationKey);
                        sb.AppendFormatLine("{0}return builddelegationonce(__fk_{1}, getdelegation(__fk_{1}) or function(...)", GetIndentString(indent), srcPos, delegationKey);
                        ++indent;
                        sb.AppendFormat("{0}", GetIndentString(indent));
                        sb.AppendFormat("return __obj_{0}", srcPos);
                        if (isStatic) {
                            sb.Append(".");
                        }
                        else {
                            sb.Append(":");
                        }
                        sb.Append(methodName);
                        sb.AppendLine("(...);");
                        --indent;
                        sb.AppendFormatLine("{0}end);", GetIndentString(indent));
                        --indent;
                        sb.AppendFormat("{0}end)()", GetIndentString(indent));
                    }
                    else if (id == "setdelegation") {
                        var kind = CalcTypeString(data.GetParam(0));
                        var obj = data.Params[1];
                        var val = data.Params[2];
                        GenerateSyntaxComponent(obj, sb, indent, false, funcOpts, calculator);
                        sb.Append(" = ");
                        GenerateSyntaxComponent(val, sb, indent, false, funcOpts, calculator);
                    }
                    else if (id == "setstaticdelegation") {
                        var kind = CalcTypeString(data.GetParam(0));
                        var obj = data.Params[1];
                        var member = data.Params[2];
                        var val = data.Params[3];
                        if (kind == "SymbolKind.Property") {
                            GenerateSyntaxComponent(obj, sb, indent, false, funcOpts, calculator);
                            sb.AppendFormat(".set_{0}(", member.GetId());
                            GenerateSyntaxComponent(val, sb, indent, false, funcOpts, calculator);
                            sb.Append(")");
                        }
                        else {
                            GenerateSyntaxComponent(obj, sb, indent, false, funcOpts, calculator);
                            sb.AppendFormat(".{0}", member.GetId());
                            sb.Append(" = ");
                            GenerateSyntaxComponent(val, sb, indent, false, funcOpts, calculator);
                        }
                    }
                    else if (id == "setinstancedelegation") {
                        var kind = CalcTypeString(data.GetParam(0));
                        var obj = data.Params[1];
                        var className = CalcTypeString(data.GetParam(2));
                        var member = data.Params[3];
                        var val = data.Params[4];
                        if (kind == "SymbolKind.Property") {
                            GenerateSyntaxComponent(obj, sb, indent, false, funcOpts, calculator);
                            sb.AppendFormat(":set_{0}(", member.GetId());
                            GenerateSyntaxComponent(val, sb, indent, false, funcOpts, calculator);
                            sb.Append(")");
                        }
                        else {
                            GenerateSyntaxComponent(obj, sb, indent, false, funcOpts, calculator);
                            sb.AppendFormat(".{0}", member.GetId());
                            sb.Append(" = ");
                            GenerateSyntaxComponent(val, sb, indent, false, funcOpts, calculator);
                        }
                    }
                    else if (id == "buildbaseobj") {
                        sb.Append("buildbaseobj(");
                        string prestr = string.Empty;
                        for (int ix = 0; ix < 3; ++ix) {
                            var param = data.Params[ix];
                            sb.Append(prestr);
                            GenerateSyntaxComponent(param, sb, indent, false, funcOpts, calculator);
                            prestr = ", ";
                        }
                        sb.Append(prestr);
                        var ctor = data.GetParamId(3);
                        sb.Append('"');
                        sb.Append("__self__");
                        sb.Append(ctor);
                        sb.Append('"');
                        if (data.GetParamNum() > 4) {
                            sb.Append(prestr);
                            GenerateArguments(data, sb, indent, 4, funcOpts, calculator);
                        }
                        sb.Append(")");
                    }
                    else if (id == "newobject" || id == "newexternobject" ||
                        id == "newstruct" || id == "newexternstruct" ||
                        id == "newdictionary" || id == "newexterndictionary" ||
                        id == "newlist" || id == "newexternlist" ||
                        id == "newcollection" || id == "newexterncollection") {
                        var _classStr = CalcTypeString(data.GetParam(0));
                        var keyStr = data.GetParamId(1);
                        var typeArgs = data.GetParam(2) as Dsl.FunctionData;
                        var typeKinds = data.GetParam(3) as Dsl.FunctionData;
                        sb.Append(id);
                        sb.Append('(');
                        sb.Append(_classStr);
                        sb.Append(", ");
                        if (null != typeArgs && typeArgs.GetParamNum() > 0) {
                            sb.AppendFormat("buildglobalinfoonce(\"{0}_TypeArgs\", getglobalinfo(\"{0}_TypeArgs\") or ", keyStr);
                            GenerateSyntaxComponent(typeArgs, sb, indent, false, funcOpts, calculator);
                            sb.Append("), ");
                        }
                        else {
                            sb.Append("nil, ");
                        }
                        if (null != typeKinds && typeKinds.GetParamNum() > 0) {
                            sb.AppendFormat("buildglobalinfoonce(\"{0}_TypeKinds\", getglobalinfo(\"{0}_TypeKinds\") or ", keyStr);
                            GenerateSyntaxComponent(typeKinds, sb, indent, false, funcOpts, calculator);
                            sb.Append("), ");
                        }
                        else {
                            sb.Append("nil, ");
                        }
                        GenerateArguments(data, sb, indent, 4, funcOpts, calculator);
                        sb.Append(')');
                    }
                    else if (id == "anonymousobject") {
                        sb.Append("wrapanonymousobject{");
                        GenerateArguments(data, sb, indent, 0, funcOpts, calculator);
                        sb.Append("}");
                    }
                    else if (id == "literaldictionary") {
                        var keyStr = data.GetParamId(0);
                        var typeArgs = data.GetParam(1);
                        var typeKinds = data.GetParam(2);
                        sb.Append("{");
                        string prestr = string.Empty;
                        for (int ix = 3; ix < data.Params.Count; ++ix) {
                            var param = data.Params[ix] as Dsl.FunctionData;
                            sb.Append(prestr);
                            var k = param.GetParam(0);
                            var kcd = k as Dsl.FunctionData;
                            var v = param.GetParam(1);
                            if (null != kcd) {
                                sb.AppendFormat("[{0}] = ", CalcTypeString(k));
                            }
                            else if (k.GetIdType() == Dsl.ValueData.STRING_TOKEN) {
                                sb.AppendFormat("[\"{0}\"] = ", Escape(k.GetId()));
                            }
                            else {
                                sb.AppendFormat("[{0}] = ", k.GetId());
                            }
                            GenerateSyntaxComponent(v, sb, indent, false, funcOpts, calculator);
                            prestr = ", ";
                        }
                        sb.Append("}");
                    }
                    else if (id == "literallist" || id == "literalcollection" || id == "literalcomplex") {
                        var keyStr = data.GetParamId(0);
                        var typeArgs = data.GetParam(1);
                        var typeKinds = data.GetParam(2);
                        sb.Append("{");
                        GenerateArguments(data, sb, indent, 3, funcOpts, calculator);
                        sb.Append("}");
                    }
                    else if (id == "literalarray") {
                        var typeStr = CalcTypeString(data.GetParam(0));
                        var typeKind = CalcTypeString(data.GetParam(1));
                        sb.Append("wraparray({");
                        GenerateArguments(data, sb, indent, 2, funcOpts, calculator);
                        sb.AppendFormat("}}, nil, {0}, {1})", typeStr, typeKind);
                    }
                    else if (id == "newarray") {
                        var typeStr = CalcTypeString(data.GetParam(0));
                        var typeKind = CalcTypeString(data.GetParam(1));
                        if (data.GetParamNum() > 2) {
                            var vname = data.GetParamId(2);
                            sb.AppendFormat("wraparray({{}}, {0}, {1}, {2})", vname, typeStr, typeKind);
                        }
                        else {
                            sb.AppendFormat("wraparray({{}}, nil, {0}, {1})", typeStr, typeKind);
                        }
                    }
                    else if (id == "newmultiarray") {
                        var typeStr = CalcTypeString(data.GetParam(0));
                        var typeKind = CalcTypeString(data.GetParam(1));
                        var defVal = data.GetParam(2);
                        int ct;
                        int.TryParse(data.GetParamId(3), out ct);
                        if (ct <= 3) {
                            //三维以下数组的定义在lualib里实现
                            sb.AppendFormat("newarraydim{0}({1}, {2}, ", ct, typeStr, typeKind);
                            GenerateSyntaxComponent(defVal, sb, 0, false, funcOpts, calculator);
                            if (ct > 0) {
                                sb.Append(", ");
                                var exp = data.GetParam(4 + 0);
                                GenerateSyntaxComponent(exp, sb, 0, false, funcOpts, calculator);
                            }
                            if (ct > 1) {
                                sb.Append(", ");
                                var exp = data.GetParam(4 + 1);
                                GenerateSyntaxComponent(exp, sb, 0, false, funcOpts, calculator);
                            }
                            if (ct > 2) {
                                sb.Append(", ");
                                var exp = data.GetParam(4 + 2);
                                GenerateSyntaxComponent(exp, sb, 0, false, funcOpts, calculator);
                            }
                            sb.Append(")");
                        }
                        else {
                            //四维及以上数组在这里使用函数对象嵌入初始化代码，应该很少用到
                            sb.Append("(function()");
                            for (int i = 0; i < ct; ++i) {
                                sb.AppendFormat(" local d{0} = ", i);
                                var exp = data.GetParam(4 + i);
                                GenerateSyntaxComponent(exp, sb, 0, false, funcOpts, calculator);
                                if (i == 0) {
                                    sb.AppendFormat("; local arr = wraparray({{}}, d0, {0}, {1})", typeStr, typeKind);
                                }
                                sb.AppendFormat("; for i{0} = 1, d{1} do arr{2} = ", i, i, GetArraySubscriptString(i));
                                if (i < ct - 1) {
                                    sb.Append("wraparray({}, ");
                                    var nextExp = data.GetParam(4 + i + 1);
                                    GenerateSyntaxComponent(nextExp, sb, 0, false, funcOpts, calculator);
                                    sb.AppendFormat(", {0}, {1});", typeStr, typeKind);
                                }
                                else {
                                    GenerateSyntaxComponent(defVal, sb, 0, false, funcOpts, calculator);
                                    sb.Append(";");
                                }
                            }
                            for (int i = 0; i < ct; ++i) {
                                sb.Append(" end;");
                            }
                            sb.Append(" return arr; end)()");
                        }
                    }
                    else if (id == "for") {
                        sb.Append("for ");
                        var param0 = data.GetParamId(0);
                        sb.Append(param0);
                        sb.Append(" = ");
                        var param1 = data.GetParam(1);
                        GenerateSyntaxComponent(param1, sb, indent, false, funcOpts, calculator);
                        sb.Append(", ");
                        var param2 = data.GetParam(2);
                        GenerateSyntaxComponent(param2, sb, indent, false, funcOpts, calculator);
                        if (data.GetParamNum() > 3) {
                            sb.Append(", ");
                            var param3 = data.GetParam(3);
                            GenerateSyntaxComponent(param3, sb, indent, false, funcOpts, calculator);
                        }
                        sb.Append(" do");
                    }
                    else if (id == "dslcatch") {
                        sb.Append("luacatch(");
                        var param0 = data.GetParam(0);
                        GenerateSyntaxComponent(param0, sb, indent, false, funcOpts, calculator);
                        sb.Append(", ");
                        var param1 = data.GetParam(1);
                        GenerateSyntaxComponent(param1, sb, indent, false, funcOpts, calculator);
                        sb.Append(", (not ");
                        var param2 = data.GetParam(2);
                        GenerateSyntaxComponent(param2, sb, indent, false, funcOpts, calculator);
                        sb.Append(") and ");
                        GenerateArguments(data, sb, indent, 3, funcOpts, calculator);
                        sb.Append(")");
                    }
                    else {
                        if (id == "if") {
                            sb.Append("if ");
                        }
                        else if (id == "elseif") {
                            sb.Append("elseif ");
                        }
                        else if (id == "while") {
                            sb.Append("while ");
                        }
                        else if (id == "until") {
                            sb.Append("until ");
                        }
                        else if (id == "block") {
                            sb.Append("do");
                        }
                        else if (id == "lock") {
                            sb.Append("do");
                        }
                        else if (id == "unsafe") {
                            sb.Append("do");
                        }
                        else if (id == "dsltoobject") {
                            sb.Append("luatoobject");
                        }
                        else if (id == "objecttodsl") {
                            sb.Append("objecttolua");
                        }
                        else if (id == "dslstrtocsstr") {
                            sb.Append("luastrtocsstr");
                        }
                        else if (id == "dslunpack") {
                            sb.Append("luaunpack");
                        }
                        else if (id == "dslthrow") {
                            sb.Append("luathrow");
                        }
                        else if (!string.IsNullOrEmpty(id)) {
                            sb.Append(id);
                        }
                        if (data.HaveParam()) {
                            if (id == "if" || id == "elseif" || id == "while" || id == "until") {
                            }
                            else {
                                switch (data.GetParamClass()) {
                                    case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PARENTHESIS:
                                        sb.Append("(");
                                        break;
                                    case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_BRACKET:
                                        sb.Append("[");
                                        break;
                                    case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PERIOD:
                                        sb.AppendFormat(".{0}", data.GetParamId(0));
                                        break;
                                }
                            }
                            if (data.GetParamClass() != (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PERIOD) {
                                GenerateArguments(data, sb, indent, 0, funcOpts, calculator);
                            }
                            if (id == "if" || id == "elseif" || id == "while" || id == "until") {
                            }
                            else {
                                switch (data.GetParamClass()) {
                                    case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PARENTHESIS:
                                        sb.Append(")");
                                        break;
                                    case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_BRACKET:
                                        sb.Append("]");
                                        break;
                                    case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PERIOD:
                                        break;
                                }
                            }
                            if (id == "if" || id == "elseif") {
                                sb.Append(" then ");
                            }
                            else if (id == "while") {
                                sb.Append(" do");
                            }
                        }
                    }
                }
            }
        }
        private static void GenerateConcreteSyntax(Dsl.FunctionData data, StringBuilder sb, int indent, bool firstLineUseIndent, FunctionOptions funcOpts, DslExpression.DslCalculator calculator)
        {
            if (data.HaveParam()) {
                GenerateConcreteSyntaxForCall(data, sb, indent, firstLineUseIndent, funcOpts, calculator);
                return;
            }
            s_CurSyntax = data;
            if (firstLineUseIndent) {
                sb.AppendFormat("{0}", GetIndentString(indent));
            }
            var fcall = data;
            if (data.IsHighOrder) {
                fcall = data.LowerOrderFunction;
            }
            string id = string.Empty;
            if (!fcall.IsHighOrder) {
                id = fcall.GetId();
            }
            if (id == "comments") {
                foreach (var comp in data.Params) {
                    GenerateSyntaxComponent(comp, sb, indent, true, funcOpts, calculator);
                    sb.AppendLine();
                }
            }
            else if (id == "execclosure") {
                GenerateClosure(data, sb, indent, false, funcOpts, calculator);
            }
            else if (id == "foreacharray") {
                var varIndex = fcall.GetParamId(0);
                var varExp = fcall.GetParamId(1);
                var varName = fcall.GetParamId(2);
                var exp = fcall.GetParam(3);
                int rank;
                int.TryParse(fcall.GetParamId(4), out rank);
                bool isExtern = fcall.GetParamId(5) == "true";
                if (rank > 1) {
                    sb.AppendFormat("local {0} = ", varExp);
                    GenerateSyntaxComponent(exp, sb, indent, false, funcOpts, calculator);
                    sb.AppendLine(";");
                    for (int ix = 0; ix < rank; ++ix) {
                        sb.AppendFormat("{0}for ", GetIndentString(indent));
                        sb.Append(varIndex);
                        sb.AppendFormat("_{0} = 1, ", ix);
                        sb.Append(varExp);
                        sb.AppendFormat(":GetLength({0}) do", ix);
                        sb.AppendLine();
                    }
                    if (data.HaveStatement()) {
                        ++indent;
                        sb.AppendFormatLine("{0}{1} = {2}[{3}];", GetIndentString(indent), varName, varExp, varIndex);
                        GenerateStatements(data, sb, indent, funcOpts, calculator);
                        --indent;
                    }
                    for (int ix = 0; ix < rank; ++ix) {
                        if (ix == rank - 1)
                            sb.AppendFormat("{0}end", GetIndentString(indent));
                        else
                            sb.AppendFormatLine("{0}end;", GetIndentString(indent));
                    }
                }
                else {
                    sb.AppendFormat("local {0} = ", varExp);
                    GenerateSyntaxComponent(exp, sb, indent, false, funcOpts, calculator);
                    sb.AppendLine(";");
                    sb.AppendFormat("{0}for ", GetIndentString(indent));
                    sb.Append(varIndex);
                    sb.Append(" = 1, ");
                    sb.Append(varExp);
                    if (isExtern)
                        sb.Append(".Length do");
                    else
                        sb.Append(":get_Length() do");
                    sb.AppendLine();
                    if (data.HaveStatement()) {
                        ++indent;
                        sb.AppendFormatLine("{0}{1} = {2}[{3}];", GetIndentString(indent), varName, varExp, varIndex);
                        GenerateStatements(data, sb, indent, funcOpts, calculator);
                        --indent;
                    }
                    sb.AppendFormat("{0}end", GetIndentString(indent));
                }
            }
            else if (id == "foreachlist") {
                var varIndex = fcall.GetParamId(0);
                var varExp = fcall.GetParamId(1);
                var varName = fcall.GetParamId(2);
                var exp = fcall.GetParam(3);
                var _callerClass = fcall.GetParam(4);
                var _class = fcall.GetParam(5);
                var isExtern = fcall.GetParamId(6) == "true";
                var strCallerClass = CalcTypeString(_callerClass);
                var strObj = CalcExpressionString(exp, funcOpts, calculator);
                var strClass = CalcTypeString(_class);
                var strMember = "get_Item";
                bool indexerByLuaLib = false;
                int indexerType;
                if (IndexerByLualib(isExtern ? strCallerClass : "@@internal", strObj, strClass, strMember, out indexerType)) {
                    indexerByLuaLib = true;
                }
                sb.AppendFormat("local {0} = ", varExp);
                GenerateSyntaxComponent(exp, sb, indent, false, funcOpts, calculator);
                sb.AppendLine(";");
                sb.AppendFormat("{0}for ", GetIndentString(indent));
                sb.Append(varIndex);
                if (indexerType == (int)IndexerTypeEnum.LikeArray) {
                    sb.Append(" = 1, ");
                    sb.Append(varExp);
                    if (isExtern)
                        sb.Append(".Count do");
                    else
                        sb.Append(":get_Count() do");
                }
                else {
                    sb.Append(" = 0, ");
                    sb.Append(varExp);
                    if (isExtern)
                        sb.Append(".Count - 1 do");
                    else
                        sb.Append(":get_Count() - 1 do");
                }
                sb.AppendLine();
                if (data.HaveStatement()) {
                    ++indent;
                    if (indexerByLuaLib) {
                        sb.AppendFormatLine("{0}{1} = {2}[{3}];", GetIndentString(indent), varName, varExp, varIndex);
                    }
                    else if (isExtern) {
                        sb.AppendFormat("{0}{1} = getexterninstanceindexer({2}, {3}, {4}, \"{5}\", 1, {6});", GetIndentString(indent), varName, strCallerClass, varExp, strClass, strMember, varIndex);
                    }
                    else {
                        sb.AppendFormatLine("{0}{1} = {2}[{3}];", GetIndentString(indent), varName, varExp, varIndex);
                    }
                    GenerateStatements(data, sb, indent, funcOpts, calculator);
                    --indent;
                }
                sb.AppendFormat("{0}end", GetIndentString(indent));
            }
            else if (id == "foreach") {
                var expIter = fcall.GetParamId(0);
                var varName = fcall.GetParamId(1);
                var exp = fcall.GetParam(2);
                var _callerClass = fcall.GetParam(3);
                var _class = fcall.GetParam(4);
                var isExtern = fcall.GetParam(5);
                sb.AppendFormat("local {0} = newiterator(", expIter);
                GenerateArguments(fcall, sb, indent, 2, funcOpts, calculator);
                sb.AppendLine(");");
                sb.AppendFormat("{0}for ", GetIndentString(indent));
                sb.Append(varName);
                sb.Append(" in getiterator(");
                sb.Append(expIter);
                sb.Append(") do");
                if (data.HaveStatement()) {
                    sb.AppendLine();
                    ++indent;
                    sb.AppendFormatLine("{0}if {1} == __cs2lua_nil then", GetIndentString(indent), varName);
                    sb.AppendFormatLine("{0}{1} = nil;", GetIndentString(indent + 1), varName);
                    sb.AppendFormatLine("{0}end;", GetIndentString(indent));
                    GenerateStatements(data, sb, indent, funcOpts, calculator);
                    --indent;
                }
                sb.AppendFormatLine("{0}end;", GetIndentString(indent));
                sb.AppendFormat("{0}recycleiterator({1})", GetIndentString(indent), expIter);
            }
            else if (id == "while") {
                var condExp = fcall.GetParam(0);
                Dsl.FunctionData closure = null;
                Dsl.FunctionData oper = null;
                bool prefix = false;
                if (CanRemoveClosure(condExp, out closure) || CanSplitPrefixPostfixOperator(condExp, out oper, out prefix)) {
                    //TryGetValue这样的单一条件表达式可以转换为非匿名函数包装样式
                    sb.AppendLine("while true do");
                    ++indent;
                    if (null != closure) {
                        GenerateClosure(closure, sb, indent, true, funcOpts, calculator);
                        var p = closure.LowerOrderFunction.GetParam(0) as Dsl.ValueData;
                        p.SetId("false");
                    }
                    if (null != oper) {
                        sb.AppendFormat("{0}", GetIndentString(indent));
                        GeneratePrefixPostfixOperator(oper, sb, true, funcOpts, calculator);
                        sb.AppendLine(";");
                        var p = oper.GetParam(0) as Dsl.ValueData;
                        p.SetId("false");
                    }
                    sb.AppendFormat("{0}if not ", GetIndentString(indent));
                    GenerateSyntaxComponent(condExp, sb, 0, false, funcOpts, calculator);
                    sb.AppendLine(" then");
                    ++indent;
                    sb.AppendFormatLine("{0}break;", GetIndentString(indent));
                    --indent;
                    sb.AppendFormatLine("{0}end;", GetIndentString(indent));
                    --indent;
                }
                else {
                    GenerateConcreteSyntaxForCall(fcall, sb, indent, false, funcOpts, calculator);
                }
                if (data.HaveStatement()) {
                    sb.AppendLine();
                    ++indent;
                    GenerateStatements(data, sb, indent, funcOpts, calculator);
                    --indent;
                    sb.AppendFormat("{0}end", GetIndentString(indent));
                }
            }
            else if (id == "if") {
                var condExp = fcall.GetParam(0);
                Dsl.FunctionData closure = null;
                Dsl.FunctionData oper = null;
                bool prefix = false;
                if (CanRemoveClosure(condExp, out closure) || CanSplitPrefixPostfixOperator(condExp, out oper, out prefix)) {
                    //TryGetValue这样的单一条件表达式可以提到if语句外面
                    if (null != closure) {
                        sb.AppendLine("--");
                        GenerateClosure(closure, sb, indent, true, funcOpts, calculator);
                        var p = closure.LowerOrderFunction.GetParam(0) as Dsl.ValueData;
                        p.SetId("false");
                    }
                    if (null != oper) {
                        sb.AppendLine("--");
                        sb.AppendFormat("{0}", GetIndentString(indent));
                        GeneratePrefixPostfixOperator(oper, sb, true, funcOpts, calculator);
                        sb.AppendLine(";");
                        var p = oper.GetParam(0) as Dsl.ValueData;
                        p.SetId("false");
                    }
                    sb.AppendFormat("{0}if ", GetIndentString(indent));
                    GenerateSyntaxComponent(condExp, sb, 0, false, funcOpts, calculator);
                    sb.AppendLine(" then");
                }
                else {
                    GenerateConcreteSyntaxForCall(fcall, sb, indent, false, funcOpts, calculator);
                }
                if (data.HaveStatement()) {
                    sb.AppendLine();
                    ++indent;
                    GenerateStatements(data, sb, indent, funcOpts, calculator);
                    --indent;
                    sb.AppendFormat("{0}end", GetIndentString(indent));
                }
            }
            else if (id == "dslusing" || id == "dsltry") {
                if (id == "dslusing")
                    sb.Append("luausing");
                else
                    sb.Append("luatry");
                var complex = data.LowerOrderFunction.GetParamId(0) == "true";
                var retVar = data.LowerOrderFunction.GetParamId(1);
                if (complex) {
                    sb.Append("(function()");
                    sb.AppendLine();
                    //函数体部分
                    if (data.HaveStatement()) {
                        sb.AppendLine();
                        //赋值操作会++indent
                        //++indent;
                        GenerateStatements(data, sb, indent, funcOpts, calculator);
                        --indent;
                    }
                    sb.AppendFormat("{0}end)", GetIndentString(indent));
                }
                else {
                    //赋值操作会++indent，所以这里输出语句时会缩进一次，这样恰好能看出原来的try块的范围，先保持此格式
                    bool canSimplify = false;
                    if (data.GetParamNum() == 1) {
                        var bodyStatement = data.GetParam(0) as Dsl.FunctionData;
                        if (null != bodyStatement) {
                            var name = bodyStatement.GetId();
                            string funcRetVar = null;
                            string tryRetVar = null;
                            string tryRetVal = null;
                            if (!bodyStatement.IsHighOrder && (name == "callstatic" || name == "callexternstatic" || name == "callinstance" || name == "callexterninstance")) {
                                canSimplify = true;
                            }
                            else if (name == "block" && bodyStatement.GetParamNum() == 3) {
                                var funcCall = bodyStatement.GetParam(0) as Dsl.FunctionData;
                                var retValVar = bodyStatement.GetParam(1) as Dsl.FunctionData;
                                var breakStatement = bodyStatement.GetParam(2) as Dsl.ValueData;
                                if (null != funcCall && null != retValVar && null != breakStatement && funcCall.GetId() == "=" && retValVar.GetId() == "=" && breakStatement.GetId() == "break") {
                                    bodyStatement = funcCall.GetParam(1) as Dsl.FunctionData;
                                    if (null != bodyStatement && !bodyStatement.IsHighOrder) {
                                        name = bodyStatement.GetId();
                                        if (name == "callstatic" || name == "callexternstatic" || name == "callinstance" || name == "callexterninstance") {
                                            canSimplify = true;
                                            funcRetVar = funcCall.GetParamId(0);
                                            tryRetVar = retValVar.GetParamId(0);
                                            tryRetVal = retValVar.GetParamId(1);
                                        }
                                    }
                                }
                            }
                            if (canSimplify) {
                                sb.Append("(");
                                //只有一个函数调用的，正常模拟
                                if (name == "callstatic" || name == "callexternstatic") {
                                    var obj = bodyStatement.Params[0];
                                    var member = bodyStatement.Params[1];
                                    var mid = member.GetId();
                                    GenerateSyntaxComponent(obj, sb, indent, false, funcOpts, calculator);
                                    sb.AppendFormat(".{0}", mid);
                                    int start = 2;
                                    if (bodyStatement.GetParamNum() > start) {
                                        sb.Append(", ");
                                    }
                                    GenerateArguments(bodyStatement, sb, indent, start, funcOpts, calculator);
                                }
                                else if (name == "callinstance" || name == "callexterninstance") {
                                    var obj = bodyStatement.Params[0];
                                    var className = CalcTypeString(data.GetParam(1));
                                    var member = bodyStatement.Params[2];
                                    var mid = member.GetId();
                                    var objCd = obj as Dsl.FunctionData;
                                    GenerateSyntaxComponent(obj, sb, indent, false, funcOpts, calculator);
                                    if (null != objCd && objCd.GetId() == "getinstance" && objCd.GetParamNum() == 4 && objCd.GetParamId(3) == "base") {
                                        sb.AppendFormat(".__self__{0}", mid);
                                    }
                                    else {
                                        sb.AppendFormat(".{0}", mid);
                                    }
                                    sb.Append(", ");
                                    GenerateSyntaxComponent(obj, sb, indent, false, funcOpts, calculator);
                                    int start = 3;
                                    if (bodyStatement.GetParamNum() > start) {
                                        sb.Append(", ");
                                    }
                                    GenerateArguments(bodyStatement, sb, indent, start, funcOpts, calculator);
                                }
                                if (null == funcRetVar) {
                                    sb.Append(")");
                                }
                                else {
                                    sb.AppendLine(");");
                                    --indent;
                                    sb.AppendFormatLine("{0}if {1} then", GetIndentString(indent), retVar);
                                    ++indent;
                                    sb.AppendFormatLine("{0}{1} = {2};", GetIndentString(indent), funcRetVar, tryRetVar);
                                    sb.AppendFormatLine("{0}{1} = {2};", GetIndentString(indent), tryRetVar, tryRetVal);
                                    --indent;
                                    sb.AppendFormat("{0}end", GetIndentString(indent));
                                }
                            }
                        }
                    }
                    if (!canSimplify) {
                        //出于性能考虑（function对象有较大开销），忽略using与try-catch机制
                        sb.AppendLine("(nil);");
                        --indent;
                        sb.AppendFormatLine("{0}repeat", GetIndentString(indent));
                        ++indent;
                        GenerateStatements(data, sb, indent, funcOpts, calculator);
                        --indent;
                        sb.AppendFormat("{0}until true", GetIndentString(indent));
                    }
                }
            }
            else {
                //函数名与参数部分由另一方法生成（有可能没有参数）
                GenerateConcreteSyntaxForCall(fcall, sb, indent, false, funcOpts, calculator);
                //函数体部分
                if (data.HaveStatement()) {
                    sb.AppendLine();
                    ++indent;
                    GenerateStatements(data, sb, indent, funcOpts, calculator);
                    --indent;
                    sb.AppendFormat("{0}end", GetIndentString(indent));
                }
            }
        }
        private static void GenerateConcreteSyntax(Dsl.StatementData data, StringBuilder sb, int indent, bool firstLineUseIndent, FunctionOptions funcOpts, DslExpression.DslCalculator calculator)
        {
            s_CurSyntax = data;
            if (firstLineUseIndent) {
                sb.AppendFormat("{0}", GetIndentString(indent));
            }
            string id = data.GetId();
            if (id == "linq") {
                string prestr = string.Empty;
                foreach (var funcData in data.Functions) {
                    var fcall = funcData;
                    if (funcData.IsHighOrder)
                        fcall = funcData.LowerOrderFunction;
                    var linqId = fcall.GetId();
                    if (linqId == "linq") {
                        sb.Append("LINQ.exec({");
                    }
                    else if (linqId == "end") {
                        sb.Append("})");
                    }
                    else {
                        sb.AppendFormat("{0}{{\"{1}\"", prestr, linqId);
                        prestr = ", ";
                        ++indent;
                        foreach (var comp in fcall.Params) {
                            sb.Append(prestr);
                            GenerateSyntaxComponent(comp, sb, indent, false, funcOpts, calculator);
                        }
                        --indent;
                        sb.Append("}");
                    }
                }
            }
            else if (id == "do") {
                foreach (var funcData in data.Functions) {
                    var fcall = funcData;
                    if (funcData.IsHighOrder)
                        fcall = funcData.LowerOrderFunction;
                    if (funcData == data.First) {
                        if (firstLineUseIndent) {
                            sb.AppendLine("repeat");
                        }
                        else {
                            sb.AppendFormatLine("{0}repeat", GetIndentString(indent));
                        }
                    }
                    else if (funcData == data.Second) {
                        var condExp = fcall.GetParam(0);
                        Dsl.FunctionData closure = null;
                        Dsl.FunctionData oper = null;
                        bool prefix = false;
                        if (CanRemoveClosure(condExp, out closure) || CanSplitPrefixPostfixOperator(condExp, out oper, out prefix)) {
                            ++indent;
                            if (null != closure) {
                                GenerateClosure(closure, sb, indent, true, funcOpts, calculator);
                                var p = closure.LowerOrderFunction.GetParam(0) as Dsl.ValueData;
                                p.SetId("false");
                            }
                            if (null != oper) {
                                sb.AppendFormat("{0}", GetIndentString(indent));
                                GeneratePrefixPostfixOperator(oper, sb, true, funcOpts, calculator);
                                sb.AppendLine(";");
                                var p = oper.GetParam(0) as Dsl.ValueData;
                                p.SetId("false");
                            }
                            --indent;
                            sb.AppendFormat("{0}until not ", GetIndentString(indent));
                            GenerateSyntaxComponent(condExp, sb, 0, false, funcOpts, calculator);
                        }
                        else if (condExp.GetId() == "false") {
                            sb.AppendFormat("{0}until true", GetIndentString(indent));
                        }
                        else {
                            sb.AppendFormat("{0}until not ", GetIndentString(indent));
                            GenerateSyntaxComponent(condExp, sb, indent, false, funcOpts, calculator);
                        }
                    }
                    if (funcData.HaveStatement()) {
                        sb.AppendLine();
                        ++indent;
                        GenerateStatements(funcData, sb, indent, funcOpts, calculator);
                        --indent;
                        if (funcData == data.Last) {
                            sb.AppendFormat("{0}end", GetIndentString(indent));
                        }
                    }
                    else {
                        sb.Append(" ");
                    }
                }
            }
            else if (id == "if") {
                var fdata = data.First;
                var fcall = fdata.LowerOrderFunction;
                var condExp = fcall.GetParam(0);
                Dsl.FunctionData closure = null;
                Dsl.FunctionData oper = null;
                bool prefix = false;
                if (CanRemoveClosure(condExp, out closure) || CanSplitPrefixPostfixOperator(condExp, out oper, out prefix)) {
                    //TryGetValue这样的单一条件表达式可以提到if语句外面
                    if (null != closure) {
                        sb.AppendLine("--");
                        GenerateClosure(closure, sb, indent, true, funcOpts, calculator);
                        var p = closure.LowerOrderFunction.GetParam(0) as Dsl.ValueData;
                        p.SetId("false");
                    }
                    if (null != oper) {
                        sb.AppendLine("--");
                        sb.AppendFormat("{0}", GetIndentString(indent));
                        GeneratePrefixPostfixOperator(oper, sb, true, funcOpts, calculator);
                        sb.AppendLine(";");
                        var p = oper.GetParam(0) as Dsl.ValueData;
                        p.SetId("false");
                    }
                    sb.AppendFormat("{0}if ", GetIndentString(indent));
                    GenerateSyntaxComponent(condExp, sb, 0, false, funcOpts, calculator);
                    sb.AppendLine(" then");
                }
                else {
                    GenerateConcreteSyntaxForCall(fcall, sb, indent, false, funcOpts, calculator);
                }
                if (fdata.HaveStatement()) {
                    sb.AppendLine();
                    ++indent;
                    GenerateStatements(fdata, sb, indent, funcOpts, calculator);
                    --indent;
                }
                if (data.GetFunctionNum() == 1) {
                    sb.AppendFormat("{0}end", GetIndentString(indent));
                }
                else {
                    for (int ix = 1; ix < data.GetFunctionNum(); ++ix) {
                        var funcData = data.GetFunction(ix);
                        var fcd = funcData;
                        if (funcData.IsHighOrder) {
                            fcd = funcData.LowerOrderFunction;
                            var elseCondExp = fcd.GetParam(0);
                            if (CanRemoveClosure(elseCondExp) || CanSplitPrefixPostfixOperator(elseCondExp)) {
                                for (int i = 0; i < ix; ++i) {
                                    data.Functions.RemoveAt(0);
                                }
                                data.First.LowerOrderFunction.Name.SetId("if");
                                sb.AppendFormatLine("{0}else", GetIndentString(indent));
                                ++indent;
                                GenerateConcreteSyntax(data, sb, indent, true, funcOpts, calculator);
                                --indent;
                                sb.AppendLine(";");
                                sb.AppendFormat("{0}end", GetIndentString(indent));
                                break;
                            }
                        }
                        GenerateConcreteSyntaxForCall(fcd, sb, indent, funcData == data.First ? false : true, funcOpts, calculator);
                        if (funcData.HaveStatement()) {
                            sb.AppendLine();
                            ++indent;
                            GenerateStatements(funcData, sb, indent, funcOpts, calculator);
                            --indent;
                            if (funcData == data.Last) {
                                sb.AppendFormat("{0}end", GetIndentString(indent));
                            }
                        }
                        else {
                            sb.Append(" ");
                        }
                    }
                }
            }
            else if (id == "deffunc") {
                var fdef = data;
                if (null != fdef && fdef.GetFunctionNum() >= 2) {
                    var first = fdef.First;
                    var second = fdef.Second;
                    var newFuncOpts = new FunctionOptions();
                    if (fdef.GetFunctionNum() >= 3) {
                        var opts = fdef.Third;
                        ParseFunctionOptions(opts, newFuncOpts);
                    }
                    int rct;
                    int.TryParse(first.GetParamId(0), out rct);
                    if (second.HaveStatement()) {
                        var fcall = second;
                        if (second.IsHighOrder)
                            fcall = second.LowerOrderFunction;
                        sb.Append("function(");
                        bool haveParams = GenerateFunctionParams(fcall, sb);
                        sb.AppendLine(")");
                        ++indent;
                        if (newFuncOpts.NeedFuncInfo) {
                            sb.AppendFormatLine("{0}local __cs2lua_func_info = luainitialize();", GetIndentString(indent));
                        }
                        GenerateStatements(second, sb, indent, newFuncOpts, calculator);
                        bool lastIsNotReturn = true;
                        int snum = second.GetParamNum();
                        for (; snum > 0; --snum) {
                            var cid = second.GetParamId(snum - 1);
                            if (cid != "comment" && cid != "comments")
                                break;
                        }
                        if (snum > 0) {
                            lastIsNotReturn = second.GetParamId(snum - 1) != "return";
                        }
                        if (rct <= 0 && lastIsNotReturn) {
                            if (newFuncOpts.NeedFuncInfo) {
                                sb.AppendFormatLine("{0}__cs2lua_func_info = luafinalize(__cs2lua_func_info);", GetIndentString(indent));
                            }
                        }
                        --indent;
                        sb.AppendFormat("{0}end", GetIndentString(indent));
                    }
                }
            }
            else {
                foreach (var funcData in data.Functions) {
                    var fcall = funcData;
                    if (funcData.IsHighOrder)
                        fcall = funcData.LowerOrderFunction;
                    GenerateConcreteSyntaxForCall(fcall, sb, indent, funcData == data.First ? false : true, funcOpts, calculator);
                    if (funcData.HaveStatement()) {
                        sb.AppendLine();
                        ++indent;
                        GenerateStatements(funcData, sb, indent, funcOpts, calculator);
                        --indent;
                        if (funcData == data.Last) {
                            sb.AppendFormat("{0}end", GetIndentString(indent));
                        }
                    }
                    else {
                        sb.Append(" ");
                    }
                }
            }
        }
        private static void GenerateAssignmentCondExp(Dsl.ISyntaxComponent leftParam, Dsl.FunctionData data, StringBuilder sb, int indent, bool firstLineUseIndent, FunctionOptions funcOpts, DslExpression.DslCalculator calculator)
        {
            var p1 = data.GetParam(0);
            var p2 = data.GetParamId(1);
            var p3 = data.GetParam(2);
            var p3Func = p3 as Dsl.FunctionData;
            var p4 = data.GetParamId(3);
            var p5 = data.GetParam(4);
            var p5Func = p5 as Dsl.FunctionData;
            string p2n = p2;
            var p3n = p3;
            string p4n = p4;
            var p5n = p5;
            bool p3IsCondExp = false;
            bool p5IsCondExp = false;
            if (p2 == "false" && null != p3Func && p3Func.GetId() == "condexp") {
                p3IsCondExp = true;
            }
            else if (p2 == "false" && null != p3Func && !ExistEmbedFunctionObject(p3Func)) {
                var func = p3Func.GetParam(0) as Dsl.FunctionData;
                if (null != func && !func.HaveId() && func.GetParamNum() == 1) {
                    func = func.GetParam(0) as Dsl.FunctionData;
                }
                if (null != func && func.GetId() == "funcobjret") {
                    p2n = "true";
                    p3n = func.GetParam(0);
                }
            }
            if (p4 == "false" && null != p5Func && p5Func.GetId() == "condexp") {
                p5IsCondExp = true;
            }
            else if (p4 == "false" && null != p5Func && !ExistEmbedFunctionObject(p5Func)) {
                var func = p5Func.GetParam(0) as Dsl.FunctionData;
                if (null != func && !func.HaveId() && func.GetParamNum() == 1) {
                    func = func.GetParam(0) as Dsl.FunctionData;
                }
                if (null != func && func.GetId() == "funcobjret") {
                    p4n = "true";
                    p5n = func.GetParam(0);
                }
            }

            if (firstLineUseIndent) {
                sb.AppendFormat("{0}", GetIndentString(indent));
            }
            sb.Append("if ");
            GenerateSyntaxComponent(p1, sb, indent, false, funcOpts, calculator);
            sb.Append(" then ");
            if (p3IsCondExp) {
                GenerateAssignmentCondExp(leftParam, p3Func, sb, indent, false, funcOpts, calculator);
            }
            else if (p2n == "true") {
                GenerateSyntaxComponent(leftParam, sb, indent, false, funcOpts, calculator);
                sb.Append(" = ");
                GenerateSyntaxComponent(p3n, sb, indent, false, funcOpts, calculator);
            }
            else {
                GenerateSyntaxComponent(leftParam, sb, indent, false, funcOpts, calculator);
                sb.Append(" = (");
                GenerateSyntaxComponent(p3, sb, indent, false, funcOpts, calculator);
                sb.Append(")()");
            }
            sb.Append(" else ");
            if (p5IsCondExp) {
                GenerateAssignmentCondExp(leftParam, p5Func, sb, indent, false, funcOpts, calculator);
            }
            else if (p4n == "true") {
                GenerateSyntaxComponent(leftParam, sb, indent, false, funcOpts, calculator);
                sb.Append(" = ");
                GenerateSyntaxComponent(p5n, sb, indent, false, funcOpts, calculator);
            }
            else {
                GenerateSyntaxComponent(leftParam, sb, indent, false, funcOpts, calculator);
                sb.Append(" = (");
                GenerateSyntaxComponent(p5, sb, indent, false, funcOpts, calculator);
                sb.Append(")()");
            }
            sb.Append(" end");
        }
        private static void GenerateOtherCondExp(Dsl.FunctionData data, StringBuilder sb, int indent, FunctionOptions funcOpts, DslExpression.DslCalculator calculator)
        {
            var p1 = data.GetParam(0);
            var p2 = data.GetParamId(1);
            var p3 = data.GetParam(2);
            var p3Func = p3 as Dsl.FunctionData;
            var p4 = data.GetParamId(3);
            var p5 = data.GetParam(4);
            var p5Func = p5 as Dsl.FunctionData;
            string p2n = p2;
            var p3n = p3;
            string p4n = p4;
            var p5n = p5;
            bool p3IsCondExp = false;
            bool p5IsCondExp = false;
            if (p2 == "false" && null != p3Func && p3Func.GetId() == "condexp") {
                p3IsCondExp = true;
            }
            else if (p2 == "false" && null != p3Func && !ExistEmbedFunctionObject(p3Func)) {
                var func = p3Func.GetParam(0) as Dsl.FunctionData;
                if (null != func && !func.HaveId() && func.GetParamNum() == 1) {
                    func = func.GetParam(0) as Dsl.FunctionData;
                }
                if (null != func && func.GetId() == "funcobjret") {
                    p2n = "true";
                    p3n = func.GetParam(0);
                }
            }
            if (p4 == "false" && null != p5Func && p5Func.GetId() == "condexp") {
                p5IsCondExp = true;
            }
            else if (p4 == "false" && null != p5Func && !ExistEmbedFunctionObject(p5Func)) {
                var func = p5Func.GetParam(0) as Dsl.FunctionData;
                if (null != func && !func.HaveId() && func.GetParamNum() == 1) {
                    func = func.GetParam(0) as Dsl.FunctionData;
                }
                if (null != func && func.GetId() == "funcobjret") {
                    p4n = "true";
                    p5n = func.GetParam(0);
                }
            }

            sb.Append("if ");
            GenerateSyntaxComponent(p1, sb, indent, false, funcOpts, calculator);
            sb.Append(" then ");
            if (p3IsCondExp) {
                GenerateOtherCondExp(p3Func, sb, indent, funcOpts, calculator);
            }
            else if (p2n == "true") {
                sb.Append("return ");
                GenerateSyntaxComponent(p3n, sb, indent, false, funcOpts, calculator);
            }
            else {
                sb.Append("return (");
                GenerateSyntaxComponent(p3, sb, indent, false, funcOpts, calculator);
                sb.Append(")()");
            }
            sb.Append(" else ");
            if (p5IsCondExp) {
                GenerateOtherCondExp(p5Func, sb, indent, funcOpts, calculator);
            }
            else if (p4n == "true") {
                sb.Append("return ");
                GenerateSyntaxComponent(p5n, sb, indent, false, funcOpts, calculator);
            }
            else {
                sb.Append("return (");
                GenerateSyntaxComponent(p5, sb, indent, false, funcOpts, calculator);
                sb.Append(")()");
            }
            sb.Append(" end");
        }
        private static void GenerateAttribute(Dsl.ISyntaxComponent comp, StringBuilder sb, int indent)
        {
            s_CurSyntax = comp;
            string prestr = string.Empty;
            var cd = comp as Dsl.FunctionData;
            if (null != cd) {
                var attrCd = cd.GetParam(0) as Dsl.FunctionData;
                string attr;
                if (attrCd.IsHighOrder) {
                    attr = CalcTypeString(attrCd.LowerOrderFunction);
                }
                else {
                    attr = attrCd.GetId();
                }
                sb.AppendFormat("{0}{{\"{1}\", {{", GetIndentString(indent), attr);
                var posAttrs = attrCd.GetParam(0) as Dsl.FunctionData;
                foreach (var p in posAttrs.Params) {
                    var pcd = p as Dsl.FunctionData;
                    var k = pcd.GetId();
                    var v = pcd.GetParamId(0);
                    sb.Append(prestr);
                    sb.AppendFormat("{0} = \"{1}\"", k, v);
                    prestr = ", ";
                }
                sb.Append("}, {");
                var nameAttrs = attrCd.GetParam(1) as Dsl.FunctionData;
                foreach (var p in nameAttrs.Params) {
                    var pcd = p as Dsl.FunctionData;
                    var k = pcd.GetId();
                    var v = pcd.GetParamId(0);
                    sb.Append(prestr);
                    sb.AppendFormat("{0} = \"{1}\"", k, v);
                    prestr = ", ";
                }
                sb.AppendLine("}};");
            }
        }
        private static void GenerateStatements(Dsl.FunctionData data, StringBuilder sb, int indent, FunctionOptions funcOpts, DslExpression.DslCalculator calculator)
        {
            s_CurSyntax = data;
            foreach (var comp in data.Params) {
                GenerateStatement(comp, sb, indent, funcOpts, calculator);
                string subId = comp.GetId();
                if (subId != "comments" && subId != "comment") {
                    sb.AppendLine(";");
                }
                else {
                    sb.AppendLine();
                }
            }
        }
    }
}
namespace DslExpression
{
    internal sealed class GetArgmentExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            string r = string.Empty;
            if (operands.Count >= 2) {
                var data = operands[0].As<Dsl.FunctionData>();
                var index = operands[1].Get<int>();
                if (null != data && index >= 0 && index < data.GetParamNum()) {
                    var arg = data.GetParam(index);
                    r = Generator.LuaGenerator.CalcTypeString(arg);
                }
            }
            return r;
        }
    }
    internal sealed class WriteIndentExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            bool r = false;
            if (operands.Count >= 2) {
                var sb = operands[0].As<StringBuilder>();
                var indent = operands[1].Get<int>();
                if (null != sb && indent >= 0) {
                    string str = Generator.LuaGenerator.GetIndentString(indent);
                    sb.Append(str);
                    r = true;
                }
            }
            return r;
        }
    }
    internal sealed class WriteSymbolExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            bool r = false;
            if (operands.Count >= 2) {
                var sb = operands[0].As<StringBuilder>();
                var name = operands[1].Get<string>();
                if (null != sb && null != name) {
                    sb.Append(name);
                }
            }
            return r;
        }
    }
    internal sealed class WriteStringExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            bool r = false;
            if (operands.Count >= 2) {
                var sb = operands[0].As<StringBuilder>();
                var str = operands[1].Get<string>();
                if (null != sb && null != str) {
                    sb.AppendFormat("\"{0}\"", Generator.LuaGenerator.Escape(str));
                }
            }
            return r;
        }
    }
    internal sealed class WriteArgumentsExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            bool r = false;
            if (operands.Count >= 3) {
                var sb = operands[0].As<StringBuilder>();
                var funcData = operands[1].As<Dsl.FunctionData>();
                var funcOpts = operands[2].As<Generator.LuaGenerator.FunctionOptions>();
                int indent = 0;
                int start = 0;
                List<int> iargs = null;
                if (operands.Count >= 4)
                    indent = operands[3].Get<int>();
                if (operands.Count >= 5) {
                    var opd = operands[4];
                    if (opd.IsInteger) {
                        start = opd.Get<int>();
                    }
                    else {
                        var enumer = opd.As<System.Collections.IEnumerable>();
                        if (null != enumer) {
                            iargs = new List<int>();
                            foreach (var e in enumer) {
                                var v = CastTo<int>(e);
                                iargs.Add(v);
                            }
                        }
                        else {
                            throw new Exception("illegel IEnumerable arg !");
                        }
                    }
                }
                if (null != sb && null != funcData) {
                    if (null == iargs)
                        Generator.LuaGenerator.GenerateArguments(funcData, sb, indent, start, funcOpts, Calculator);
                    else
                        Generator.LuaGenerator.GenerateArguments(funcData, sb, indent, iargs, funcOpts, Calculator);
                }
            }
            return r;
        }
    }
    internal sealed class UseFunctionExp : AbstractExpression
    {
        protected override CalculatorValue DoCalc()
        {
            bool r = false;
            if (null != m_NameExp) {
                var name = m_NameExp.Calc().AsString;
                var pstr = m_ParamsStrExp.Calc().AsString;
                var code = m_CodeBlock;
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(pstr) && !string.IsNullOrEmpty(code)) {
                    var tsb = new StringBuilder();
                    tsb.AppendFormat("function {0}{1}", name, pstr);
                    tsb.AppendLine();
                    Generator.LuaGenerator.GenerateCodeBlock(tsb, 1, code);
                    tsb.AppendLine("end");
                    Generator.LuaGenerator.FunctionsFromDslHook.TryAdd(name, tsb.ToString());
                    if (null != m_FuncDataExp && null != m_FuncOptsExp && null != m_StringBuilderExp && null != m_IndentExp && null != m_StartOrIgnoreArgs) {
                        var funcData = m_FuncDataExp.Calc().As<FunctionData>();
                        var funcOpts = m_FuncOptsExp.Calc().As<Generator.LuaGenerator.FunctionOptions>();
                        var sb = m_StringBuilderExp.Calc().As<StringBuilder>();
                        int indent = m_IndentExp.Calc().Get<int>();
                        var opd = m_StartOrIgnoreArgs.Calc();
                        int start = 0;
                        List<int> iargs = null;
                        if (opd.IsInteger) {
                            start = opd.Get<int>();
                        }
                        else {
                            var enumer = opd.As<System.Collections.IEnumerable>();
                            if (null != enumer) {
                                iargs = new List<int>();
                                foreach (var e in enumer) {
                                    int v = CastTo<int>(e);
                                    iargs.Add(v);
                                }
                            }
                            else {
                                throw new Exception("illegel IEnumerable arg !");
                            }
                        }
                        sb.Append(name);
                        sb.Append("(");
                        string prestr = string.Empty;
                        foreach (var fp in m_FirstArgs) {
                            sb.Append(prestr);
                            var str = fp.Calc().AsString;
                            if (!string.IsNullOrEmpty(str)) {
                                sb.Append(str);
                            }
                            else {
                                sb.Append("nil");
                            }
                            prestr = ", ";
                        }
                        if (null == iargs) {
                            if(start < funcData.GetParamNum()) {
                                sb.Append(prestr);
                            }
                            Generator.LuaGenerator.GenerateArguments(funcData, sb, indent, start, funcOpts, Calculator);
                        }
                        else {
                            if (iargs.Count < funcData.GetParamNum()) {
                                sb.Append(prestr);
                            }
                            Generator.LuaGenerator.GenerateArguments(funcData, sb, indent, iargs, funcOpts, Calculator);
                        }
                        sb.Append(")");
                    }
                    r = true;
                }
            }
            return r;
        }
        protected override bool Load(FunctionData funcData)
        {
            if (funcData.IsHighOrder) {
                var cd = funcData.LowerOrderFunction;
                int num = cd.GetParamNum();
                if (num >= 2) {
                    var name = cd.GetParam(0);
                    m_NameExp = Calculator.Load(name);
                    var pstr = cd.GetParam(1);
                    m_ParamsStrExp = Calculator.Load(pstr);
                    if (num >= 7) {
                        var fd = cd.GetParam(2);
                        m_FuncDataExp = Calculator.Load(fd);
                        var funcOpts = cd.GetParam(3);
                        m_FuncOptsExp = Calculator.Load(funcOpts);
                        var sb = cd.GetParam(4);
                        m_StringBuilderExp = Calculator.Load(sb);
                        var indent = cd.GetParam(5);
                        m_IndentExp = Calculator.Load(indent);
                        var siargs = cd.GetParam(6);
                        m_StartOrIgnoreArgs = Calculator.Load(siargs);
                        for (int ix = 7; ix < num; ++ix) {
                            var p = cd.GetParam(ix);
                            var exp = Calculator.Load(p);
                            m_FirstArgs.Add(exp);
                        }
                    }
                }
            }
            if (funcData.HaveExternScript()) {
                m_CodeBlock = funcData.GetParamId(0);
            }
            return true;
        }
        private IExpression m_NameExp = null;
        private IExpression m_ParamsStrExp = null;
        private IExpression m_FuncDataExp = null;
        private IExpression m_FuncOptsExp = null;
        private IExpression m_StringBuilderExp = null;
        private IExpression m_IndentExp = null;
        private IExpression m_StartOrIgnoreArgs = null;
        private List<IExpression> m_FirstArgs = new List<IExpression>();
        private string m_CodeBlock = string.Empty;
    }
}

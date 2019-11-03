using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Generator
{
    internal class JsGenerator
    {
        internal static void Generate(string csprojPath, string outPath, string ext)
        {
            if (string.IsNullOrEmpty(outPath)) {
                outPath = Path.Combine(csprojPath, "dsl");
            } else if (!Path.IsPathRooted(outPath)) {
                outPath = Path.Combine(csprojPath, outPath);
            }

            s_ExePath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            s_SrcPath = Path.Combine(csprojPath, "dsl");
            s_LogPath = Path.Combine(csprojPath, "log");
            s_OutPath = outPath;
            s_Ext = ext;
            if (!Directory.Exists(s_OutPath)) {
                Directory.CreateDirectory(s_OutPath);
            }
            var files = Directory.GetFiles(s_SrcPath, "*.dsl", SearchOption.TopDirectoryOnly);
            foreach (string file in files) {
                try {
                    Dsl.DslFile dslFile = new Dsl.DslFile();
                    dslFile.Load(file, s => Log(file, s));
                    GenerateJs(dslFile, Path.Combine(s_OutPath, Path.ChangeExtension(file.Replace("cs2dsl__", "cs2js__"), s_Ext)));
                } catch (Exception ex) {
                    Log(file, string.Format("exception:{0}\n{1}", ex.Message, ex.StackTrace));
                    File.WriteAllText(Path.Combine(s_LogPath, "Generator.log"), s_LogBuilder.ToString());
                    System.Environment.Exit(-1);
                }
            }
            File.WriteAllText(Path.Combine(s_LogPath, "Generator.log"), s_LogBuilder.ToString());
            System.Environment.Exit(0);
        }
        private static void GenerateJs(Dsl.DslFile dslFile, string outputFile)
        {
            StringBuilder sb = new StringBuilder();
            string prestr = string.Empty;
            int indent = 0;
            foreach (var dslInfo in dslFile.DslInfos) {
                string id = dslInfo.GetId();
                var funcData = dslInfo.First;
                var callData = funcData.Call;
                if (id == "require") {
                    sb.AppendFormatLine("{0}require(\"{1}.js\");", GetIndentString(indent), callData.GetParamId(0).Replace("cs2dsl__", "cs2js__"));
                } else if (id == "enum") {

                } else if (id == "class" || id == "struct") {
                    string className = callData.GetParamId(0);
                    var baseClass = callData.GetParam(1);

                    sb.AppendLine();
                    
                    sb.AppendFormatLine("{0}function {1}(){{", GetIndentString(indent), className);
                    ++indent;
                    if (null != baseClass) {
                        GenerateSyntaxComponent(baseClass, sb, indent, true, 0);
                        sb.AppendLine(".call(this);");
                        sb.AppendLine();
                    }
                    var instMethods = FindStatement(funcData, "instance_methods") as Dsl.FunctionData;
                    if (null != instMethods) {
                        foreach (var def in instMethods.Statements) {
                            var mdef = def as Dsl.CallData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                var fdef = mdef.GetParam(1) as Dsl.FunctionData;
                                if (null != fdef) {
                                    var fcall = fdef.Call;
                                    int paramsStart = 0;
                                    sb.AppendFormat("{0}this.{1} = {2}(", GetIndentString(indent), mname, fcall.GetId());
                                    prestr = string.Empty;
                                    for (int ix = 1; ix < fcall.Params.Count;++ix ) {
                                        var param = fcall.Params[ix];
                                        string paramId = param.GetId();
                                        if (paramId == "...") {
                                            paramsStart = ix - 1;
                                            continue;
                                        }
                                        sb.Append(prestr);
                                        prestr = ", ";
                                        var pv = param as Dsl.ValueData;
                                        if (null != pv) {
                                            sb.Append(paramId);
                                        } else {
                                            var pc = param as Dsl.CallData;
                                            if (null != pc) {
                                                sb.Append(pc.GetParamId(0));
                                            }
                                        }
                                    }
                                    sb.AppendLine("){");
                                    ++indent;
                                    foreach (var comp in fdef.Statements) {
                                        GenerateSyntaxComponent(comp, sb, indent, true, paramsStart);
                                        string subId = comp.GetId();
                                        if (subId != "comments" && subId != "comment") {
                                            sb.AppendLine(";");
                                        } else {
                                            sb.AppendLine();
                                        }
                                    }
                                    --indent;
                                    sb.AppendFormatLine("{0}}}", GetIndentString(indent));
                                }
                            }
                        }
                    }
                    var instFields = FindStatement(funcData, "instance_fields") as Dsl.FunctionData;
                    if (null != instFields) {
                        foreach (var def in instFields.Statements) {
                            var mdef = def as Dsl.CallData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                var comp = mdef.GetParam(1);
                                sb.AppendFormat("{0}this.{1} = ", GetIndentString(indent), mname);
                                GenerateSyntaxComponent(comp, sb, indent, false, 0);
                                sb.AppendLine(";");
                            }
                        }
                    }
                    --indent;
                    sb.AppendFormatLine("{0}}}", GetIndentString(indent));

                    sb.AppendLine();
                    
                    sb.AppendFormatLine("{0}(function(){{", GetIndentString(indent));
                    ++indent;
                    var staticMethods = FindStatement(funcData, "static_methods") as Dsl.FunctionData;
                    if (null != staticMethods) {
                        foreach (var def in staticMethods.Statements) {
                            var mdef = def as Dsl.CallData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                var fdef = mdef.GetParam(1) as Dsl.FunctionData;
                                if (null != fdef) {
                                    int paramsStart = 0;
                                    var fcall = fdef.Call;
                                    sb.AppendFormat("{0}{1}.{2} = {3}(", GetIndentString(indent), className, mname, fcall.GetId());
                                    prestr = string.Empty;
                                    for (int ix = 0; ix < fcall.Params.Count; ++ix) {
                                        var param = fcall.Params[ix];
                                        string paramId = param.GetId();
                                        if (paramId == "...") {
                                            paramsStart = ix;
                                            continue;
                                        }
                                        sb.Append(prestr);
                                        prestr = ", ";
                                        var pv = param as Dsl.ValueData;
                                        if (null != pv) {
                                            sb.Append(pv.GetId());
                                        } else {
                                            var pc = param as Dsl.CallData;
                                            if (null != pc) {
                                                sb.Append(pc.GetParamId(0));
                                            }
                                        }
                                    }
                                    sb.AppendLine("){");
                                    ++indent;
                                    foreach (var comp in fdef.Statements) {
                                        GenerateSyntaxComponent(comp, sb, indent, true, paramsStart);
                                        string subId = comp.GetId();
                                        if (subId != "comments" && subId != "comment") {
                                            sb.AppendLine(";");
                                        } else {
                                            sb.AppendLine();
                                        }
                                    }
                                    --indent;
                                    sb.AppendFormatLine("{0}}}", GetIndentString(indent));
                                }
                            }
                        }
                    }
                    var staticFields = FindStatement(funcData, "static_fields") as Dsl.FunctionData;
                    if (null != staticFields) {
                        foreach (var def in staticFields.Statements) {
                            var mdef = def as Dsl.CallData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                var comp = mdef.GetParam(1);
                                sb.AppendFormat("{0}{1}.{2} = ", GetIndentString(indent), className, mname);
                                GenerateSyntaxComponent(comp, sb, indent, false, 0);
                                sb.AppendLine(";");
                            }
                        }
                    }
                    --indent;
                    sb.AppendFormatLine("{0}}})()", GetIndentString(indent));
                }
            }
            File.WriteAllText(outputFile, sb.ToString());
        }
        private static void GenerateSyntaxComponent(Dsl.ISyntaxComponent comp, StringBuilder sb, int indent, bool firstLineUseIndent, int paramsStart)
        {
            var valData = comp as Dsl.ValueData;
            if (null != valData) {
                GenerateConcreteSyntax(valData, sb, indent, firstLineUseIndent, paramsStart);
            } else {
                var callData = comp as Dsl.CallData;
                if (null != callData) {
                    GenerateConcreteSyntax(callData, sb, indent, firstLineUseIndent, paramsStart);
                } else {
                    var funcData = comp as Dsl.FunctionData;
                    if (null != funcData) {
                        GenerateConcreteSyntax(funcData, sb, indent, firstLineUseIndent, paramsStart);
                    } else {
                        var statementData = comp as Dsl.StatementData;
                        GenerateConcreteSyntax(statementData, sb, indent, firstLineUseIndent, paramsStart);
                    }
                }
            }
        }
        private static void GenerateConcreteSyntax(Dsl.ValueData data, StringBuilder sb, int indent, bool firstLineUseIndent, int paramsStart)
        {
            if (firstLineUseIndent) {
                sb.AppendFormat("{0}", GetIndentString(indent));
            }
            string id = data.GetId();
            switch (data.GetIdType()) {
                case (int)Dsl.ValueData.ID_TOKEN:
                case (int)Dsl.ValueData.NUM_TOKEN:
                case (int)Dsl.ValueData.BOOL_TOKEN:
                    sb.Append(id);
                    break;
                case (int)Dsl.ValueData.STRING_TOKEN:
                    sb.AppendFormat("\"{0}\"", Escape(id));
                    break;
            }
        }
        private static void GenerateConcreteSyntax(Dsl.CallData data, StringBuilder sb, int indent, bool firstLineUseIndent, int paramsStart)
        {
            string id = string.Empty;
            var callData = data.Call;
            if (null == callData) {
                id = data.GetId();
            }
            if (firstLineUseIndent) {
                sb.AppendFormat("{0}", GetIndentString(indent));
            }
            if (data.GetParamClass() == (int)Dsl.CallData.ParamClassEnum.PARAM_CLASS_OPERATOR) {
                int paramNum = data.GetParamNum();
                if (paramNum == 1) {
                    var param1 = data.GetParam(0);
                    sb.AppendFormat("({0} ", id);
                    GenerateSyntaxComponent(param1, sb, indent, false, paramsStart);
                    sb.Append(")");
                } else if (paramNum == 2) {
                    var param1 = data.GetParam(0);
                    var param2 = data.GetParam(1);
                    bool handled = false;
                    if (id == "=" && param1.GetId() == "multiassign") {
                        var cd = param1 as Dsl.CallData;
                        if (null != cd) {
                            if (cd.GetParamNum() > 1) {
                                string varName = string.Format("__multiassign_{0}", data.GetLine());
                                sb.AppendFormat("var {0}", varName);
                                sb.AppendFormat(" {0} ", id);
                                GenerateSyntaxComponent(param2, sb, indent, false, paramsStart);
                                sb.Append(";");
                                int varNum = cd.GetParamNum();
                                for (int i = 0; i < varNum; ++i) {
                                    var parami = cd.GetParam(i);
                                    GenerateSyntaxComponent(parami, sb, indent, false, paramsStart);
                                    sb.AppendFormat(" = {0}[{1}]", varName, i);
                                    if (i < varNum - 1) {
                                        sb.Append(";");
                                    }
                                }
                            } else {
                                GenerateSyntaxComponent(cd.GetParam(0), sb, indent, false, paramsStart);
                                sb.AppendFormat(" {0} ", id);
                                GenerateSyntaxComponent(param2, sb, indent, false, paramsStart);
                            }
                            handled = true;
                        }
                    }
                    if (!handled) {
                        if (id != "=")
                            sb.Append("(");
                        GenerateSyntaxComponent(param1, sb, indent, false, paramsStart);
                        sb.AppendFormat(" {0} ", id);
                        GenerateSyntaxComponent(param2, sb, indent, false, paramsStart);
                        if (id != "=")
                            sb.Append(")");
                    }
                }
            } else if (id == "comment") {
                sb.AppendFormat("//{0}", data.GetParamId(0));
            } else if (id == "local") {
                sb.Append("var ");
                string prestr = string.Empty;
                foreach (var param in data.Params) {
                    sb.Append(prestr);
                    GenerateSyntaxComponent(param, sb, indent, false, paramsStart);
                    prestr = ", ";
                }
            } else if (id == "return") {
                sb.Append("return ");
                if (data.GetParamNum() > 1)
                    sb.Append("[");
                string prestr = string.Empty;
                foreach (var param in data.Params) {
                    sb.Append(prestr);
                    GenerateSyntaxComponent(param, sb, indent, false, paramsStart);
                    prestr = ", ";
                }
                if (data.GetParamNum() > 1)
                    sb.Append("]");
            } else if (id == "execunary") {
                string op = data.GetParamId(0);
                sb.AppendFormat("{0} ", op);
                GenerateSyntaxComponent(data.GetParam(1), sb, indent, false, paramsStart);
            } else if (id == "execbinary") {
                string op = data.GetParamId(0);
                GenerateSyntaxComponent(data.GetParam(1), sb, indent, false, paramsStart);
                sb.AppendFormat(" {0} ", op);
                GenerateSyntaxComponent(data.GetParam(2), sb, indent, false, paramsStart);
            } else if (id == "getstatic") {
                var obj = data.Params[0];
                var member = data.Params[1];
                GenerateSyntaxComponent(obj, sb, indent, false, paramsStart);
                sb.AppendFormat(".{0}", member.GetId());
            } else if (id == "getinstance") {
                var obj = data.Params[0];
                var member = data.Params[1];
                GenerateSyntaxComponent(obj, sb, indent, false, paramsStart);
                sb.AppendFormat(".{0}", member.GetId());
            } else if (id == "setstatic") {
                var obj = data.Params[0];
                var member = data.Params[1];
                var val = data.Params[2];
                GenerateSyntaxComponent(obj, sb, indent, false, paramsStart);
                sb.AppendFormat(".{0}", member.GetId());
                sb.Append(" = ");
                GenerateSyntaxComponent(val, sb, indent, false, paramsStart);
            } else if (id == "setinstance") {
                var obj = data.Params[0];
                var member = data.Params[1];
                var val = data.Params[2];
                GenerateSyntaxComponent(obj, sb, indent, false, paramsStart);
                sb.AppendFormat(".{0}", member.GetId());
                sb.Append(" = ");
                GenerateSyntaxComponent(val, sb, indent, false, paramsStart);
            } else if (id == "callstatic") {
                var obj = data.Params[0];
                var member = data.Params[1];
                GenerateSyntaxComponent(obj, sb, indent, false, paramsStart);
                sb.AppendFormat(".{0}", member.GetId());
                sb.Append("(");
                string prestr = string.Empty;
                for (int ix = 2; ix < data.Params.Count; ++ix) {
                    var param = data.Params[ix];
                    sb.Append(prestr);
                    string paramId = param.GetId();
                    if (paramId == "...") {
                        sb.AppendFormat("getParams({0})", paramsStart);
                        continue;
                    }
                    GenerateSyntaxComponent(param, sb, indent, false, paramsStart);
                    prestr = ", ";
                }
                sb.Append(")");
            } else if (id == "callinstance") {
                var obj = data.Params[0];
                var member = data.Params[1];
                GenerateSyntaxComponent(obj, sb, indent, false, paramsStart);
                sb.AppendFormat(".{0}", member.GetId());
                sb.Append("(");
                string prestr = string.Empty;
                for (int ix = 2; ix < data.Params.Count; ++ix) {
                    var param = data.Params[ix];
                    sb.Append(prestr);
                    string paramId = param.GetId();
                    if (paramId == "...") {
                        sb.AppendFormat("getParams({0})", paramsStart);
                        continue;
                    }
                    GenerateSyntaxComponent(param, sb, indent, false, paramsStart);
                    prestr = ", ";
                }
                sb.Append(")");
            } else if (id == "literaldictionary") {
                sb.Append("{");
                string prestr = string.Empty;
                for (int ix = 0; ix < data.Params.Count; ++ix) {
                    var param = data.Params[ix] as Dsl.CallData;
                    sb.Append(prestr);
                    var k = param.GetParam(0);
                    var v = param.GetParam(1);
                    GenerateSyntaxComponent(k, sb, indent, false, paramsStart);
                    sb.Append(" : ");
                    GenerateSyntaxComponent(v, sb, indent, false, paramsStart);
                    prestr = ", ";
                }
                sb.Append("}");
            } else if (id == "literallist" || id == "literalcollection") {
                sb.Append("[");
                string prestr = string.Empty;
                for (int ix = 0; ix < data.Params.Count; ++ix) {
                    var param = data.Params[ix];
                    sb.Append(prestr);
                    GenerateSyntaxComponent(param, sb, indent, false, paramsStart);
                    prestr = ", ";
                }
                sb.Append("]");
            } else if (id == "literalarray") {
                sb.Append("[");
                string prestr = string.Empty;
                for (int ix = 1; ix < data.Params.Count; ++ix) {
                    var param = data.Params[ix];
                    sb.Append(prestr);
                    GenerateSyntaxComponent(param, sb, indent, false, paramsStart);
                    prestr = ", ";
                }
                sb.Append("]");
            } else if (id == "newarray") {
                sb.Append("new Array()");
            } else {
                if (null != callData) {
                    GenerateSyntaxComponent(callData, sb, indent, false, paramsStart);
                } else if (id == "elseif") {
                    sb.Append("else if");
                } else {
                    sb.Append(id);
                }
                if (data.HaveParam()) {
                    switch (data.GetParamClass()) {
                        case (int)Dsl.CallData.ParamClassEnum.PARAM_CLASS_PARENTHESIS:
                            sb.Append("(");
                            break;
                        case (int)Dsl.CallData.ParamClassEnum.PARAM_CLASS_BRACKET:
                            sb.Append("[");
                            break;
                        case (int)Dsl.CallData.ParamClassEnum.PARAM_CLASS_PERIOD:
                            sb.AppendFormat(".{0}", data.GetParamId(0));
                            break;
                    }
                    if (data.GetParamClass() != (int)Dsl.CallData.ParamClassEnum.PARAM_CLASS_PERIOD) {
                        string prestr = string.Empty;
                        for (int ix = 0; ix < data.Params.Count; ++ix) {
                            var param = data.Params[ix];
                            sb.Append(prestr);
                            string paramId = param.GetId();
                            if (paramId == "...") {
                                sb.AppendFormat("getParams({0})", paramsStart);
                                continue;
                            }
                            GenerateSyntaxComponent(param, sb, indent, false, paramsStart);
                            prestr = ", ";
                        }
                    }
                    switch (data.GetParamClass()) {
                        case (int)Dsl.CallData.ParamClassEnum.PARAM_CLASS_PARENTHESIS:
                            sb.Append(")");
                            break;
                        case (int)Dsl.CallData.ParamClassEnum.PARAM_CLASS_BRACKET:
                            sb.Append("]");
                            break;
                        case (int)Dsl.CallData.ParamClassEnum.PARAM_CLASS_PERIOD:
                            break;
                    }
                }
            }
        }
        private static void GenerateConcreteSyntax(Dsl.FunctionData data, StringBuilder sb, int indent, bool firstLineUseIndent, int paramsStart)
        {
            if (firstLineUseIndent) {
                sb.AppendFormat("{0}", GetIndentString(indent));
            }
            var fcall = data.Call;
            string id = string.Empty;
            var callData = fcall.Call;
            if (null == callData) {
                id = fcall.GetId();
            }
            if (id == "comments") {
                foreach (var comp in data.Statements) {
                    GenerateSyntaxComponent(comp, sb, indent, true, paramsStart);
                    sb.AppendLine();
                }
            } else if (id == "local") {
                bool first = true;
                foreach (var comp in data.Statements) {
                    if (!first) {
                        sb.AppendLine(";");
                    } else {
                        first = false;
                    }
                    sb.Append("var ");
                    GenerateSyntaxComponent(comp, sb, indent, false, paramsStart);
                }
            } else {
                GenerateConcreteSyntax(fcall, sb, indent, false, paramsStart);
                if (data.HaveStatement()) {
                    sb.AppendLine("{");
                    ++indent;
                    foreach (var comp in data.Statements) {
                        GenerateSyntaxComponent(comp, sb, indent, true, paramsStart);
                        string subId = comp.GetId();
                        if (subId != "comments" && subId != "comment") {
                            sb.AppendLine(";");
                        } else {
                            sb.AppendLine();
                        }
                    }
                    --indent;
                    sb.AppendFormat("{0}}}", GetIndentString(indent));
                }
            }
        }
        private static void GenerateConcreteSyntax(Dsl.StatementData data, StringBuilder sb, int indent, bool firstLineUseIndent, int paramsStart)
        {
            if (firstLineUseIndent) {
                sb.AppendFormat("{0}", GetIndentString(indent));
            }
            foreach (var funcData in data.Functions) {
                var fcall = funcData.Call;
                GenerateConcreteSyntax(fcall, sb, indent, false, paramsStart);
                if (funcData.HaveStatement()) {
                    sb.AppendLine("{");
                    ++indent;
                    foreach (var comp in funcData.Statements) {
                        GenerateSyntaxComponent(comp, sb, indent, true, paramsStart);
                        string subId = comp.GetId();
                        if (subId != "comments" && subId != "comment") {
                            sb.AppendLine(";");
                        } else {
                            sb.AppendLine();
                        }
                    }
                    --indent;
                    sb.AppendFormat("{0}}}", GetIndentString(indent));
                } else {
                    sb.Append(" ");
                }
            }
        }
        private static Dsl.ISyntaxComponent FindParam(Dsl.FunctionData funcData, string key)
        {
            foreach (var statement in funcData.Call.Params) {
                if (key == statement.GetId()) {
                    return statement;
                }
            }
            return null;
        }
        private static Dsl.ISyntaxComponent FindStatement(Dsl.FunctionData funcData, string key)
        {
            foreach (var statement in funcData.Statements) {
                if (key == statement.GetId()) {
                    return statement;
                }
            }
            return null;
        }
        private static void Log(string file, string msg)
        {
            s_LogBuilder.AppendFormatLine("[{0}]:{1}", file, msg);
        }
        private static string GetIndentString(int indent)
        {
            const string c_IndentString = "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t";
            return c_IndentString.Substring(0, indent);
        }
        private static string Escape(string src)
        {
            StringBuilder sb = new StringBuilder();
            //dsl语言只显示处理'\0'、"\xhh"、"\ooo"转义，其它都以实际字符保存在源代码中，以下4个特殊的控制字符无法在源代码中保存，cs2dsl会保存成'\\字符'的形式，之后使用dsl读入时，字符串中将以'\字符'的形式存在
            //转义时要对此进行特殊处理
            src = src.Replace("\\a", "\a").Replace("\\b", "\b").Replace("\\f", "\f").Replace("\\v", "\v");
            for (int i = 0; i < src.Length; ++i) {
                char c = src[i];
                string es = Escape(c);
                sb.Append(es);
            }
            return sb.ToString();
        }
        private static string Escape(char c)
        {
            switch (c) {
                case '\a':
                    return "\\a";
                case '\b':
                    return "\\b";
                case '\f':
                    return "\\f";
                case '\n':
                    return "\\n";
                case '\r':
                    return "\\r";
                case '\t':
                    return "\\t";
                case '\v':
                    return "\\v";
                case '\\':
                    return "\\\\";
                case '\"':
                    return "\\\"";
                case '\'':
                    return "\\'";
                case '\0':
                    return "\\0";
                default:
                    return c.ToString();
            }
        }

        private static string s_ExePath = string.Empty;
        private static string s_SrcPath = string.Empty;
        private static string s_LogPath = string.Empty;
        private static string s_OutPath = string.Empty;
        private static string s_Ext = string.Empty;
        private static StringBuilder s_LogBuilder = new StringBuilder();
    }
}

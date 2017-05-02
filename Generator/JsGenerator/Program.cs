using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace JsGenerator
{
    static class StringBuilderExtension
    {
        public static void AppendFormatLine(this StringBuilder sb, string format, params object[] args)
        {
            sb.AppendFormat(format, args);
            sb.AppendLine();
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            string path = "dsl";
            if (args.Length > 0) {
                path = args[0];
            }
            var files = Directory.GetFiles(path, "*.dsl", SearchOption.TopDirectoryOnly);
            foreach (string file in files) {
                Dsl.DslFile dslFile = new Dsl.DslFile();
                dslFile.Load(file, s => Log(file, s));
                GenerateJs(dslFile, Path.ChangeExtension(file, "js"));
            }
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
                    sb.AppendFormatLine("{0}require(\"{1}.js\");", GetIndentString(indent), callData.GetParamId(0));
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
                                        sb.AppendLine(";");
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
                                        sb.AppendLine(";");
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
                    sb.AppendFormat("\"{0}\"", id);
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
                    if (id == "=" && param1.GetId() == "multiassign") {
                        string varName = string.Format("__compiler_multiassign_{0}", data.GetLine());
                        sb.AppendFormat("var {0}", varName);
                        sb.AppendFormat(" {0} ", id);
                        GenerateSyntaxComponent(param2, sb, indent, false, paramsStart);
                        sb.Append(";");
                        var cd = param1 as Dsl.CallData;
                        int varNum = cd.GetParamNum();
                        for (int i = 0; i < varNum; ++i) {
                            string var = cd.GetParamId(i);
                            sb.AppendFormat("{0} = {1}[{2}]", var, varName, i);
                            if (i < varNum - 1) {
                                sb.Append(";");
                            }
                        }
                    } else {
                        if (id != "=")
                            sb.Append("(");
                        GenerateSyntaxComponent(param1, sb, indent, false, paramsStart);
                        sb.AppendFormat(" {0} ", id);
                        GenerateSyntaxComponent(param2, sb, indent, false, paramsStart);
                        if (id != "=")
                            sb.Append(")");
                    }
                }
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
            } else if (id == "dictionaryinit") {
                sb.Append("{");
                string prestr = string.Empty;
                for (int ix = 0; ix < data.Params.Count; ++ix) {
                    var param = data.Params[ix] as Dsl.CallData;
                    sb.Append(prestr);
                    var k = param.IsHighOrder ? param.Call as Dsl.ISyntaxComponent : param.Name as Dsl.ISyntaxComponent;
                    var v = param.GetParam(0);
                    GenerateSyntaxComponent(k, sb, indent, false, paramsStart);
                    sb.Append(" : ");
                    GenerateSyntaxComponent(v, sb, indent, false, paramsStart);
                    prestr = ", ";
                }
                sb.Append("}");
            } else if (id == "listinit" || id == "collectioninit" || id == "arrayinit") {
                sb.Append("[");
                string prestr = string.Empty;
                for (int ix = 0; ix < data.Params.Count; ++ix) {
                    var param = data.Params[ix];
                    sb.Append(prestr);
                    GenerateSyntaxComponent(param, sb, indent, false, paramsStart);
                    prestr = ", ";
                }
                sb.Append("]");
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
            if (id == "local") {
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
                        sb.AppendLine(";");
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
                        sb.AppendLine(";");
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
            Console.WriteLine("[{0}]:{1}", file, msg);
        }
        private static string GetIndentString(int indent)
        {
            const string c_IndentString = "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t";
            return c_IndentString.Substring(0, indent);
        }
    }
}

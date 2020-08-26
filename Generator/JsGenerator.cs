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
        internal static void Generate(string csprojPath, string outPath, string ext, bool parallel)
        {
            if (string.IsNullOrEmpty(outPath)) {
                outPath = Path.Combine(csprojPath, "dsl");
            }
            else if (!Path.IsPathRooted(outPath)) {
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
            Action<string> handler = (file) => {
                try {
                    string fileName = Path.GetFileNameWithoutExtension(file);

                    Dsl.DslFile dslFile = new Dsl.DslFile();
                    dslFile.Load(file, s => Log(file, s));
                    GenerateJs(dslFile, Path.Combine(s_OutPath, Path.ChangeExtension(fileName.Replace("cs2dsl__", "cs2js__"), s_Ext)));
                }
                catch (Exception ex) {
                    string id = string.Empty;
                    int line = 0;
                    if (null != s_CurSyntax) {
                        id = s_CurSyntax.GetId();
                        if (null == id)
                            id = string.Empty;
                        line = s_CurSyntax.GetLine();
                    }
                    Log(file, string.Format("[{0}:{1}]:exception:{2}\n{3}", id, line, ex.Message, ex.StackTrace));
                    File.WriteAllText(Path.Combine(s_LogPath, "Generator.log"), s_LogBuilder.ToString());
                    System.Environment.Exit(-1);
                }
            };
            if (parallel) {
                Parallel.ForEach(files, handler);
            }
            else {
                foreach (var file in files) {
                    handler(file);
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
                Dsl.FunctionData funcData = dslInfo as Dsl.FunctionData;
                Dsl.FunctionData callData = funcData;
                if (null != funcData && funcData.IsHighOrder) {
                    callData = funcData.LowerOrderFunction;
                }
                if (id == "require") {
                    sb.AppendFormatLine("{0}require(\"{1}.js\");", GetIndentString(indent), callData.GetParamId(0).Replace("cs2dsl__", "cs2js__"));
                }
                else if (id == "enum") {

                }
                else if (id == "class" || id == "struct") {
                    string className = callData.GetParamId(0);
                    var baseClass = callData.GetParam(1);

                    sb.AppendLine();

                    sb.AppendFormatLine("{0}function {1}(){{", GetIndentString(indent), className);
                    ++indent;
                    if (null != baseClass && baseClass.IsValid()) {
                        GenerateSyntaxComponent(baseClass, sb, indent, true, 0);
                        sb.AppendLine(".call(this);");
                        sb.AppendLine();
                    }
                    var instMethods = FindStatement(funcData, "instance_methods") as Dsl.FunctionData;
                    if (null != instMethods) {
                        foreach (var def in instMethods.Params) {
                            var mdef = def as Dsl.FunctionData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                var fdef = mdef.GetParam(1) as Dsl.StatementData;
                                if (null != fdef && fdef.GetFunctionNum() == 2) {
                                    var first = fdef.First;
                                    var second = fdef.Second;
                                    int rct;
                                    int.TryParse(first.GetParamId(0), out rct);
                                    if (second.HaveStatement()) {
                                        var fcall = second;
                                        if (second.IsHighOrder)
                                            fcall = second.LowerOrderFunction;
                                        int paramsStart = 0;
                                        sb.AppendFormat("{0}this.{1} = {2}(", GetIndentString(indent), mname, "function");
                                        prestr = string.Empty;
                                        for (int ix = 1; ix < fcall.Params.Count; ++ix) {
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
                                            }
                                            else {
                                                var pc = param as Dsl.FunctionData;
                                                if (null != pc) {
                                                    sb.Append(pc.GetParamId(0));
                                                }
                                            }
                                        }
                                        sb.AppendLine("){");
                                        ++indent;
                                        foreach (var comp in second.Params) {
                                            GenerateSyntaxComponent(comp, sb, indent, true, paramsStart);
                                            string subId = comp.GetId();
                                            if (subId != "comments" && subId != "comment") {
                                                sb.AppendLine(";");
                                            }
                                            else {
                                                sb.AppendLine();
                                            }
                                        }
                                        --indent;
                                        sb.AppendFormatLine("{0}}}", GetIndentString(indent));
                                    }
                                }
                            }
                        }
                    }
                    var instFields = FindStatement(funcData, "instance_fields") as Dsl.FunctionData;
                    if (null != instFields) {
                        foreach (var def in instFields.Params) {
                            var mdef = def as Dsl.FunctionData;
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
                        foreach (var def in staticMethods.Params) {
                            var mdef = def as Dsl.FunctionData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                var fdef = mdef.GetParam(1) as Dsl.StatementData;
                                if (null != fdef && fdef.GetFunctionNum() == 2) {
                                    var first = fdef.First;
                                    var second = fdef.Second;
                                    int rct;
                                    int.TryParse(first.GetParamId(0), out rct);
                                    if (second.HaveStatement()) {
                                        var fcall = second;
                                        if (second.IsHighOrder)
                                            fcall = second.LowerOrderFunction;
                                        int paramsStart = 0;
                                        sb.AppendFormat("{0}{1}.{2} = {3}(", GetIndentString(indent), className, mname, "function");
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
                                            }
                                            else {
                                                var pc = param as Dsl.FunctionData;
                                                if (null != pc) {
                                                    sb.Append(pc.GetParamId(0));
                                                }
                                            }
                                        }
                                        sb.AppendLine("){");
                                        ++indent;
                                        foreach (var comp in second.Params) {
                                            GenerateSyntaxComponent(comp, sb, indent, true, paramsStart);
                                            string subId = comp.GetId();
                                            if (subId != "comments" && subId != "comment") {
                                                sb.AppendLine(";");
                                            }
                                            else {
                                                sb.AppendLine();
                                            }
                                        }
                                        --indent;
                                        sb.AppendFormatLine("{0}}}", GetIndentString(indent));
                                    }
                                }
                            }
                        }
                    }
                    var staticFields = FindStatement(funcData, "static_fields") as Dsl.FunctionData;
                    if (null != staticFields) {
                        foreach (var def in staticFields.Params) {
                            var mdef = def as Dsl.FunctionData;
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
            s_CurSyntax = comp;
            var valData = comp as Dsl.ValueData;
            if (null != valData) {
                GenerateConcreteSyntax(valData, sb, indent, firstLineUseIndent, paramsStart);
            }
            else {
                var funcData = comp as Dsl.FunctionData;
                if (null != funcData) {
                    GenerateConcreteSyntax(funcData, sb, indent, firstLineUseIndent, paramsStart);
                }
                else {
                    var statementData = comp as Dsl.StatementData;
                    GenerateConcreteSyntax(statementData, sb, indent, firstLineUseIndent, paramsStart);
                }
            }
        }
        private static void GenerateConcreteSyntax(Dsl.ValueData data, StringBuilder sb, int indent, bool firstLineUseIndent, int paramsStart)
        {
            s_CurSyntax = data;
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
        private static void GenerateConcreteSyntaxForCall(Dsl.FunctionData data, StringBuilder sb, int indent, bool firstLineUseIndent, int paramsStart)
        {
            s_CurSyntax = data;
            Dsl.FunctionData callData = null;
            string id = string.Empty;
            if (data.IsHighOrder) {
                callData = data.LowerOrderFunction;
            }
            else {
                id = data.GetId();
            }
            if (firstLineUseIndent) {
                sb.AppendFormat("{0}", GetIndentString(indent));
            }
            if (data.GetParamClass() == (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_OPERATOR) {
                int paramNum = data.GetParamNum();
                if (paramNum == 1) {
                    var param1 = data.GetParam(0);
                    sb.AppendFormat("({0} ", id);
                    GenerateSyntaxComponent(param1, sb, indent, false, paramsStart);
                    sb.Append(")");
                }
                else if (paramNum == 2) {
                    var param1 = data.GetParam(0);
                    var param2 = data.GetParam(1);
                    bool handled = false;
                    if (id == "=" && param1.GetId() == "multiassign") {
                        var cd = param1 as Dsl.FunctionData;
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
                            }
                            else {
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
            }
            else if (id == "nullcoalescing") {
                var p1 = data.GetParam(0);
                var p2 = data.GetParamId(1);
                var p3 = data.GetParam(2);
                if (p2 == "true") {
                    sb.Append("nullcoalescing(");
                    GenerateSyntaxComponent(p1, sb, indent, false, paramsStart);
                    sb.Append(", function(){ return ");
                    GenerateSyntaxComponent(p3, sb, indent, false, paramsStart);
                    sb.Append(";})");
                }
                else {
                    sb.Append("nullcoalescing(");
                    GenerateSyntaxComponent(p1, sb, indent, false, paramsStart);
                    sb.Append(", ");
                    GenerateSyntaxComponent(p3, sb, indent, false, paramsStart);
                    sb.Append(")");
                }
            }
            else if (id == "comment") {
                sb.AppendFormat("//{0}", data.GetParamId(0));
            }
            else if (id == "local") {
                sb.Append("var ");
                string prestr = string.Empty;
                foreach (var param in data.Params) {
                    sb.Append(prestr);
                    GenerateSyntaxComponent(param, sb, indent, false, paramsStart);
                    prestr = ", ";
                }
            }
            else if (id == "return") {
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
            }
            else if (id == "prefixoperator") {
                var varExp = data.GetParam(0);
                var opExp = data.GetParam(1);
                sb.Append("(function(){ ");
                GenerateSyntaxComponent(varExp, sb, 0, false, paramsStart);
                sb.Append(" = ");
                GenerateSyntaxComponent(opExp, sb, 0, false, paramsStart);
                sb.Append("; return ");
                GenerateSyntaxComponent(varExp, sb, 0, false, paramsStart);
                sb.Append(";)()");
            }
            else if (id == "postfixoperator") {
                var oldVal = data.GetParam(0);
                var varExp = data.GetParam(1);
                var opExp = data.GetParam(2);
                sb.Append("(function(){ local ");
                sb.Append(oldVal);
                sb.Append(" = ");
                GenerateSyntaxComponent(varExp, sb, 0, false, paramsStart);
                sb.Append("; ");
                GenerateSyntaxComponent(varExp, sb, 0, false, paramsStart);
                sb.Append(" = ");
                GenerateSyntaxComponent(opExp, sb, 0, false, paramsStart);
                sb.Append("; return ");
                sb.Append(oldVal);
                sb.Append(";)()");
            }
            else if (id == "execunary") {
                string op = data.GetParamId(0);
                sb.AppendFormat("{0} ", op);
                GenerateSyntaxComponent(data.GetParam(1), sb, indent, false, paramsStart);
            }
            else if (id == "execbinary") {
                string op = data.GetParamId(0);
                GenerateSyntaxComponent(data.GetParam(1), sb, indent, false, paramsStart);
                sb.AppendFormat(" {0} ", op);
                GenerateSyntaxComponent(data.GetParam(2), sb, indent, false, paramsStart);
            }
            else if (id == "getstatic" || id == "getexternstatic") {
                var obj = data.Params[1];
                var member = data.Params[2];
                GenerateSyntaxComponent(obj, sb, indent, false, paramsStart);
                sb.AppendFormat(".{0}", member.GetId());
            }
            else if (id == "getinstance" || id == "getexterninstance") {
                var obj = data.Params[1];
                var member = data.Params[2];
                GenerateSyntaxComponent(obj, sb, indent, false, paramsStart);
                sb.AppendFormat(".{0}", member.GetId());
            }
            else if (id == "setstatic" || id == "getexternstatic") {
                var obj = data.Params[1];
                var member = data.Params[2];
                var val = data.Params[3];
                GenerateSyntaxComponent(obj, sb, indent, false, paramsStart);
                sb.AppendFormat(".{0}", member.GetId());
                sb.Append(" = ");
                GenerateSyntaxComponent(val, sb, indent, false, paramsStart);
            }
            else if (id == "setinstance" || id == "getexterninstance") {
                var obj = data.Params[1];
                var member = data.Params[2];
                var val = data.Params[3];
                GenerateSyntaxComponent(obj, sb, indent, false, paramsStart);
                sb.AppendFormat(".{0}", member.GetId());
                sb.Append(" = ");
                GenerateSyntaxComponent(val, sb, indent, false, paramsStart);
            }
            else if (id == "callstatic" || id == "callexternstatic") {
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
            }
            else if (id == "callinstance" || id == "callexterninstance") {
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
            }
            else if (id == "typeargs") {
                if (data.GetParamNum() > 0) {
                    sb.Append("[");
                    GenerateArguments(data, sb, indent, 0);
                    sb.Append("]");
                }
                else {
                    sb.Append("null");
                }
            }
            else if (id == "typekinds") {
                if (data.GetParamNum() > 0) {
                    sb.Append("[");
                    GenerateArguments(data, sb, indent, 0);
                    sb.Append("]");
                }
                else {
                    sb.Append("null");
                }
            }
            else if (id == "literaldictionary") {
                sb.Append("{");
                string prestr = string.Empty;
                for (int ix = 2; ix < data.Params.Count; ++ix) {
                    var param = data.Params[ix] as Dsl.FunctionData;
                    sb.Append(prestr);
                    var k = param.GetParam(0);
                    var v = param.GetParam(1);
                    GenerateSyntaxComponent(k, sb, indent, false, paramsStart);
                    sb.Append(" : ");
                    GenerateSyntaxComponent(v, sb, indent, false, paramsStart);
                    prestr = ", ";
                }
                sb.Append("}");
            }
            else if (id == "literallist" || id == "literalcollection" || id == "literalcomplex") {
                sb.Append("[");
                string prestr = string.Empty;
                for (int ix = 2; ix < data.Params.Count; ++ix) {
                    var param = data.Params[ix];
                    sb.Append(prestr);
                    GenerateSyntaxComponent(param, sb, indent, false, paramsStart);
                    prestr = ", ";
                }
                sb.Append("]");
            }
            else if (id == "literalarray") {
                sb.Append("[");
                string prestr = string.Empty;
                for (int ix = 2; ix < data.Params.Count; ++ix) {
                    var param = data.Params[ix];
                    sb.Append(prestr);
                    GenerateSyntaxComponent(param, sb, indent, false, paramsStart);
                    prestr = ", ";
                }
                sb.Append("]");
            }
            else if (id == "newarray") {
                var typeStr = CalcTypeString(data.GetParam(0));
                var typeKind = CalcTypeString(data.GetParam(1));
                if (data.GetParamNum() > 2) {
                    var vname = data.GetParamId(2);
                    sb.AppendFormat("wraparray([], {0}, {1}, {2})", vname, typeStr, typeKind);
                }
                else {
                    sb.AppendFormat("wraparray([], nil, {0}, {1})", typeStr, typeKind);
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
                    sb.AppendFormat("newarraydim{0}({1}, {2}, ", ct);
                    GenerateSyntaxComponent(defVal, sb, 0, false, paramsStart);
                    if (ct > 0) {
                        sb.Append(", ");
                        var exp = data.GetParam(4 + 0);
                        GenerateSyntaxComponent(exp, sb, 0, false, paramsStart);
                    }
                    if (ct > 1) {
                        sb.Append(", ");
                        var exp = data.GetParam(4 + 1);
                        GenerateSyntaxComponent(exp, sb, 0, false, paramsStart);
                    }
                    if (ct > 2) {
                        sb.Append(", ");
                        var exp = data.GetParam(4 + 2);
                        GenerateSyntaxComponent(exp, sb, 0, false, paramsStart);
                    }
                    sb.Append(")");
                }
                else {
                    //四维及以上数组在这里使用函数对象嵌入初始化代码，应该很少用到
                    sb.Append("(function(){");
                    for (int i = 0; i < ct; ++i) {
                        sb.AppendFormat(" var d{0} = ", i);
                        var exp = data.GetParam(4 + i);
                        GenerateSyntaxComponent(exp, sb, 0, false, paramsStart);
                        if (i == 0) {
                            sb.AppendFormat("; var arr = wraparray([], d0, {0}, {1})", typeStr, typeKind);
                        }
                        sb.AppendFormat("; for(var i{0} = 1; i{0} < d{1}; ++i{0}){{ arr{2} = ", i, i, GetArraySubscriptString(i));
                        if (i < ct - 1) {
                            sb.Append("wraparray([], ");
                            var nextExp = data.GetParam(4 + i + 1);
                            GenerateSyntaxComponent(nextExp, sb, 0, false, paramsStart);
                            sb.AppendFormat(", {0}, {1});", typeStr, typeKind);
                        }
                        else {
                            GenerateSyntaxComponent(defVal, sb, 0, false, paramsStart);
                            sb.Append(";");
                        }
                    }
                    for (int i = 0; i < ct; ++i) {
                        sb.Append(" };");
                    }
                    sb.Append(" return arr; })()");
                }
            }
            else {
                if (null != callData) {
                    GenerateSyntaxComponent(callData, sb, indent, false, paramsStart);
                }
                else if (id == "elseif") {
                    sb.Append("else if");
                }
                else {
                    sb.Append(id);
                }
                if (data.HaveParam()) {
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
                    if (data.GetParamClass() != (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PERIOD) {
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
            }
        }
        private static void GenerateConcreteSyntax(Dsl.FunctionData data, StringBuilder sb, int indent, bool firstLineUseIndent, int paramsStart)
        {
            if(!data.IsHighOrder && data.HaveParam()) {
                GenerateConcreteSyntaxForCall(data, sb, indent, firstLineUseIndent, paramsStart);
                return;
            }
            s_CurSyntax = data;
            if (firstLineUseIndent) {
                sb.AppendFormat("{0}", GetIndentString(indent));
            }
            var fcall = data.LowerOrderFunction;
            string id = string.Empty;
            Dsl.FunctionData callData = null;
            if (fcall.IsHighOrder)
                callData = fcall.LowerOrderFunction;
            if (null == callData) {
                id = fcall.GetId();
            }
            if (id == "comments") {
                foreach (var comp in data.Params) {
                    GenerateSyntaxComponent(comp, sb, indent, true, paramsStart);
                    sb.AppendLine();
                }
            }
            else if (id == "local") {
                bool first = true;
                foreach (var comp in data.Params) {
                    if (!first) {
                        sb.AppendLine(";");
                    }
                    else {
                        first = false;
                    }
                    sb.Append("var ");
                    GenerateSyntaxComponent(comp, sb, indent, false, paramsStart);
                }
            }
            else if (id == "execclosure") {
                string localName = data.GetParamId(0);
                bool needDecl = (bool)Convert.ChangeType(data.GetParamId(1), typeof(bool));
                sb.AppendLine("(function(){ ");
                if (data.HaveStatement()) {
                    ++indent;
                    if (needDecl) {
                        sb.AppendFormatLine("{0}var {1};", GetIndentString(indent), localName);
                    }
                    foreach (var comp in data.Params) {
                        GenerateSyntaxComponent(comp, sb, indent, true, paramsStart);
                        string subId = comp.GetId();
                        if (subId != "comments" && subId != "comment") {
                            sb.AppendLine(";");
                        }
                        else {
                            sb.AppendLine();
                        }
                    }
                    sb.AppendFormatLine("{0}return {1};", GetIndentString(indent), localName);
                    --indent;
                }
                sb.AppendFormat("{0}}})()", GetIndentString(indent));
            }
            else {
                GenerateConcreteSyntaxForCall(fcall, sb, indent, false, paramsStart);
                if (data.HaveStatement()) {
                    sb.AppendLine("{");
                    ++indent;
                    foreach (var comp in data.Params) {
                        GenerateSyntaxComponent(comp, sb, indent, true, paramsStart);
                        string subId = comp.GetId();
                        if (subId != "comments" && subId != "comment") {
                            sb.AppendLine(";");
                        }
                        else {
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
                var fcall = funcData.ThisOrLowerOrderCall;
                GenerateConcreteSyntaxForCall(fcall, sb, indent, false, paramsStart);
                if (funcData.HaveStatement()) {
                    sb.AppendLine("{");
                    ++indent;
                    foreach (var comp in funcData.Params) {
                        GenerateSyntaxComponent(comp, sb, indent, true, paramsStart);
                        string subId = comp.GetId();
                        if (subId != "comments" && subId != "comment") {
                            sb.AppendLine(";");
                        }
                        else {
                            sb.AppendLine();
                        }
                    }
                    --indent;
                    sb.AppendFormat("{0}}}", GetIndentString(indent));
                }
                else {
                    sb.Append(" ");
                }
            }
        }
        private static void GenerateArguments(Dsl.FunctionData data, StringBuilder sb, int indent, int start)
        {
            s_CurSyntax = data;
            GenerateArguments(data, sb, indent, start, string.Empty);
        }
        private static void GenerateArguments(Dsl.FunctionData data, StringBuilder sb, int indent, int start, string sig)
        {
            s_CurSyntax = data;
            string prestr = string.Empty;
            if (!string.IsNullOrEmpty(sig)) {
                sb.Append(prestr);
                sb.AppendFormat("\"{0}\"", Escape(sig));
                prestr = ", ";
            }
            for (int ix = start; ix < data.Params.Count; ++ix) {
                var param = data.Params[ix];
                sb.Append(prestr);
                string paramId = param.GetId();
                if (param.GetIdType() == (int)Dsl.ValueData.ID_TOKEN && paramId == "...") {
                    sb.Append("...");
                    continue;
                }
                GenerateSyntaxComponent(param, sb, indent, false, 0);
                prestr = ", ";
            }
        }
        private static string CalcTypesString(Dsl.FunctionData cd)
        {
            StringBuilder sb = new StringBuilder();
            string prestr = string.Empty;
            foreach (var p in cd.Params) {
                var str = CalcTypeString(p);
                sb.Append(prestr);
                sb.Append(str);
                prestr = ":";
            }
            return sb.ToString();
        }
        private static string CalcTypeString(Dsl.ISyntaxComponent comp)
        {
            string ret = comp.GetId();
            var cd = comp as Dsl.FunctionData;
            if (null != cd && cd.GetParamClass() == (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PERIOD) {
                string prefix;
                if (cd.IsHighOrder) {
                    prefix = CalcTypeString(cd.LowerOrderFunction);
                }
                else {
                    prefix = cd.GetId();
                }
                ret = prefix + "." + cd.GetParamId(0);
            }
            return ret;
        }
        private static string CalcExpressionString(Dsl.ISyntaxComponent comp)
        {
            StringBuilder sb = new StringBuilder();
            GenerateSyntaxComponent(comp, sb, 0, false, 0);
            return sb.ToString();
        }
        private static Dsl.ISyntaxComponent FindParam(Dsl.FunctionData funcData, string key)
        {
            var fcd = funcData;
            if (funcData.IsHighOrder)
                fcd = funcData;
            foreach (var statement in fcd.Params) {
                if (key == statement.GetId()) {
                    return statement;
                }
            }
            return null;
        }
        private static Dsl.ISyntaxComponent FindStatement(Dsl.FunctionData funcData, string key)
        {
            foreach (var statement in funcData.Params) {
                if (key == statement.GetId()) {
                    return statement;
                }
            }
            return null;
        }
        private static string GetLastName(string fullName)
        {
            int ix = fullName.LastIndexOf('.');
            if (ix < 0)
                return fullName;
            else
                return fullName.Substring(ix + 1);
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
        private static string GetArraySubscriptString(int index)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i <= index; ++i) {
                sb.AppendFormat("[i{0}]", i);
            }
            return sb.ToString();
        }
        private static string Escape(string src)
        {
            StringBuilder sb = new StringBuilder();
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

        private static Dsl.ISyntaxComponent s_CurSyntax = null;
        private static string s_ExePath = string.Empty;
        private static string s_SrcPath = string.Empty;
        private static string s_LogPath = string.Empty;
        private static string s_OutPath = string.Empty;
        private static string s_Ext = string.Empty;
        private static StringBuilder s_LogBuilder = new StringBuilder();
    }
}

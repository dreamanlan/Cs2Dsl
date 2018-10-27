using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace LuaGenerator
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
                try {
                    Dsl.DslFile dslFile = new Dsl.DslFile();
                    dslFile.Load(file, s => Log(file, s));
                    GenerateLua(dslFile, Path.ChangeExtension(file.Replace("cs2dsl__", "cs2lua__"), "txt"));
                } catch (Exception ex) {
                    Log(file, string.Format("exception:{0}\n{1}", ex.Message, ex.StackTrace));
                }
            }
        }
        private static void GenerateLua(Dsl.DslFile dslFile, string outputFile)
        {
            StringBuilder sb = new StringBuilder();
            string prestr = string.Empty;
            int indent = 0;
            foreach (var dslInfo in dslFile.DslInfos) {
                string id = dslInfo.GetId();
                var funcData = dslInfo.First;
                var callData = funcData.Call;
                if (id == "require") {
                    sb.AppendFormatLine("{0}require \"{1}\";", GetIndentString(indent), callData.GetParamId(0).Replace("cs2dsl__", "cs2lua__"));
                } else if (id == "enum") {

                } else if (id == "class" || id == "struct") {
                    string className = callData.GetParamId(0);
                    var baseClass = callData.GetParam(1);
                    bool isValueType = id == "struct";

                    sb.AppendLine();
                    
                    sb.AppendFormatLine("{0}{1} = {{", GetIndentString(indent), className);
                    ++indent;
                    
                    var staticMethods = FindStatement(funcData, "static_methods") as Dsl.FunctionData;
                    if (null != staticMethods) {
                        foreach (var def in staticMethods.Statements) {
                            var mdef = def as Dsl.CallData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                var fdef = mdef.GetParam(1) as Dsl.FunctionData;
                                if (mname == "__new_object" && null != fdef) {
                                    var fcall = fdef.Call;
                                    sb.AppendFormat("{0}{1} = {2}(", GetIndentString(indent), mname, fcall.GetId());
                                    prestr = string.Empty;
                                    for (int ix = 0; ix < fcall.Params.Count; ++ix) {
                                        var param = fcall.Params[ix];
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
                                    sb.AppendLine(")");
                                    ++indent;
                                    foreach (var comp in fdef.Statements) {
                                        GenerateSyntaxComponent(comp, sb, indent, true);
                                        sb.AppendLine(";");
                                    }
                                    --indent;
                                    sb.AppendFormatLine("{0}end,", GetIndentString(indent));
                                }
                            }
                        }
                    }
	                
                    sb.AppendFormatLine("{0}__define_class = function()", GetIndentString(indent));
                    ++indent;
                    sb.AppendFormatLine("{0}local static = {1};", GetIndentString(indent), className);
                                        
                    if (null != staticMethods) {
                        sb.AppendFormatLine("{0}local static_methods = {{", GetIndentString(indent));
                        ++indent;
                        foreach (var def in staticMethods.Statements) {
                            var mdef = def as Dsl.CallData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                var fdef = mdef.GetParam(1) as Dsl.FunctionData;
                                if (mname != "__new_object" && null != fdef) {
                                    var fcall = fdef.Call;
                                    sb.AppendFormat("{0}{1} = {2}(", GetIndentString(indent), mname, fcall.GetId());
                                    prestr = string.Empty;
                                    for (int ix = 0; ix < fcall.Params.Count; ++ix) {
                                        var param = fcall.Params[ix];
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
                                    sb.AppendLine(")");
                                    ++indent;
                                    foreach (var comp in fdef.Statements) {
                                        GenerateSyntaxComponent(comp, sb, indent, true);
                                        sb.AppendLine(";");
                                    }
                                    --indent;
                                    sb.AppendFormatLine("{0}end,", GetIndentString(indent));
                                }
                            }
                        }
                        --indent;
                        sb.AppendFormatLine("{0}}};", GetIndentString(indent));
                    }

                    sb.AppendLine();

                    sb.AppendFormatLine("{0}local static_fields_build = function()", GetIndentString(indent));
                    ++indent;
			        sb.AppendFormatLine("{0}local static_fields = {{", GetIndentString(indent));
                    ++indent;

                    var staticFields = FindStatement(funcData, "static_fields") as Dsl.FunctionData;
                    if (null != staticFields) {
                        foreach (var def in staticFields.Statements) {
                            var mdef = def as Dsl.CallData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                var comp = mdef.GetParam(1);
                                sb.AppendFormat("{0}{1} = ", GetIndentString(indent), mname);
                                GenerateFieldValueComponent(comp, sb, indent, false);
                                sb.AppendLine(",");
                            }
                        }
                    }

                    --indent;
			        sb.AppendFormatLine("{0}}};", GetIndentString(indent));
			        sb.AppendFormatLine("{0}return static_fields;", GetIndentString(indent));
                    --indent;
                    sb.AppendFormatLine("{0}end;", GetIndentString(indent));
                    
                    var staticProps = FindStatement(funcData, "static_props") as Dsl.FunctionData;
                    if (null != staticProps && staticProps.GetStatementNum() > 0) {
                        sb.AppendFormatLine("{0}local static_props = {{", GetIndentString(indent));
                        ++indent;
                        foreach (var def in staticProps.Statements) {
                            var mdef = def as Dsl.CallData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                var prop = mdef.GetParam(1);
                                sb.AppendFormatLine("{0}{1} = {{", GetIndentString(indent), mname);
                                ++indent;
                                var body = prop as Dsl.FunctionData;
                                if (null != body) {
                                    foreach (var comp in body.Statements) {
                                        GenerateSyntaxComponent(comp, sb, indent, true);
                                        sb.AppendLine(",");
                                    }
                                }
                                --indent;
                                sb.AppendFormatLine("{0}}},", GetIndentString(indent));
                            }
                        }
                        --indent;
                        sb.AppendFormatLine("{0}}};", GetIndentString(indent));
                    } else {
                        sb.AppendFormatLine("{0}local static_props = nil;", GetIndentString(indent));
                    }
                    var staticEvents = FindStatement(funcData, "static_events") as Dsl.FunctionData;
                    if (null != staticEvents && staticEvents.GetStatementNum() > 0) {
                        sb.AppendFormatLine("{0}local static_events = {{", GetIndentString(indent));
                        ++indent;
                        foreach (var def in staticEvents.Statements) {
                            var mdef = def as Dsl.CallData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                var evt = mdef.GetParam(1);
                                sb.AppendFormatLine("{0}{1} = {{", GetIndentString(indent), mname);
                                ++indent;
                                var body = evt as Dsl.FunctionData;
                                if (null != body) {
                                    foreach (var comp in body.Statements) {
                                        GenerateSyntaxComponent(comp, sb, indent, true);
                                        sb.AppendLine(",");
                                    }
                                }
                                --indent;
                                sb.AppendFormatLine("{0}}},", GetIndentString(indent));
                            }
                        }
                        --indent;
                        sb.AppendFormatLine("{0}}};", GetIndentString(indent));
                    } else {
                        sb.AppendFormatLine("{0}local static_events = nil;", GetIndentString(indent));
                    }

                    sb.AppendLine();
                    
                    var instMethods = FindStatement(funcData, "instance_methods") as Dsl.FunctionData;
                    if (null != instMethods) {
                        sb.AppendFormatLine("{0}local instance_methods = {{", GetIndentString(indent));
                        ++indent;
                        foreach (var def in instMethods.Statements) {
                            var mdef = def as Dsl.CallData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                var fdef = mdef.GetParam(1) as Dsl.FunctionData;
                                if (null != fdef) {
                                    var fcall = fdef.Call;
                                    sb.AppendFormat("{0}{1} = {2}(this", GetIndentString(indent), mname, fcall.GetId());
                                    prestr = ", ";
                                    for (int ix = 1; ix < fcall.Params.Count;++ix ) {
                                        var param = fcall.Params[ix];
                                        string paramId = param.GetId();
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
                                    sb.AppendLine(")");
                                    ++indent;
                                    foreach (var comp in fdef.Statements) {
                                        GenerateSyntaxComponent(comp, sb, indent, true);
                                        sb.AppendLine(";");
                                    }
                                    --indent;
                                    sb.AppendFormatLine("{0}end,", GetIndentString(indent));
                                }
                            }
                        }
                        --indent;
                        sb.AppendFormatLine("{0}}};", GetIndentString(indent));
                    }

                    
		            sb.AppendFormatLine("{0}local instance_fields_build = function()", GetIndentString(indent));
                    ++indent;
			        sb.AppendFormatLine("{0}local instance_fields = {{", GetIndentString(indent));
                    ++indent;

                    var instFields = FindStatement(funcData, "instance_fields") as Dsl.FunctionData;
                    if (null != instFields) {
                        foreach (var def in instFields.Statements) {
                            var mdef = def as Dsl.CallData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                var comp = mdef.GetParam(1);
                                sb.AppendFormat("{0}{1} = ", GetIndentString(indent), mname);
                                GenerateFieldValueComponent(comp, sb, indent, false);
                                sb.AppendLine(",");
                            }
                        }
                    }
                    
                    --indent;
			        sb.AppendFormatLine("{0}}};", GetIndentString(indent));
			        sb.AppendFormatLine("{0}return instance_fields;", GetIndentString(indent));
                    --indent;
                    sb.AppendFormatLine("{0}end;", GetIndentString(indent));

                    var instanceProps = FindStatement(funcData, "instance_props") as Dsl.FunctionData;
                    if (null != instanceProps && instanceProps.GetStatementNum() > 0) {
                        sb.AppendFormatLine("{0}local instance_props = {{", GetIndentString(indent));
                        ++indent;
                        foreach (var def in instanceProps.Statements) {
                            var mdef = def as Dsl.CallData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                var prop = mdef.GetParam(1);
                                sb.AppendFormatLine("{0}{1} = {{", GetIndentString(indent), mname);
                                ++indent;
                                var body = prop as Dsl.FunctionData;
                                if (null != body) {
                                    foreach (var comp in body.Statements) {
                                        GenerateSyntaxComponent(comp, sb, indent, true);
                                        sb.AppendLine(",");
                                    }
                                }
                                --indent;
                                sb.AppendFormatLine("{0}}},", GetIndentString(indent));
                            }
                        }
                        --indent;
                        sb.AppendFormatLine("{0}}};", GetIndentString(indent));
                    } else {
                        sb.AppendFormatLine("{0}local instance_props = nil;", GetIndentString(indent));
                    }
                    var instanceEvents = FindStatement(funcData, "instance_events") as Dsl.FunctionData;
                    if (null != instanceEvents && instanceEvents.GetStatementNum() > 0) {
                        sb.AppendFormatLine("{0}local instance_events = {{", GetIndentString(indent));
                        ++indent;
                        foreach (var def in instanceProps.Statements) {
                            var mdef = def as Dsl.CallData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                var evt = mdef.GetParam(1);
                                sb.AppendFormatLine("{0}{1} = {{", GetIndentString(indent), mname);
                                ++indent;
                                var body = evt as Dsl.FunctionData;
                                if (null != body) {
                                    foreach (var comp in body.Statements) {
                                        GenerateSyntaxComponent(comp, sb, indent, true);
                                        sb.AppendLine(",");
                                    }
                                }
                                --indent;
                                sb.AppendFormatLine("{0}}},", GetIndentString(indent));
                            }
                        }
                        --indent;
                        sb.AppendFormatLine("{0}}};", GetIndentString(indent));
                    } else {
                        sb.AppendFormatLine("{0}local instance_events = nil;", GetIndentString(indent));
                    }

                    sb.AppendLine();

                    var interfaces = FindStatement(funcData, "interfaces") as Dsl.FunctionData;
                    if (null != interfaces && interfaces.GetStatementNum() > 0) {
                        sb.AppendFormatLine("{0}local interfaces = {{", GetIndentString(indent));
                        ++indent;
                        foreach (var def in interfaces.Statements) {
                            var mdef = def as Dsl.ValueData;
                            sb.AppendFormatLine("{0}\"{1}\",", GetIndentString(indent), mdef.GetId());
                        }
                        --indent;
                        sb.AppendFormatLine("{0}}};", GetIndentString(indent));
                    } else {
                        sb.AppendFormatLine("{0}local interfaces = nil;", GetIndentString(indent));
                    }
                    var interfaceMap = FindStatement(funcData, "interface_map") as Dsl.FunctionData;
                    if (null != interfaceMap && interfaceMap.GetStatementNum() > 0) {
                        sb.AppendFormatLine("{0}local interface_map = {{", GetIndentString(indent));
                        ++indent;
                        foreach (var def in interfaceMap.Statements) {
                            var mdef = def as Dsl.CallData;
                            if (mdef.GetId() == "=") {
                                string mname = mdef.GetParamId(0);
                                string mvalue = mdef.GetParamId(1);
                                sb.AppendFormatLine("{0}{1} = \"{2}\",", GetIndentString(indent), mname, mvalue);
                            }
                        }
                        --indent;
                        sb.AppendFormatLine("{0}}};", GetIndentString(indent));
                    } else {
                        sb.AppendFormatLine("{0}local interface_map = nil;", GetIndentString(indent));
                    }

                    sb.AppendLine();

                    sb.AppendFormatLine("{0}return defineclass({1}, \"{2}\", static, static_methods, static_fields_build, static_props, static_events, instance_methods, instance_fields_build, instance_props, instance_events, interfaces, interface_map, {3});", GetIndentString(indent), null == baseClass ? "nil" : baseClass.GetId(), className, isValueType ? "true" : "false");

                    --indent;
                    sb.AppendFormatLine("{0}end,", GetIndentString(indent));
                    --indent;
                    sb.AppendFormatLine("{0}}};", GetIndentString(indent));
                    sb.AppendLine();
                    sb.AppendFormatLine("{0}{1}.__define_class();", GetIndentString(indent), className);
                }
            }
            File.WriteAllText(outputFile, sb.ToString());
        }
        private static void GenerateFieldValueComponent(Dsl.ISyntaxComponent comp, StringBuilder sb, int indent, bool firstLineUseIndent)
        {
            var valData = comp as Dsl.ValueData;
            if (null != valData) {
                GenerateConcreteSyntax(valData, sb, indent, firstLineUseIndent, true);
            } else {
                var callData = comp as Dsl.CallData;
                if (null != callData) {
                    GenerateConcreteSyntax(callData, sb, indent, firstLineUseIndent);
                } else {
                    var funcData = comp as Dsl.FunctionData;
                    if (null != funcData) {
                        GenerateConcreteSyntax(funcData, sb, indent, firstLineUseIndent);
                    } else {
                        var statementData = comp as Dsl.StatementData;
                        GenerateConcreteSyntax(statementData, sb, indent, firstLineUseIndent);
                    }
                }
            }
        }
        private static void GenerateSyntaxComponent(Dsl.ISyntaxComponent comp, StringBuilder sb, int indent, bool firstLineUseIndent)
        {
            var valData = comp as Dsl.ValueData;
            if (null != valData) {
                GenerateConcreteSyntax(valData, sb, indent, firstLineUseIndent);
            } else {
                var callData = comp as Dsl.CallData;
                if (null != callData) {
                    GenerateConcreteSyntax(callData, sb, indent, firstLineUseIndent);
                } else {
                    var funcData = comp as Dsl.FunctionData;
                    if (null != funcData) {
                        GenerateConcreteSyntax(funcData, sb, indent, firstLineUseIndent);
                    } else {
                        var statementData = comp as Dsl.StatementData;
                        GenerateConcreteSyntax(statementData, sb, indent, firstLineUseIndent);
                    }
                }
            }
        }
        private static void GenerateConcreteSyntax(Dsl.ValueData data, StringBuilder sb, int indent, bool firstLineUseIndent)
        {
            GenerateConcreteSyntax(data, sb, indent, firstLineUseIndent, false);
        }
        private static void GenerateConcreteSyntax(Dsl.ValueData data, StringBuilder sb, int indent, bool firstLineUseIndent, bool useSpecNil)
        {
            if (firstLineUseIndent) {
                sb.AppendFormat("{0}", GetIndentString(indent));
            }
            string id = data.GetId();
            switch (data.GetIdType()) {
                case (int)Dsl.ValueData.ID_TOKEN:
                case (int)Dsl.ValueData.NUM_TOKEN:
                case (int)Dsl.ValueData.BOOL_TOKEN:
                    if (id == "null") {
                        if (useSpecNil)
                            id = "__cs2lua_nil_field_value";
                        else
                            id = "nil";
                    }
                    sb.Append(id);
                    break;
                case (int)Dsl.ValueData.STRING_TOKEN:
                    sb.AppendFormat("\"{0}\"", id);
                    break;
            }
        }
        private static void GenerateConcreteSyntax(Dsl.CallData data, StringBuilder sb, int indent, bool firstLineUseIndent)
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
                id = ConvertOperator(id);
                int paramNum = data.GetParamNum();
                if (paramNum == 1) {
                    var param1 = data.GetParam(0);
                    sb.AppendFormat("({0} ", id);
                    GenerateSyntaxComponent(param1, sb, indent, false);
                    sb.Append(")");
                } else if (paramNum == 2) {
                    var param1 = data.GetParam(0);
                    var param2 = data.GetParam(1);
                    if (id == "=" && param1.GetId() == "multiassign") {
                        string varName = string.Format("__compiler_multiassign_{0}", data.GetLine());
                        sb.AppendFormat("var {0}", varName);
                        sb.AppendFormat(" {0} ", id);
                        GenerateSyntaxComponent(param2, sb, indent, false);
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
                        GenerateSyntaxComponent(param1, sb, indent, false);
                        sb.AppendFormat(" {0} ", id);
                        GenerateSyntaxComponent(param2, sb, indent, false);
                        if (id != "=")
                            sb.Append(")");
                    }
                }
            } else if (id == "comment") {
                sb.AppendFormat("--{0}", data.GetParamId(0));
            } else if (id == "local") {
                sb.Append("local ");
                string prestr = string.Empty;
                foreach (var param in data.Params) {
                    sb.Append(prestr);
                    GenerateSyntaxComponent(param, sb, indent, false);
                    prestr = ", ";
                }
            } else if (id == "return") {
                sb.Append("return ");
                if (data.GetParamNum() > 1)
                    sb.Append("[");
                string prestr = string.Empty;
                foreach (var param in data.Params) {
                    sb.Append(prestr);
                    GenerateSyntaxComponent(param, sb, indent, false);
                    prestr = ", ";
                }
                if (data.GetParamNum() > 1)
                    sb.Append("]");
            } else if (id == "execunary") {
                string op = data.GetParamId(0);
                op = ConvertOperator(op);
                sb.AppendFormat("({0} ", op);
                GenerateSyntaxComponent(data.GetParam(1), sb, indent, false);
                sb.Append(")");
            } else if (id == "execbinary") {
                string op = data.GetParamId(0);
                op = ConvertOperator(op);
                sb.Append("(");
                GenerateSyntaxComponent(data.GetParam(1), sb, indent, false);
                sb.AppendFormat(" {0} ", op);
                GenerateSyntaxComponent(data.GetParam(2), sb, indent, false);
                sb.Append(")");
            } else if (id == "getstatic") {
                var obj = data.Params[0];
                var member = data.Params[1];
                GenerateSyntaxComponent(obj, sb, indent, false);
                sb.AppendFormat(".{0}", member.GetId());
            } else if (id == "getinstance") {
                var obj = data.Params[0];
                var member = data.Params[1];
                GenerateSyntaxComponent(obj, sb, indent, false);
                sb.AppendFormat(".{0}", member.GetId());
            } else if (id == "setstatic") {
                var obj = data.Params[0];
                var member = data.Params[1];
                var val = data.Params[2];
                GenerateSyntaxComponent(obj, sb, indent, false);
                sb.AppendFormat(".{0}", member.GetId());
                sb.Append(" = ");
                GenerateSyntaxComponent(val, sb, indent, false);
            } else if (id == "setinstance") {
                var obj = data.Params[0];
                var member = data.Params[1];
                var val = data.Params[2];
                GenerateSyntaxComponent(obj, sb, indent, false);
                sb.AppendFormat(".{0}", member.GetId());
                sb.Append(" = ");
                GenerateSyntaxComponent(val, sb, indent, false);
            } else if (id == "callstatic") {
                var obj = data.Params[0];
                var member = data.Params[1];
                GenerateSyntaxComponent(obj, sb, indent, false);
                sb.AppendFormat(".{0}", member.GetId());
                sb.Append("(");
                string prestr = string.Empty;
                for (int ix = 2; ix < data.Params.Count; ++ix) {
                    var param = data.Params[ix];
                    sb.Append(prestr);
                    string paramId = param.GetId();
                    if (paramId == "...") {
                        sb.Append("...");
                        continue;
                    }
                    GenerateSyntaxComponent(param, sb, indent, false);
                    prestr = ", ";
                }
                sb.Append(")");
            } else if (id == "callinstance") {
                var obj = data.Params[0];
                var member = data.Params[1];
                GenerateSyntaxComponent(obj, sb, indent, false);
                sb.AppendFormat(":{0}", member.GetId());
                sb.Append("(");
                string prestr = string.Empty;
                for (int ix = 2; ix < data.Params.Count; ++ix) {
                    var param = data.Params[ix];
                    sb.Append(prestr);
                    string paramId = param.GetId();
                    if (paramId == "...") {
                        sb.Append("...");
                        continue;
                    }
                    GenerateSyntaxComponent(param, sb, indent, false);
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
                    GenerateSyntaxComponent(k, sb, indent, false);
                    sb.Append(" : ");
                    GenerateSyntaxComponent(v, sb, indent, false);
                    prestr = ", ";
                }
                sb.Append("}");
            } else if (id == "listinit" || id == "collectioninit" || id == "arrayinit") {
                sb.Append("[");
                string prestr = string.Empty;
                for (int ix = 0; ix < data.Params.Count; ++ix) {
                    var param = data.Params[ix];
                    sb.Append(prestr);
                    GenerateSyntaxComponent(param, sb, indent, false);
                    prestr = ", ";
                }
                sb.Append("]");
            } else if (id == "foreach") {
                sb.Append("for ");
                var param1 = data.GetParamId(0);
                sb.Append(param1);
                sb.Append(" in ");
                var param2 = data.GetParam(1);
                GenerateSyntaxComponent(param2, sb, indent, false);
                sb.Append(" do");
            } else {
                if (null != callData) {
                    GenerateSyntaxComponent(callData, sb, indent, false);
                } else if (id == "if") {
                    sb.Append("if ");
                } else if (id == "elseif") {
                    sb.Append("elseif ");
                } else if (id == "else") {
                    sb.Append("else ");
                } else if (id == "while") {
                    sb.Append("while ");
                } else if (id == "until") {
                    sb.Append("until ");
                } else if (id == "for") {
                    sb.Append("for ");
                } else {
                    sb.Append(id);
                }
                if (data.HaveParam()) {
                    if (id == "if" || id == "elseif" || id == "while" || id == "until" || id == "for") {
                    } else {
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
                    }
                    if (data.GetParamClass() != (int)Dsl.CallData.ParamClassEnum.PARAM_CLASS_PERIOD) {
                        string prestr = string.Empty;
                        for (int ix = 0; ix < data.Params.Count; ++ix) {
                            var param = data.Params[ix];
                            sb.Append(prestr);
                            string paramId = param.GetId();
                            if (paramId == "...") {
                                sb.Append("...");
                                continue;
                            }
                            GenerateSyntaxComponent(param, sb, indent, false);
                            prestr = ", ";
                        }
                    }
                    if (id == "if" || id == "elseif" || id == "while" || id == "until" || id == "for") {
                    } else {
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
                    if (id == "if" || id == "elseif") {
                        sb.Append(" then ");
                    } else if (id == "while" || id == "for") {
                        sb.Append(" do");
                    }
                }
            }
        }
        private static void GenerateConcreteSyntax(Dsl.FunctionData data, StringBuilder sb, int indent, bool firstLineUseIndent)
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
                    GenerateSyntaxComponent(comp, sb, indent, true);
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
                    sb.Append("local ");
                    GenerateSyntaxComponent(comp, sb, indent, false);
                }
            } else {
                GenerateConcreteSyntax(fcall, sb, indent, false);
                if (data.HaveStatement()) {
                    sb.AppendLine();
                    ++indent;
                    foreach (var comp in data.Statements) {
                        GenerateSyntaxComponent(comp, sb, indent, true);
                        sb.AppendLine(";");
                    }
                    --indent;
                    sb.AppendFormat("{0}end", GetIndentString(indent));
                }
            }
        }
        private static void GenerateConcreteSyntax(Dsl.StatementData data, StringBuilder sb, int indent, bool firstLineUseIndent)
        {
            if (firstLineUseIndent) {
                sb.AppendFormat("{0}", GetIndentString(indent));
            }
            foreach (var funcData in data.Functions) {
                var fcall = funcData.Call;
                GenerateConcreteSyntax(fcall, sb, indent, funcData == data.First ? false : true);
                if (funcData.HaveStatement()) {
                    sb.AppendLine();
                    ++indent;
                    foreach (var comp in funcData.Statements) {
                        GenerateSyntaxComponent(comp, sb, indent, true);
                        sb.AppendLine(";");
                    }
                    --indent;
                    if (funcData == data.Last) {
                        sb.AppendFormat("{0}end", GetIndentString(indent));
                    }
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
        private static string ConvertOperator(string id)
        {
            if (id == "!=") {
                id = "~=";
            } else if (id == "&&") {
                id = "and";
            } else if (id == "||") {
                id = "or";
            } else if (id == "!") {
                id = "not";
            }
            return id;
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

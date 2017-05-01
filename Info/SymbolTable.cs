﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace RoslynTool.CsToLua
{
    internal class SymbolTable
    {
        internal CSharpCompilation Compilation
        {
            get { return m_Compilation; }
        }
        internal IAssemblySymbol AssemblySymbol
        {
            get { return m_AssemblySymbol; }
        }
        internal Dictionary<string, INamedTypeSymbol> ExternTypes
        {
            get { return m_ExternTypes; }
        }
        internal Dictionary<string, INamedTypeSymbol> IgnoredTypes
        {
            get { return m_IgnoredTypes; }
        }
        internal Dictionary<string, INamespaceSymbol> NamespaceSymbols
        {
            get { return m_NamespaceSymbols; }
        }
        internal Dictionary<string, ClassSymbolInfo> ClassSymbols
        {
            get { return m_ClassSymbols; }
        }
        internal Dictionary<string, HashSet<string>> Requires
        {
            get { return m_Requires; }
        }
        internal Dictionary<string, ITypeSymbol> ExternEnums
        {
            get { return m_ExternEnums; }
        }
        internal Dictionary<string, List<SyntaxNode>> GenericTypeDefines
        {
            get { return m_GenericTypeDefines; }
        }
        internal Dictionary<string, INamedTypeSymbol> GenericTypeInstances
        {
            get { return m_GenericTypeInstances; }
        }
        internal List<ITypeParameterSymbol> TypeParameters
        {
            get { return m_TypeParameters; }
        }
        internal List<ITypeSymbol> TypeArguments
        {
            get { return m_TypeArguments; }
        }
        internal void SetTypeParamsAndArgs(List<ITypeParameterSymbol> typeParams, List<ITypeSymbol> typeArgs, INamedTypeSymbol refType)
        {
            m_TypeParameters.Clear();
            m_TypeArguments.Clear();
            m_TypeParameters.AddRange(typeParams);
            m_TypeArguments.AddRange(typeArgs);
            MergeTypeParamsAndArgs(m_TypeParameters, m_TypeArguments, refType);
        }
        internal ITypeSymbol FindTypeArgument(ITypeSymbol sym)
        {
            ITypeSymbol ret = sym;
            var refType = sym as ITypeParameterSymbol;
            if (null != refType) {
                int ix = IndexOfTypeParameter(m_TypeParameters, refType);
                if (ix >= 0) {
                    ret = m_TypeArguments[ix];
                }
            }
            return ret;
        }
        internal bool IsIgnoredSymbol(ITypeSymbol sym)
        {
            return m_IgnoredTypes.ContainsKey(ClassInfo.GetFullName(sym));
        }
        internal bool IsCs2DslSymbol(ISymbol sym)
        {
            if (sym.Kind == SymbolKind.Method) {
                return IsCs2DslSymbol(sym as IMethodSymbol);
            } else if (sym.Kind == SymbolKind.Field) {
                return IsCs2DslSymbol(sym as IFieldSymbol);
            } else if (sym.Kind == SymbolKind.Property) {
                return IsCs2DslSymbol(sym as IPropertySymbol);
            } else if (sym.Kind == SymbolKind.Event) {
                return IsCs2DslSymbol(sym as IEventSymbol);
            } else {
                var arrSym = sym as IArrayTypeSymbol;
                if (null != arrSym) {
                    return IsCs2DslSymbol(arrSym.ElementType);
                } else {
                    var typeSym = sym as ITypeSymbol;
                    if (null != typeSym) {
                        return IsCs2DslSymbol(typeSym);
                    } else {
                        return sym.ContainingAssembly == m_AssemblySymbol;
                    }
                }
            }
        }
        internal bool IsCs2DslSymbol(IMethodSymbol sym)
        {
            return sym.ContainingAssembly == m_AssemblySymbol && !m_ExternTypes.ContainsKey(ClassInfo.SpecialGetFullTypeName(sym.ContainingType, true));
        }
        internal bool IsCs2DslSymbol(IFieldSymbol sym)
        {
            return sym.ContainingAssembly == m_AssemblySymbol && !m_ExternTypes.ContainsKey(ClassInfo.SpecialGetFullTypeName(sym.ContainingType, true));
        }
        internal bool IsCs2DslSymbol(IPropertySymbol sym)
        {
            return sym.ContainingAssembly == m_AssemblySymbol && !m_ExternTypes.ContainsKey(ClassInfo.SpecialGetFullTypeName(sym.ContainingType, true));
        }
        internal bool IsCs2DslSymbol(IEventSymbol sym)
        {
            return sym.ContainingAssembly == m_AssemblySymbol && !m_ExternTypes.ContainsKey(ClassInfo.SpecialGetFullTypeName(sym.ContainingType, true));
        }
        internal bool IsCs2DslSymbol(ITypeSymbol sym)
        {
            return sym.ContainingAssembly == m_AssemblySymbol && !m_ExternTypes.ContainsKey(ClassInfo.SpecialGetFullTypeName(sym, true));
        }
        internal void Init(CSharpCompilation compilation)
        {
            m_Compilation = compilation;
            m_AssemblySymbol = compilation.Assembly;
            INamespaceSymbol nssym = m_AssemblySymbol.GlobalNamespace;
            InitRecursively(nssym);
        }
        internal void AddRequire(string refClass, string moduleName)
        {
            HashSet<string> hashset;
            if (!m_Requires.TryGetValue(refClass, out hashset)) {
                hashset = new HashSet<string>();
                m_Requires.Add(refClass, hashset);
            }
            if (!hashset.Contains(moduleName)) {
                hashset.Add(moduleName);
            }
        }
        internal void AddExternEnum(string enumKey, ITypeSymbol sym)
        {
            if (!m_ExternEnums.ContainsKey(enumKey)) {
                m_ExternEnums.Add(enumKey, sym);
            }
        }
        internal void AddGenericTypeDefine(string key, SyntaxNode node)
        {
            List<SyntaxNode> defines;
            if (!m_GenericTypeDefines.TryGetValue(key, out defines)) {
                defines = new List<SyntaxNode>();
                m_GenericTypeDefines.Add(key, defines);
            }
            defines.Add(node);
        }
        internal void AddGenericTypeInstance(string key, INamedTypeSymbol sym)
        {
            if (!m_GenericTypeInstances.ContainsKey(key)) {
                m_GenericTypeInstances.Add(key, sym);
            }
        }
        internal string NameMangling(IMethodSymbol sym)
        {
            string ret = GetMethodName(sym);
            if (!string.IsNullOrEmpty(ret) && ret[0] == '.')
                ret = ret.Substring(1);
            string key = ClassInfo.GetFullNameWithTypeParameters(sym.ContainingType);
            ClassSymbolInfo csi;
            if (m_ClassSymbols.TryGetValue(key, out csi)) {
                bool isMangling;
                csi.SymbolOverloadFlags.TryGetValue(ret, out isMangling);
                if (isMangling) {
                    ret = CalcMethodMangling(sym);
                }
            }
            return ret;
        }
        internal bool IsFieldCreateSelf(IFieldSymbol sym)
        {
            bool ret = false;
            string key = ClassInfo.GetFullName(sym.ContainingType);
            ClassSymbolInfo csi;
            if (m_ClassSymbols.TryGetValue(key, out csi)) {
                ret = csi.FieldCreateSelfs.ContainsKey(sym.Name);
            }
            return ret;
        }
        internal bool IsUseExplicitTypeParam(IFieldSymbol sym)
        {
            bool ret = false;
            string key = ClassInfo.GetFullName(sym.ContainingType);
            ClassSymbolInfo csi;
            if (m_ClassSymbols.TryGetValue(key, out csi)) {
                ret = csi.FieldUseExplicitTypeParams.ContainsKey(sym.Name);
            }
            return ret;
        }

        private void InitRecursively(INamespaceSymbol nssym)
        {
            if (null != nssym) {
                string ns = ClassInfo.GetNamespaces(nssym);
                m_NamespaceSymbols.Add(ns, nssym);
                foreach (var typeSym in nssym.GetTypeMembers()) {
                    InitRecursively(typeSym);
                }
                foreach (var newSym in nssym.GetNamespaceMembers()) {
                    InitRecursively(newSym);
                }                
            }
        }
        private void InitRecursively(INamedTypeSymbol typeSym)
        {
            string key = ClassInfo.GetFullName(typeSym);
            if (!m_ClassSymbols.ContainsKey(key)) {
                ClassSymbolInfo csi = new ClassSymbolInfo();
                m_ClassSymbols.Add(key, csi);
                csi.Init(typeSym, m_Compilation, this);
            }
            foreach (var newSym in typeSym.GetTypeMembers()) {
                InitRecursively(newSym);
            }
        }
        private SymbolTable() { }

        private CSharpCompilation m_Compilation = null;
        private IAssemblySymbol m_AssemblySymbol = null;
        private Dictionary<string, INamedTypeSymbol> m_ExternTypes = new Dictionary<string, INamedTypeSymbol>();
        private Dictionary<string, INamedTypeSymbol> m_IgnoredTypes = new Dictionary<string, INamedTypeSymbol>();
        private Dictionary<string, INamespaceSymbol> m_NamespaceSymbols = new Dictionary<string, INamespaceSymbol>();
        private Dictionary<string, ClassSymbolInfo> m_ClassSymbols = new Dictionary<string, ClassSymbolInfo>();
        private Dictionary<string, HashSet<string>> m_Requires = new Dictionary<string, HashSet<string>>();
        private Dictionary<string, ITypeSymbol> m_ExternEnums = new Dictionary<string, ITypeSymbol>();
        
        private Dictionary<string, List<SyntaxNode>> m_GenericTypeDefines = new Dictionary<string, List<SyntaxNode>>();
        private Dictionary<string, INamedTypeSymbol> m_GenericTypeInstances = new Dictionary<string, INamedTypeSymbol>();

        private List<ITypeParameterSymbol> m_TypeParameters = new List<ITypeParameterSymbol>();
        private List<ITypeSymbol> m_TypeArguments = new List<ITypeSymbol>();
        
        internal static SymbolTable Instance
        {
            get
            {
                return s_Instance;
            }
        }
        private static SymbolTable s_Instance = new SymbolTable();
        
        internal static void MergeTypeParamsAndArgs(List<ITypeParameterSymbol> tParams, List<ITypeSymbol> tArgs, INamedTypeSymbol refType)
        {
            var typeParams = refType.TypeParameters;
            var typeArgs = refType.TypeArguments;
            for (int i = 0; i < typeParams.Length && i < typeArgs.Length; ++i) {
                var t1 = typeParams[i];
                var t2 = typeArgs[i];
                int ix = IndexOfTypeParameter(tParams, t1);
                if (t2.TypeKind != TypeKind.TypeParameter) {
                    if (ix < 0) {
                        tParams.Add(t1);
                        tArgs.Add(t2);
                    } else {
                        tArgs[ix] = t2;
                    }
                } else {
                    int ix2 = IndexOfTypeParameter(tParams, t2);
                    if (ix < 0) {
                        if (ix2 >= 0) {
                            tParams.Add(t1);
                            tArgs.Add(tArgs[ix2]);
                        }
                    } else {
                        if (ix2 >= 0) {
                            tArgs[ix] = tArgs[ix2];
                        }
                    }
                }
            }
            if (null != refType.ContainingType) {
                MergeTypeParamsAndArgs(tParams, tArgs, refType.ContainingType);
            }
        }
        internal static int IndexOfTypeParameter(List<ITypeParameterSymbol> tParams, ITypeSymbol t)
        {
            string name = ClassInfo.SpecialGetFullTypeNameWithTypeParameters(t);
            for (int i = 0; i < tParams.Count; ++i) {
                if (name == ClassInfo.SpecialGetFullTypeNameWithTypeParameters(tParams[i])) {
                    return i;
                }
            }
            return -1;
        }
        internal static string CalcMethodMangling(IMethodSymbol methodSym)
        {
            IAssemblySymbol assemblySym = SymbolTable.Instance.AssemblySymbol;
            if (null == methodSym)
                return string.Empty;
            StringBuilder sb = new StringBuilder();
            string name = GetMethodName(methodSym);
            if (!string.IsNullOrEmpty(name) && name[0] == '.')
                name = name.Substring(1);
            sb.Append(name);
            if (methodSym.ContainingAssembly == assemblySym) {
                foreach (var param in methodSym.Parameters) {
                    sb.Append("__");
                    if (param.RefKind == RefKind.Ref) {
                        sb.Append("Ref_");
                    } else if (param.RefKind == RefKind.Out) {
                        sb.Append("Out_");
                    }
                    var oriparam = param.OriginalDefinition;
                    if (oriparam.Type.Kind == SymbolKind.ArrayType) {
                        sb.Append("Arr_");
                        var arrSym = oriparam.Type as IArrayTypeSymbol;
                        string fn;
                        if (arrSym.ElementType.TypeKind == TypeKind.TypeParameter) {
                            fn = ClassInfo.GetFullNameWithTypeParameters(arrSym.ElementType);
                        } else {
                            fn = ClassInfo.GetFullName(arrSym.ElementType);
                        }
                        sb.Append(fn.Replace('.', '_'));
                    } else if (oriparam.Type.TypeKind == TypeKind.TypeParameter) {
                        string fn = ClassInfo.GetFullNameWithTypeParameters(oriparam.Type);
                        sb.Append(fn.Replace('.', '_'));
                    } else {
                        string fn = ClassInfo.GetFullName(oriparam.Type);
                        sb.Append(fn.Replace('.', '_'));
                    }
                }
            }
            return sb.ToString();
        }
        internal static string GetMethodName(IMethodSymbol sym)
        {
            if (null == sym) {
                return string.Empty;
            }
            if (sym.ExplicitInterfaceImplementations.Length > 0) {
                int ix = sym.Name.LastIndexOf('.');
                return ClassInfo.CalcNameWithFullTypeName(sym.Name.Substring(ix + 1), sym.ContainingType);
            } else {
                return sym.Name;
            }
        }
        internal static string GetPropertyName(IPropertySymbol sym)
        {
            if (null == sym) {
                return string.Empty;
            }
            if (sym.ExplicitInterfaceImplementations.Length > 0) {
                int ix = sym.Name.LastIndexOf('.');
                return ClassInfo.CalcNameWithFullTypeName(sym.Name.Substring(ix + 1), sym.ContainingType);
            } else {
                return sym.Name;
            }
        }
        internal static string GetEventName(IEventSymbol sym)
        {
            if (null == sym) {
                return string.Empty;
            }
            if (sym.ExplicitInterfaceImplementations.Length > 0) {
                int ix = sym.Name.LastIndexOf('.');
                return ClassInfo.CalcNameWithFullTypeName(sym.Name.Substring(ix + 1), sym.ContainingType);
            } else {
                return sym.Name;
            }
        }
        internal static string CheckDslKeyword(string name, out bool change)
        {
            if (name.StartsWith("@")) {
                change = true;
                return "__compiler_cs_" + name.Substring(1);
            } else if (s_ExtraDslKeywords.Contains(name)) {
                change = true;
                return "__compiler_dsl_" + name;
            } else {
                change = false;                
                return name;
            }
        }
        internal static bool IsBasicValueProperty(IPropertySymbol sym)
        {
            bool ret = false;
            if (null != sym && !sym.IsStatic && null != sym.ContainingType) {
                if (ClassInfo.GetFullName(sym.ContainingType) == SymbolTable.PrefixExternClassName("System.Enum")) {
                    ret = true;
                } else {
                    string type = ClassInfo.GetFullName(sym.ContainingType);
                    ret = IsBasicType(type, true);
                }
            }
            return ret;
        }
        internal static bool IsBasicValueMethod(IMethodSymbol sym)
        {
            bool ret = false;
            if (null != sym && !sym.IsStatic && null != sym.ContainingType) {
                if (ClassInfo.GetFullName(sym.ContainingType) == SymbolTable.PrefixExternClassName("System.Enum")) {
                    ret = true;
                } else {
                    string type = ClassInfo.GetFullName(sym.ContainingType);
                    ret = IsBasicType(type, true);
                }
            }
            return ret;
        }
        internal static bool IsBasicType(ITypeSymbol type)
        {
            return IsBasicType(type, true);
        }
        internal static bool IsBasicType(ITypeSymbol type, bool includeString)
        {
            bool ret = false;
            if (type.TypeKind == TypeKind.Enum || ClassInfo.GetFullName(type) == SymbolTable.PrefixExternClassName("System.Enum")) {
                ret = true;
            } else {
                string typeName = ClassInfo.GetFullName(type);
                ret = IsBasicType(typeName, includeString);
            }
            return ret;
        }
        internal static bool IsBasicType(string type, bool includeString)
        {
            string t = UnPrefixExternClassName(type);
            if (includeString && t == "System.String")
                return true;
            return s_BasicTypes.Contains(t);
        }
        internal static bool IsIntegerType(ITypeSymbol type)
        {
            bool ret = false;
            if (type.TypeKind == TypeKind.Enum || ClassInfo.GetFullName(type) == SymbolTable.PrefixExternClassName("System.Enum")) {
                ret = true;
            } else {
                string typeName = ClassInfo.GetFullName(type);
                ret = IsIntegerType(typeName);
            }
            return ret;
        }
        internal static bool IsIntegerType(string type)
        {
            string t = UnPrefixExternClassName(type);
            return s_IntegerTypes.Contains(t);
        }
        internal static string PrefixExternClassName(string cn)
        {
            string prefix = s_ExternClassNamePrefix;
            if (string.IsNullOrEmpty(prefix))
                return cn;
            else
                return prefix + cn;
        }
        internal static string UnPrefixExternClassName(string cn)
        {
            string prefix = s_ExternClassNamePrefix;
            if (cn.StartsWith(prefix))
                return cn.Substring(prefix.Length);
            else
                return cn;
        }
        internal static void SetExternClassNamePrefix(string val)
        {
            s_ExternClassNamePrefix = val;
        }
        internal static bool NoAutoRequire
        {
            get { return s_NoAutoRequire; }
            set { s_NoAutoRequire = value; }
        }
        internal static bool DslComponentByString
        {
            get { return s_DslComponentByString; }
            set { s_DslComponentByString = value; }
        }
        internal static bool UseArrayGetSet
        {
            get { return s_UseArrayGetSet; }
            set { s_UseArrayGetSet = value; }
        }
        internal static bool ArrayLowerBoundIsOne
        {
            get { return s_ArrayLowerBoundIsOne; }
            set { s_ArrayLowerBoundIsOne = value; }
        }

        private static string s_ExternClassNamePrefix = string.Empty;
        private static bool s_NoAutoRequire = false;
        private static bool s_DslComponentByString = false;
        private static bool s_UseArrayGetSet = false;
        private static bool s_ArrayLowerBoundIsOne = false;

        private static HashSet<string> s_ExtraDslKeywords = new HashSet<string> {
            "and", "elseif", "end", "function", "local", "null", "not", "or", "repeat", "then", "until"
        };
        private static HashSet<string> s_BasicTypes = new HashSet<string> {
            "System.Boolean", "System.Byte", "System.SByte", "System.Int16", "System.UInt16", "System.Int32", "System.UInt32", "System.Int64", "System.UInt64", "System.Single", "System.Double"
        };
        private static HashSet<string> s_IntegerTypes = new HashSet<string> {
            "System.Byte", "System.SByte", "System.Int16", "System.UInt16", "System.Int32", "System.UInt32", "System.Int64", "System.UInt64"
        };
    }
}

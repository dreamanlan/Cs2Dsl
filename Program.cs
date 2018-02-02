﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynTool.CsToDsl;
using System.Diagnostics;

namespace RoslynTool
{
    partial class Program
    {
        static int Main(string[] args)
        {
            try {
                string file = "test.cs";
                List<string> macros = new List<string>();
                List<string> ignoredPath = new List<string>();
                List<string> externPath = new List<string>();
                List<string> internPath = new List<string>();
                Dictionary<string, string> refByNames = new Dictionary<string, string>();
                Dictionary<string, string> refByPaths = new Dictionary<string, string>();
                bool enableInherit = false;
                bool enableLinq = false;
                bool outputResult = false;
                bool parallel = false;
                if (args.Length > 0) {
                    for (int i = 0; i < args.Length; ++i) {
                        if (0 == string.Compare(args[i], "-d", true)) {
                            if (i < args.Length - 1) {
                                string arg = args[i + 1];
                                if (!arg.StartsWith("-")) {
                                    macros.Add(arg);
                                    ++i;
                                }
                            }
                        } else if (0 == string.Compare(args[i], "-ignorepath", true)) {
                            if (i < args.Length - 1) {
                                string arg = args[i + 1];
                                if (!arg.StartsWith("-")) {
                                    ignoredPath.Add(arg);
                                    ++i;
                                }
                            }
                        } else if (0 == string.Compare(args[i], "-externpath", true)) {
                            if (i < args.Length - 1) {
                                string arg = args[i + 1];
                                if (!arg.StartsWith("-")) {
                                    externPath.Add(arg);
                                    ++i;
                                }
                            }
                        } else if (0 == string.Compare(args[i], "-internpath", true)) {
                            if (i < args.Length - 1) {
                                string arg = args[i + 1];
                                if (!arg.StartsWith("-")) {
                                    internPath.Add(arg);
                                    ++i;
                                }
                            }
                        } else if (0 == string.Compare(args[i], "-systemdllpath", true)) {
                            if (i < args.Length - 1) {
                                string arg = args[i + 1];
                                if (!arg.StartsWith("-")) {
                                    SymbolTable.SystemDllPath = arg;
                                    ++i;
                                }
                            }
                        } else if (0 == string.Compare(args[i], "-src", true)) {
                            if (i < args.Length - 1) {
                                string arg = args[i + 1];
                                if (!arg.StartsWith("-")) {
                                    file = arg;
                                    if (!File.Exists(file)) {
                                        Console.WriteLine("file path not found ! {0}", file);
                                    }
                                    ++i;
                                }
                            }
                        } else if (0 == string.Compare(args[i], "-enableinherit", true)) {
                            enableInherit = true;
                        } else if (0 == string.Compare(args[i], "-enablelinq", true)) {
                            enableLinq = true;
                        } else if (0 == string.Compare(args[i], "-outputresult", true)) {
                            outputResult = true;
                        } else if (0 == string.Compare(args[i], "-parallel", true)) {
                            parallel = true;
                        } else if (0 == string.Compare(args[i], "-noautorequire", true)) {
                            SymbolTable.NoAutoRequire = true;
                        } else if (0 == string.Compare(args[i], "-componentbystring", true)) {
                            SymbolTable.DslComponentByString = true;
                        } else if (0 == string.Compare(args[i], "-usearraygetset", true)) {
                            SymbolTable.UseArrayGetSet = true;
                        } else if (0 == string.Compare(args[i], "-arraylowerboundisone", true)) {
                            SymbolTable.ArrayLowerBoundIsOne = true;
                        } else if (0 == string.Compare(args[i], "-refbyname", true)) {
                            string name = string.Empty, alias = "global";
                            if (i < args.Length - 1) {
                                string arg = args[i + 1];
                                if (!arg.StartsWith("-")) {
                                    name = arg;
                                    ++i;
                                } else {
                                    continue;
                                }
                            } else {
                                continue;
                            }
                            if (i < args.Length - 1) {
                                string arg = args[i + 1];
                                if (!arg.StartsWith("-")) {
                                    alias = arg;
                                    ++i;
                                }
                            }
                            if (!refByNames.ContainsKey(name)) {
                                refByNames.Add(name, alias);
                            } else {
                                Console.WriteLine("refbyname duplicate, ignored ! {0}={1}", name, alias);
                            }
                        } else if (0 == string.Compare(args[i], "-refbypath", true)) {
                            string path = string.Empty, alias = "global";
                            if (i < args.Length - 1) {
                                string arg = args[i + 1];
                                if (!arg.StartsWith("-")) {
                                    path = arg;
                                    ++i;
                                } else {
                                    continue;
                                }
                            } else {
                                continue;
                            }
                            if (i < args.Length - 1) {
                                string arg = args[i + 1];
                                if (!arg.StartsWith("-")) {
                                    alias = arg;
                                    ++i;
                                }
                            }
                            if (!File.Exists(path)) {
                                Console.WriteLine("refbypath path not found ! {0}={1}", path, alias);
                            } else {
                                if (!refByPaths.ContainsKey(path)) {
                                    refByPaths.Add(path, alias);
                                } else {
                                    Console.WriteLine("refbypath duplicate, ignored ! {0}={1}", path, alias);
                                }
                            }
                        } else {
                            file = args[i];
                            if (!File.Exists(file)) {
                                Console.WriteLine("file path not found ! {0}", file);
                            }
                            break;
                        }
                    }
                } else {
                    Console.WriteLine("[Usage]:Cs2Dsl [-enableinherit] [-enablelinq] [-outputresult] [-noautorequire] [-componentbystring] [-usearraygetset] [-arraylowerboundisone] [-d macro] [-ignorepath path] [-refbyname dllname alias] [-refbypath dllpath alias] [-systemdllpath dllpath] [-src] csfile|csprojfile");
                    Console.WriteLine("\twhere:");
                    Console.WriteLine("\t\tmacro = c# macro define, used in your csharp code #if/#elif/#else/#endif etc.");
                    Console.WriteLine("\t\tinternpath = only c# source file path in the csproj as intern class, only these classes translate to dsl.");
                    Console.WriteLine("\t\texternpath = mark c# source file path in the csproj as extern class (API), these classes doesn't translate to dsl.");
                    Console.WriteLine("\t\tignorepath = ignore c# source file path in the csproj, these classes doesn't translate to dsl (need translate them by hand, cs2lua use \"require 'cs2dsl_custom';\" resolve xref).");
                    Console.WriteLine("\t\tdllname = dotnet system assembly name, referenced by your csharp code.");
                    Console.WriteLine("\t\tdllpath = dotnet assembly path, referenced by your csharp code.");
                    Console.WriteLine("\t\talias = global for default or some dll toplevel namespace alias, used in your csharp code such as 'extern alias ui;'.");
                    Console.WriteLine();
                    if (File.Exists(file)) {
                        Console.WriteLine("now will process test csharp code test.cs in current directory ...");
                        Console.WriteLine();
                    }
                }
                if (File.Exists(file)) {
                    var stopwatch = Stopwatch.StartNew();
                    var result = (int)CsToDslProcessor.Process(file, macros, ignoredPath, externPath, internPath, refByNames, refByPaths, enableInherit, enableLinq, outputResult, parallel);
                    stopwatch.Stop();
                    Console.WriteLine("RunningTime: {0}s", stopwatch.Elapsed.TotalSeconds);
                    return result;
                } else {
                    return (int)ExitCode.FileNotFound;
                }
            } catch (Exception ex) {
                Console.WriteLine("exception:{0}", ex.Message);
                Console.WriteLine("{0}", ex.StackTrace);
                while (null != ex.InnerException) {
                    ex = ex.InnerException;

                    Console.WriteLine("inner exception:{0}", ex.Message);
                    Console.WriteLine("{0}", ex.StackTrace);
                }
                return (int)ExitCode.Exception;
            }
        }
    }
}

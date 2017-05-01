using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Cs2Dsl.Ignore]
class LuaConsole
{
    public static void Print(params object[] args)
    {
    }
}

namespace TopLevel 
{
    using lua = LuaConsole;
    
    namespace Child1
    {   
        class GenericClass<T> where T : new()
        {
            public static event Action OnAction0;
            public static Action OnAction1;
            public event Action OnAction2;
            public Action OnAction3;
            public int Prop
            {
                get { return 1; }
                set { m_Test = value; }
            }
            public int PropErr
            {
                get;
                set;
            } = 456;
            public int this[int v1, int v2]
            {
                get
                {
                    return 123;
                }
                set
                {

                }
            }
            public event Action AAA
            {
                add { OnAction0 += value; }
                remove { OnAction0 -= value; }
            }
            public void Test(T v)
            {
                T a = default(T);
                GameObject obj = new GameObject();
                obj.GetComponent<Renderer>();
                lua.Print(v is int);
                string path = "";
                using (var fs = new StreamWriter(path)) {
                    lua.Print(fs.ToString());
                }
                global::System.String s = "test";
                var obj = new { a=123, b=456 };
                lua.Print(obj.a);
                int v = this[1,2];
                this[2,3]=456;
            }
            public void Test(int vv, float bb)
            {

            }
            public GenericClass(ref int v, out int v2)
            {
                m_Test = v + 456;
                v = 123;
                v2 = 789;
            }
            private int m_Test = 123;
            private int[] m_Array = new int[]{1,2,3,4};
            private int m_Test2 = m_Test + 1;

            static GenericClass()
            {
                s_Test = 9876;
            }
            private static int s_Test = 8765;
        }
        delegate void SimpleEventHandler();
        class Foo
        {
            public event SimpleEventHandler OnSimple;
            public SimpleEventHandler OnSimple2;

            public Foo():this(0)
            {}
            public Foo(int v)
            {
                this.m_Test = v;
            }

            public void GTest(GenericClass<int> arg){}
            public void GTest(GenericClass<float> arg){}

            public void Test(int a, ref int b, out int c, params int[] args)
            {
                Func<int, int> f = p1 => p1;
                f(1);
                Func<int, int, int> f2 = (p1, p2) => { return p1 + p2; };
                f2(1, 2);
                m_Test = a + b + 123;
                b = a < b ? a : b;
                if (a > 0)
                {
                    c = a + b + args[0];
                }
                int? v = args?.Length;
                if (a < b) {
                    c = b - a;
                } else if (a >= b) {
                    c = a - b;
                }
                if (a < b) {
                    c = b - a;
                } else {
                    c = a - b;
                }
                if (a < b) {
                    c = b - a;
                } else if (a < c) {
                    c = a - b;
                } else {
                    c = 0;
                }
            }

            public int Test2(int a, int b, ref int c, out int d)
            {
                c += a + b;
                d = c * 2;
                int a = c+Test(d);
                a+=Test(2);
                return c;
            }

            public int Test(int v)
            {
                int v1 = 0, v2;
                GenericClass<int> f = new GenericClass<int>(ref v1, out v2);
                int v = f[3,4];
                f[4,5] = 6;
                f[4,5] = Test2(1,2,ref v1, out v2);
                v = Test2(1,2,ref v1, out v2);
                Test2(1,2,ref v1, out v2);
                v = 1+Test2(1,2,ref v1, out v2);
                v+=Test2(1,2,ref v1, out v2);
                v2=(v=Test2(1,2,ref v1, out v2));
                f.Test(v);
                OnSimple += () => { };
                {
                    OnSimple2 += () => { };
                }
            }

            internal int m_Test = 0;
            internal int m_Test2 = 0;
        } 
    } 
    
    namespace Child2 
    { 
        enum TestEnum
        {
            One = 0,
            Two,
            Three,
        }
        [Cs2Dsl.Ignore]
        struct Point
        {
            [Cs2Dsl.Ignore]
            public static float X;
            public static float Y;
        }
        class Bar
        {
            public Bar(int i, int j)
            {

            }
            delegate void IntHandler(int v);
            public void Handler()
            {
                var f = delegate() {
                    LuaConsole.Print(1, 2, 3);
                };
                Test(123);
                IntHandler t = Test;
                t(1);
                IntHandler t2 = this.Test;
                t2(2);
                IntHandler aa;
                aa=Test;
                TestDelegate(Test);
            }
            public void TestDelegate(IntHandler handler)
            {

            }
            public void Test()
            {
                var F = Child2.Bar.s_Test;
                Child1.Foo f = new Child1.Foo(123);
                f.OnSimple = this.Handler;
                var ff = new Child1.Foo { m_Test = 456, m_Test2 = 789 };
                int a = 0, b = 0, c = 0;
                b = (c += (int)2);
                List<List<int>> list = new List<List<int>> { new List<int> { 1, 2 }, new List<int> { 3, 4, 5 } };
                list.Add(new List<int>() { 123, 456 });
                var v = list[0];
                Dictionary<string, string> dict = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };
                Test(dict);
                f.Test(1, ref b, out c, 3);
                LuaConsole.Print(b, c);
                int r = 0;
                r += f.Test2(1, 2, ref b, out c);
                LuaConsole.Print(r, b, c);
                int v0 = f.Test2(3, 4, ref b, out c);
                var v = ff ?? f;

                var vv = f?.m_Test;
                var vvv = f?.Test(123);
                
                v = f.Test2(3, 4, ref b, out c);
                LuaConsole.Print(v, b, c);
                while (a < 10 + 2) {
                    ++a;
                    if (a < 5 + 3) continue;
                }
                do
                {
                    ++b;
                }while(b<100);
                int[] abc = new int[256];
                int[] def = new int[] { 1, 2, 3, 4, 5 };
                int[][][] g0 = new int[3][][];
                int[, ,] h0 = new int[3, 5, 7];
                int[][] g = new int[][] { new int[] { 1, 2 }, new int[] { 3, 4 } };
                int[,] h = new int[,] { { 1, 2 }, { 3, 4 } };

                for (int i = 0; i < g0.Length; ++i) {
                    g0[i] = new int[3][];
                    if (1 < 2) break;
                    if (2 < 3) continue;
                    for (int j = 0; j < g0[i].Length; ++j) {
                        g0[i][j] = new int[3];
                    }
                }

                for (int i = 0; i < h0.GetLength(0); ++i) {
                    for (int j = 0; j < h0.GetLength(1); ++j) {
                        for (int k = 0; k < h0.GetLength(2); ++k) {
                            h0[i, j, k] = i * j * k;
                        }
                    }
                }

                var hh = new[] { 5 + 2, 6, 7, 8 };
                Test(hh);

                switch (a) {
                    case 1:
                    case 3:
                        if (a == 1)
                            break;
                        break;
                    case 4:
                    default:
                        break;
                    case 2:
                        break;
                }

                foreach (var i in def) {
                    s_Test += hh?[i];
                }
                Test(new[] { 1, 2, 3 });
                Test(new int[] { 1, 2, 3 });
                Test(new Dictionary<string, string> { { "1", "2" }, { "3", "4" } });
            }
            public void Test(int v)
            {
                LuaConsole.Print(v);
            }
            public void Test(int[] arr)
            {

            }
            public void Test(Dictionary<string, string> dict)
            {

            }

            public static int s_Test = 123;
        } 
    } 
}
/*
local obj = TopLevel.Child2.Bar:new();
obj:Test();
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Cs2Dsl.Ignore]
class LuaConsole
{
    public static void Print(params object[] args)
    {
    }
}

public interface ICs2LuaPoolAllocatedObjectEx<T> where T : ICs2LuaPoolAllocatedObjectEx<T>
{
    void InitPool(Cs2LuaObjectPoolEx<T> pool);
    T Downcast();
}
public sealed class Cs2LuaObjectPoolEx<T> where T : ICs2LuaPoolAllocatedObjectEx<T>
{
    public void Init(Func<T> creater, Action<T> destroyer)
    {
        m_Creater = creater;
        m_Destroyer = destroyer;
    }
    public void Init(int initPoolSize, Func<T> creater, Action<T> destroyer)
    {
        m_Creater = creater;
        m_Destroyer = destroyer;
        for (int i = 0; i < initPoolSize; ++i) {
            T t = creater();
            t.InitPool(this);
            m_UnusedObjects.Enqueue(t);
        }
    }
    public T Alloc()
    {
        if (m_UnusedObjects.Count > 0)
            return m_UnusedObjects.Dequeue();
        else {
            T t = m_Creater();
            if (null != t) {
                t.InitPool(this);
            }
            return t;
        }
    }
    public void Recycle(ICs2LuaPoolAllocatedObjectEx<T> t)
    {
        if (null != t) {
            m_UnusedObjects.Enqueue(t.Downcast());
        }
    }
    public void Clear()
    {
        if (null != m_Destroyer) {
            foreach (var item in m_UnusedObjects) {
                m_Destroyer(item);
            }
        }
        m_UnusedObjects.Clear();
    }
    public int Count
    {
        get {
            return m_UnusedObjects.Count;
        }
    }

    private Queue<T> m_UnusedObjects = new Queue<T>();
    private Func<T> m_Creater = null;
    private Action<T> m_Destroyer = null;
}

internal sealed class DataChangeCallBackInfo : ICs2LuaPoolAllocatedObjectEx<DataChangeCallBackInfo>
{
    public int m_ActorId;

    public DataChangeCallBackInfo()
    {
        reset();
    }
    public void reset()
    {
        m_ActorId = 0;
    }
    public DataChangeCallBackInfo Downcast()
    {
        return this;
    }
    public void InitPool(Cs2LuaObjectPoolEx<DataChangeCallBackInfo> pool)
    {

    }
}

class Test
{
    public void Init()
    {
        m_DataChangeCallBackInfoPool.Init(null, null);
    }
    private Cs2LuaObjectPoolEx<DataChangeCallBackInfo> m_DataChangeCallBackInfoPool = new Cs2LuaObjectPoolEx<DataChangeCallBackInfo>();
}
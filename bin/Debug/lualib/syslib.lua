local function get_basic_type_func()
    return function(v)
        return v
    end
end

Slua = Slua or {out = {}, nil_field_value = {}}
__cs2lua_out = Slua.out

System = System or {}
System.Boolean = System.Boolean or get_basic_type_func()
System.SByte = System.SByte or get_basic_type_func()
System.Byte = System.Byte or get_basic_type_func()
System.Char = System.Char or get_basic_type_func()
System.Int16 = System.Int16 or get_basic_type_func()
System.Int32 = System.Int32 or get_basic_type_func()
System.Int64 = System.Int64 or get_basic_type_func()
System.UInt16 = System.UInt16 or get_basic_type_func()
System.UInt32 = System.UInt32 or get_basic_type_func()
System.UInt64 = System.UInt64 or get_basic_type_func()
System.Single = System.Single or get_basic_type_func()
System.Double = System.Double or get_basic_type_func()
System.String = System.String or get_basic_type_func()
System.Collections = System.Collections or {}
System.Collections.Generic = System.Collections.Generic or {}
System.Collections.Generic.List_T = {
    __cs2lua_defined = true,
    __cs2lua_fullname = "System.Collections.Generic.List_T",
    __cs2lua_typename = "List",
    __exist = function(k)
        return false
    end
}
System.Collections.Generic.Queue_T = {
    __cs2lua_defined = true,
    __cs2lua_fullname = "System.Collections.Generic.Queue_T",
    __cs2lua_typename = "Queue",
    __exist = function(k)
        return false
    end
}
System.Collections.Generic.Stack_T = {
    __cs2lua_defined = true,
    __cs2lua_fullname = "System.Collections.Generic.Stack_T",
    __cs2lua_typename = "Stack",
    __exist = function(k)
        return false
    end
}
System.Collections.Generic.Dictionary_TKey_TValue = {
    __cs2lua_defined = true,
    __cs2lua_fullname = "System.Collections.Generic.Dictionary_TKey_TValue",
    __cs2lua_typename = "Dictionary",
    __exist = function(k)
        return false
    end
}
System.Collections.Generic.HashSet_T = {
    __cs2lua_defined = true,
    __cs2lua_fullname = "System.Collections.Generic.HashSet_T",
    __cs2lua_typename = "HashSet",
    __exist = function(k)
        return false
    end
}
System.Collections.Generic.KeyValuePair_TKey_TValue = {
    __cs2lua_defined = true,
    __cs2lua_fullname = "System.Collections.Generic.KeyValuePair_TKey_TValue",
    __cs2lua_typename = "KeyValuePair",
    __exist = function(k)
        return false
    end
}
System.Array = System.Array or {}
System.Nullable_T = System.Nullable_T or {}

System.Collections.Generic.MyDictionary_TKey_TValue = System.Collections.Generic.Dictionary_TKey_TValue or {}

System.Linq = System.Linq or {}
System.Linq.Enumerable = System.Linq.Enumerable or {}

Cs2LuaList_T = Cs2LuaList_T or {}
Cs2LuaIntDictionary_TValue = Cs2LuaIntDictionary_TValue or {}
Cs2LuaStringDictionary_TValue = Cs2LuaStringDictionary_TValue or {}

__cs2lua_special_integer_operators = {"/", "%", "+", "-", "*", "<<", ">>", "&", "|", "^", "~"}
__cs2lua_div = 0
__cs2lua_mod = 1
__cs2lua_add = 2
__cs2lua_sub = 3
__cs2lua_mul = 4
__cs2lua_lshift = 5
__cs2lua_rshift = 6
__cs2lua_bitand = 7
__cs2lua_bitor = 8
__cs2lua_bitxor = 9
__cs2lua_bitnot = 10

SymbolKind = {
    Alias = 0,
    ArrayType = 1,
    Assembly = 2,
    DynamicType = 3,
    ErrorType = 4,
    Event = 5,
    Field = 6,
    Label = 7,
    Local = 8,
    Method = 9,
    NetModule = 10,
    NamedType = 11,
    Namespace = 12,
    Parameter = 13,
    PointerType = 14,
    Property = 15,
    RangeVariable = 16,
    TypeParameter = 17,
    Preprocessing = 18,
    Discard = 19
}

TypeKind = {
    Unknown = 0,
    Array = 1,
    Class = 2,
    Delegate = 3,
    Dynamic = 4,
    Enum = 5,
    Error = 6,
    Interface = 7,
    Module = 8,
    Pointer = 9,
    Struct = 10,
    Structure = 10,
    TypeParameter = 11,
    Submission = 12
}

MethodKind = {
    AnonymousFunction = 0,
    LambdaMethod = 0,
    Constructor = 1,
    Conversion = 2,
    DelegateInvoke = 3,
    Destructor = 4,
    EventAdd = 5,
    EventRaise = 6,
    EventRemove = 7,
    ExplicitInterfaceImplementation = 8,
    UserDefinedOperator = 9,
    Ordinary = 10,
    PropertyGet = 11,
    PropertySet = 12,
    ReducedExtension = 13,
    StaticConstructor = 14,
    SharedConstructor = 14,
    BuiltinOperator = 15,
    DeclareMethod = 16,
    LocalFunction = 17
}

g_LuaStr2CsStrCaches = {}
g_CsStr2LuaStrCaches = {}

function csstrtoluastr(str)
    if str==nil then
        return nil        
    elseif type(str) == "string" then
        return str
    elseif type(str) == "userdata" then
        local v = g_CsStr2LuaStrCaches[str]
        if v==nil then
            local s = tostring(str)
            g_CsStr2LuaStrCaches[str] = s
            return s
        else
            return v
        end
    else
        return str
    end
end

function luastrtocsstr(str)
    if str==nil then
        return nil
    elseif type(str) == "string" then
        local v = g_LuaStr2CsStrCaches[str]
        if v==nil then
            local s = System.String("ctor__Void__String_Arr_Char", str)
            g_LuaStr2CsStrCaches[str] = s
            return s
        else
            return v
        end
    elseif type(str) == "userdata" then
        return str
    else
        return str
    end
end

function printStack()
    Utility.Warn("{0}", debug.traceback())
end

function printJitStatus()
    local infos = Slua.CreateClass("System.Text.StringBuilder")
    local results = {jit.status()}
    infos:AppendFormat("jit status count {0}", #results)
    infos:AppendLine("AppendLine")
    for i, v in ipairs(results) do
        if i == 1 then
            infos:AppendFormat("jit status {0}", v)
        else
            infos:AppendFormat(" {0}", v)
        end
        infos:AppendLine("AppendLine")
    end
    UnityEngine.Debug.Log("Log_String", infos:ToString())
end

jit.off()
jit.flush()
printJitStatus()

if not package.loading then
    package.loading = {}
end

local rawrequire = require
-- a chatty version of the actual import function above
function require(x)
    if package.loading[x] == nil then
        package.loading[x] = true
        --print('loading started for ' .. x)
        rawrequire(x)
        --print('loading ended for ' .. x)
        package.loading[x] = nil
    else
        --print('already loading ' .. x)
    end
end

local function wrap_max(...)
    return math.max(...)
end

local function wrap_min(...)
    return math.min(...)
end

LuaConsole = {
    Write = function(...)
        io.write(...)
    end,
    Print = function(...)
        print(...)
    end
}

Cs2LuaLibrary = {
    IsCs2Lua = function(obj)
        if obj then
            local meta = getmetatable(obj)
            if meta and rawget(meta, "__cs2lua_defined") then
                return true
            end
        end
        return false
    end,
    FormatString = function(fmt, ...)
        return Utility.LuaFormat(fmt, ...);
    end,
    ToString = function(T, val)
        return tostring(val)
    end,
    GetType = function(T, val)
        if type(val) == "number" then
            return System.Int32
        elseif type(val) == "string" then
            return System.String
        else
            return val:GetType()
        end
    end,
    Max = wrap_max,
    Min = wrap_min,
    Max__System_Single__System_Single = wrap_max,
    Max__System_Int32__System_Int32 = wrap_max,
    Max__System_UInt32__System_UInt32 = wrap_max,
    Min__System_Single__System_Single = wrap_min,
    Min__System_Int32__System_Int32 = wrap_min,
    Min__System_UInt32__System_UInt32 = wrap_min,
}

function settempmetatable(class)
    setmetatable(
        class,
        {
            __index = function(tb, key)
                setmetatable(class, nil)
                class.__define_class()
                return tb[key]
            end,
            __newindex = function(tb, key, val)
                setmetatable(class, nil)
                class.__define_class()
                tb[key] = val
            end,
            __call = function(...)
                setmetatable(class, nil)
                class.__define_class()
                return class(...)
            end
        }
    )
    rawset(class, "__cs2lua_predefined", true)
end

function lualog(fmt, ...)
    Utility.Warn(fmt, ...);
end

function tanslationlog(fmt, ...)
    Utility.Warn(fmt, ...);
end

function luausing(func, ...)
    if nil==func then
        return true, 0
    else
        return pcall(func, ...)
    end
end

function luatry_geterror(e)
    local err = tostring(e)
    local trace = debug.traceback(err)
    UnityEngine.Debug.LogError("LogError_Object", err .. ", " .. trace)
    return {err, trace}
end

function luatry(func, ...)
    if nil==func then
        return true, 0
    else
        return xpcall(
            func,
            luatry_geterror,
            ...
        )
    end
end

function luacatch(handled, err, func)
    local retval = nil
    if not handled and func then
        retval = func({Message = err[1], StackTrace = err[2], ToString = function() return Message end})
    end
    return retval
end

function luathrow(obj)
    if type(obj) == "string" then
        error(obj)
    else
        error(obj.Message)
    end
end

function luaunpack(arr)
    local meta = getmetatable(arr)
    if meta and rawget(meta, "__cs2lua_defined") then
        return unpack(arr)
    else
        -- local tb = {}
        -- for i=1,#arr do
        --   tb[i] = arr[i]
        -- end
        -- return unpack(tb);
        return arr
    end
end

function issignature(sig, method)
    if type(sig) == "userdata" then
        sig = csstrtoluastr(sig)
    end
    if type(sig) == "string" then
        local l = string.find(sig, ":", 1, true)        
        local s,e = string.find(sig, method, 1 + 1, true)
        if s ~= nil and s == l + 1 then
            return true
        end
    end
    return false
end

function callexternextension(callerClass, method, ...)
    local obj = ...
    if calllerClass == System.Linq.Enumerable then
        error("can't call linq extension method !")
    else
        return obj[method](...);
    end
end

function getexternstaticindexer(callerClass, typeargs, typekinds, class, name, argCount, ...)
    return class[name](...)
end
function getexterninstanceindexer(callerClass, typeargs, typekinds, obj, class, name, argCount, ...)
    local arg1,arg2 = ...
    local index
    local meta = getmetatable(obj)
    if meta then
        local class = rawget(meta, "__class")
        if class == System.Collections.Generic.List_T then
            index = __unwrap_if_string(arg1)
            return obj[index + 1]
        elseif class == System.Collections.Generic.Dictionary_TKey_TValue then
            index = __unwrap_if_string(arg1)
            return obj[index]
        else
            if issignature(arg1, name) then
                index = __unwrap_if_string(arg2)
            else
                index = __unwrap_if_string(arg1)
            end
            if nil == index then
                UnityEngine.Debug.LogError("LogError_String", "[cs2lua] table index is nil")
                return nil
            end
            local typename = rawget(meta, "__typename")
            if typename == "LuaArray" then
                return obj[index + 1]
            elseif typename == "LuaVarObject" then
                return obj[index]
            elseif name == "get_Chars" then
                return Utility.StringGetChar(obj, index)
            else
                return obj:getItem(index)
            end
        end
    end
end

function setexternstaticindexer(callerClass, typeargs, typekinds, class, name, argCount, toplevel, ...)
    return class[name](...)
end
function setexterninstanceindexer(callerClass, typeargs, typekinds, obj, class, name, argCount, toplevel, ...)
    local arg1,arg2,arg3 = ...
    local index,val
    if issignature(arg1, name) then
        index = __unwrap_if_string(arg2)
        val = arg3
    else
        index = __unwrap_if_string(arg1)
        val = arg2
    end
    if nil == index then
        UnityEngine.Debug.LogError("LogError_String", "[cs2lua] table index is nil")
        return
    end
    local meta = getmetatable(obj)
    if meta then
        local class = rawget(meta, "__class")
        local typename = rawget(meta, "__typename")
        if class == System.Collections.Generic.List_T then
            obj[index + 1] = val
        elseif class == System.Collections.Generic.Dictionary_TKey_TValue then
            obj[index] = val
        elseif typename == "LuaArray" then
            obj[index + 1] = val
        elseif typename == "LuaVarObject" then
            obj[index] = val
        else
            obj:setItem(index, val)
        end
    end
    return nil
end

function invokeexternoperatorreturnstruct(funcInfo, rettype, class, method, ...)
    local arg1,arg2,arg3 = ...
    local marg1 = arg1 and getmetatable(arg1)
    local marg2 = arg2 and getmetatable(arg2)
    local marg3 = arg3 and getmetatable(arg3)
    --对slua，对应到lua元表操作符函数的操作符重载cs2lua转lua代码时已经换成对应操作符表达式。
    --执行到这里的应该是无法对应到lua操作符的操作符重载
    if rettype==System.Boolean and class==System.Type then
        if method=="op_Equality" then
            return arg1==arg2
        elseif method=="op_Inequality" then
            return arg1~=arg2
        end
    end
    if arg1==nil and method == "op_Equality" then
        return Slua.IsNull(arg2)
    elseif arg1==nil and method == "op_Inequality" then
        return not Slua.IsNull(arg2)
    elseif arg1~=nil and arg2==nil and method == "op_Equality" then
        if issignature(arg1, method) then
            return Slua.IsNull(arg3)
        else
            return Slua.IsNull(arg1)
        end
    elseif arg1~=nil and arg2==nil and method == "op_Inequality" then
        if issignature(arg1, method) then
            return not Slua.IsNull(arg3)
        else
            return not Slua.IsNull(arg1)
        end
    elseif arg1~=nil and arg2~=nil and method == "op_Equality" then
        return class[method](...)
    elseif arg1~=nil and arg2~=nil and method == "op_Inequality" then
        return class[method](...)
    elseif method == "op_Implicit" then
        local t = nil
        if marg2 then
            t = rawget(marg2, "__typename")
        elseif marg1 then
            t = rawget(marg1, "__typename")
        end
        if class == UnityEngine.Vector4 then
            if t == "Vector3" then
                local v4 = UnityEngine.Vector4.New(arg2.x,arg2.y,arg2.z)
                table.insert(funcInfo.v4_list, v4)
                return v4
            elseif t == "Vector4" then
                if rettype == UnityEngine.Vector3 then
                    local v3 = UnityEngine.Vector3.New(arg2.x,arg2.y,arg2.z)
                    table.insert(funcInfo.v3_list, v3)
                    return v3
                else
                    local v2 = UnityEngine.Vector2.New(arg2.x,arg2.y)
                    table.insert(funcInfo.v2_list, v2)
                    return v2
                end
            end
        elseif class == UnityEngine.Vector2 then
            if t == "Vector3" then
                local v2 = UnityEngine.Vector2.New(arg2.x,arg2.y)
                table.insert(funcInfo.v2_list, v2)
                return v2
            else
                local v3 = UnityEngine.Vector3.New(arg2.x,arg2.y,0)
                table.insert(funcInfo.v3_list, v3)
                return v3
            end
        elseif class == UnityEngine.Color32 then
            if t == "Color32" then
                local c = UnityEngine.Color.New(arg2.r/255.0,arg2.g/255.0,arg2.b/255.0,arg2.a/255.0)
                table.insert(funcInfo.c_list, c)
                return c
            else
                local c32 = Color32Pool.Alloc()
                table.insert(funcInfo.c32_list, c32)
                c32.x = arg2.r * 255
                c32.y = arg2.g * 255
                c32.z = arg2.b * 255
                c32.w = arg2.a * 255
                return c32
            end
        elseif class == BoxedValue then
            return class[method](arg1,arg2)
        else
            --这里就不仔细判断了，就假定是UnityEngine.Object子类了
            return not Slua.IsNull(arg1)
        end
    elseif method == "op_Multiply" then
        if arg1~=nil and arg2~=nil and arg3~=nil then
            local t1 = nil
            local t2 = nil
            if marg2 then
                t1 = rawget(marg2, "__typename")
            end
            if marg3 then
                t2 = rawget(marg3, "__typename")
            end
            if t1=="Vector2" and type(arg3)=="number" then
                local v2 = UnityEngine.Vector2.New(arg2.x*arg3,arg2.y*arg3)
                table.insert(funcInfo.v2_list, v2)
                return v2
            elseif type(arg2)=="number" and t2=="Vector2" then
                local v2 = UnityEngine.Vector2.New(arg3.x*arg2,arg3.y*arg2)
                table.insert(funcInfo.v2_list, v2)
                return v2
            elseif t1=="Vector2" and type(arg3)=="Vector2" then
                local v2 = UnityEngine.Vector2.New(arg2.x*arg3.x,arg2.y*arg3.y)
                table.insert(funcInfo.v2_list, v2)
                return v2
            elseif t1=="Vector3" and type(arg3)=="number" then
                local v3 = UnityEngine.Vector3.New(arg2.x*arg3,arg2.y*arg3,arg2.z*arg3)
                table.insert(funcInfo.v3_list, v3)
                return v3
            elseif type(arg2)=="number" and t2=="Vector3" then
                local v3 = UnityEngine.Vector3.New(arg3.x*arg2,arg3.y*arg2,arg3.z*arg2)
                table.insert(funcInfo.v3_list, v3)
                return v3
            elseif t1=="Vector4" and type(arg3)=="number" then
                local v4 = UnityEngine.Vector4.New(arg2.x*arg3,arg2.y*arg3,arg2.z*arg3,arg2.w*arg3)
                table.insert(funcInfo.v4_list, v4)
                return v4
            elseif type(arg2)=="number" and t2=="Vector4" then
                local v4 = UnityEngine.Vector4.New(arg3.x*arg2,arg3.y*arg2,arg3.z*arg2,arg3.w*arg2)
                table.insert(funcInfo.v4_list, v4)
                return v4
            elseif t1=="Color" and type(arg3)=="number" then
                local c = UnityEngine.Color.New(arg2.r*arg3,arg2.g*arg3,arg2.b*arg3,arg2.a*arg3)
                table.insert(funcInfo.c_list, c)
                return c
            elseif type(arg2)=="number" and t2=="Color" then
                local c = UnityEngine.Color.New(arg3.r*arg2,arg3.g*arg2,arg3.b*arg2,arg3.a*arg2)
                table.insert(funcInfo.c_list, c)
                return c
            else
                return class[method](...)
            end
        elseif arg1~=nil and arg2~=nil then
            if type(arg1)=="number" and type(arg2)=="number" then
                return arg1 * arg2
            else
                return class[method](...)
            end
        end
    elseif method == "op_Division" then
        if arg1~=nil and arg2~=nil and arg3~=nil then
            local t1 = nil
            local t2 = nil
            if marg2 then
                t1 = rawget(marg2, "__typename")
            end
            if marg3 then
                t2 = rawget(marg3, "__typename")
            end
            if t1=="Vector2" and type(arg3)=="number" then
                local v2 = Vector2Pool.Alloc()
                table.insert(funcInfo.v2_list, v2)
                v2.x = arg2.x/arg3
                v2.y = arg2.y/arg3
                return v2
            elseif t1=="Vector3" and type(arg3)=="number" then
                local v3 = Vector3Pool.Alloc()
                table.insert(funcInfo.v3_list, v3)
                v3.x = arg2.x/arg3
                v3.y = arg2.y/arg3
                v3.z = arg2.z/arg3
                return v3
            elseif t1=="Vector4" and type(arg3)=="number" then
                local v4 = Vector4Pool.Alloc()
                table.insert(funcInfo.v4_list, v4)
                v4.x = arg2.x/arg3
                v4.y = arg2.y/arg3
                v4.z = arg2.z/arg3
                v4.w = arg2.w/arg3
                return v4
            elseif t1=="Color" and type(arg3)=="number" then
                local c = ColorPool.Alloc()
                table.insert(funcInfo.c_list, c)
                c.x = arg2.r/arg3
                c.y = arg2.g/arg3
                c.z = arg2.b/arg3
                c.w = arg2.a/arg3
                return c
            else
                return class[method](...)
            end
        elseif arg1~=nil and arg2~=nil then
            if type(arg1)=="number" and type(arg2)=="number" then
                return arg1 / arg2
            else
                return class[method](...)
            end
        end
    end
    if method then        
        return class[method](...)
    else
        UnityEngine.Debug.LogError("LogError_String", "[cs2lua] table index is nil")
    end
    return nil
end

function invokeoperator(rettype, class, method, ...)
    return class[method](...)
end

function invokeexternoperator(rettype, class, method, ...)
    local arg1,arg2,arg3 = ...
    if rettype==System.Boolean and class==System.Type then
        if method=="op_Equality" then
            return arg1==arg2
        elseif method=="op_Inequality" then
            return arg1~=arg2
        end
    end
    if arg1==nil and method == "op_Equality" then
        return Slua.IsNull(arg2)
    elseif arg1==nil and method == "op_Inequality" then
        return not Slua.IsNull(arg2)
    elseif arg1~=nil and arg2==nil and method == "op_Equality" then
        if issignature(arg1, method) then
            return Slua.IsNull(arg3)
        else
            return Slua.IsNull(arg1)
        end
    elseif arg1~=nil and arg2==nil and method == "op_Inequality" then
        if issignature(arg1, method) then
            return not Slua.IsNull(arg3)
        else
            return not Slua.IsNull(arg1)
        end
    elseif method == "op_Implicit" then
        if class == BoxedValue then
            return class[method](arg1,arg2)
        else
            --这里就不仔细判断了，就假定是UnityEngine.Object子类了
            return not Slua.IsNull(arg1)
        end
    elseif method == "op_Multiply" then
        if arg1~=nil and arg2~=nil and arg3~=nil then
            return class[method](...)
        elseif arg1~=nil and arg2~=nil then
            if type(arg1)=="number" and type(arg2)=="number" then
                return arg1 * arg2
            else
                return class[method](...)
            end
        end
    elseif method == "op_Division" then
        if arg1~=nil and arg2~=nil and arg3~=nil then
            return class[method](...)
        elseif arg1~=nil and arg2~=nil then
            if type(arg1)=="number" and type(arg2)=="number" then
                return arg1 / arg2
            else
                return class[method](...)
            end
        end
    end
    if method then        
        return class[method](...)
    else
        UnityEngine.Debug.LogError("LogError_String", "[cs2lua] table index is nil")
    end
    return nil
end

local function _get_first_untable_from_pack_args(...)
    local arg1, arg2 = ...
    if arg1 or arg2 then
        if type(arg1) == "table" then
            return arg1[1], arg1[2]
        else
            return arg1, arg2
        end
    end
end

function invokeforbasicvalue(obj, isEnum, class, method, ...)
    local arg1,arg2 = ...
    local meta = getmetatable(obj)
    if isEnum and obj and method == "ToString" then
        return class.Value2String[obj]
    end
    if method then
        if class == System.Char and method == "ToString" then
            return Utility.CharToString(obj)
        elseif class == System.String then
            local csstr = obj
            if type(obj) == "string" then
                csstr = System.String("String__Arr_Char", obj)
            end
            if method == "Split" then
                local result1, result2 = _get_first_untable_from_pack_args(...)
                if type(result1) == "string" and type(result2) == "number" then
                    result2 = Utility.CharToString(result2)
                end
                return csstr[method](csstr, result1, result2)
            elseif method == "TrimStart" then
                local result = _get_first_untable_from_pack_args(...)
                if type(result) == "number" then
                    result = Utility.CharToString(result)
                end
                return csstr[method](csstr, result)
            else
                return csstr[method](csstr, ...)
            end
        elseif meta then
            return obj[method](obj, ...)
        elseif method == "CompareTo" then
            if issignature(arg1, method) then
                if type(obj)=="boolean" and type(arg2)=="boolean" then
                    if obj and arg2 then
                        return 0
                    elseif not obj and not arg2 then
                        return 0
                    elseif obj and not arg2 then
                        return 1
                    else
                        return -1
                    end
                else
                    if obj > arg2 then
                        return 1
                    elseif obj < arg2 then
                        return -1
                    else
                        return 0
                    end
                end
            else
                if type(obj)=="boolean" and type(arg2)=="boolean" then
                    if obj and arg2 then
                        return 0
                    elseif not obj and not arg2 then
                        return 0
                    elseif obj and not arg2 then
                        return 1
                    else
                        return -1
                    end
                else
                    if obj > arg1 then
                        return 1
                    elseif obj < arg1 then
                        return -1
                    else
                        return 0
                    end
                end
            end
        elseif method == "ToString" then
            return tostring(obj)
        elseif method == "Split" then
            local result1, result2 = _get_first_untable_from_pack_args(...)
            if type(result1) == "string" and type(result2) == "number" then
                result2 = Utility.CharToString(result2)
            end
            return obj[method](obj, result1, result2)
        elseif method == "TrimStart" then
            local result = _get_first_untable_from_pack_args(...)
            if type(result) == "number" then
                result = Utility.CharToString(result)
            end
            return obj[method](obj, result)
        end
    else
        UnityEngine.Debug.LogError("LogError_String", "[cs2lua] table index is nil")
    end
    return nil
end
function getforbasicvalue(obj, isEnum, class, property)
    local meta = getmetatable(obj)
    if property then
        if type(obj) == "string" then
            local csstr = System.String("String_Arr_Char", obj)
            return csstr[property]
        elseif meta then
            return obj[property]
        else
            if type(obj) == "number" then
                if property == "Length" then
                    return string.len(tostring(obj))
                end
            end
            return obj[property]
        end
    else
        UnityEngine.Debug.LogError("LogError_String", "[cs2lua] table index is nil")
    end
    return nil
end
function setforbasicvalue(obj, isEnum, class, property, value)
    local meta = getmetatable(obj)
    if property then
        if type(obj) == "string" then
            local csstr = System.String("String_Arr_Char", obj)
            csstr[property] = value
        elseif meta then
            obj[property] = value
        else
            obj[property] = value
        end
    else
        UnityEngine.Debug.LogError("LogError_String", "[cs2lua] table index is nil")
    end
    return nil
end

function invokearraystaticmethod(firstArray, secondArray, method, ...)
    if nil ~= firstArray and nil ~= method then
        local arg1,arg2,arg3 = ...
        local meta = getmetatable(firstArray)
        if meta and rawget(meta, "__cs2lua_defined") then
            if method == "IndexOf" then
                return firstArray:IndexOf(arg1, arg3)
            elseif method == "Sort" then
                return table.sort(
                    firstArray,
                    function(a, b)
                        return arg3(a, b) < 0
                    end
                )
            else
                return nil
            end
        else
            --这种情形认为是外部导出的数组调用，直接调导出接口了（由于System.Array有generic成员，这些方法的调用估计会出错）
            return System.Array[method](...)
        end
    else
        return nil
    end
end

--暂时只对整数除法进行特殊处理，运算溢出暂不处理
--__cs2lua_div = 0;
--__cs2lua_mod = 1;
--__cs2lua_add = 2;
--__cs2lua_sub = 3;
--__cs2lua_mul = 4;
--__cs2lua_lshift = 5;
--__cs2lua_rshift = 6;
--__cs2lua_bitand = 7;
--__cs2lua_bitor = 8;
--__cs2lua_bitxor = 9;
--__cs2lua_bitnot = 10;
function invokeintegeroperator(op, luaop, opd1, opd2, type1, type2)
    if op == __cs2lua_div then
        local r
        if opd1 * opd2 > 0 then
            r = math.floor(opd1 / opd2)
        else
            r = -math.floor(-opd1 / opd2)
        end
        return r
    elseif op == __cs2lua_mod then
        return opd1 % opd2
    elseif op == __cs2lua_add then
        if type1 then
            return opd1 + opd2
        else
            return opd2
        end
    elseif op == __cs2lua_sub then
        if type1 then
            return opd1 - opd2
        else
            return -opd2
        end
    elseif op == __cs2lua_mul then
        return opd1 * opd2
    elseif op == __cs2lua_lshift then
        return lshift(opd1, opd2)
    elseif op == __cs2lua_rshift then
        return rshift(opd1, opd2)
    elseif op == __cs2lua_bitand then
        return bitand(opd1, opd2)
    elseif op == __cs2lua_bitor then
        return bitor(opd1, opd2)
    elseif op == __cs2lua_bitxor then
        return bitxor(opd1, opd2)
    elseif op == __cs2lua_bitnot then
        return bitnot(opd1 or opd2)
    end
end

function createpool(newFunc)
    local pool = {}
    pool.Alloc = function()
            if #(pool.m_Data)>0 then
                return table.remove(pool.m_Data)
            else
                return newFunc()
            end
        end
    pool.Recycle = function(data)
            table.insert(pool.m_Data, data)
        end
    pool.m_Data = {}
    return pool
end

FuncInfoPool = createpool(
    function()
        return {
            v2_list = {},
            v3_list = {},
            v4_list = {},
            q_list = {},
            c_list = {},
            c32_list = {},
            rt_list = {},
        }
    end)
Vector2Pool = createpool(
    function()
        return UnityEngine.Vector2.NewFunc()
    end)
Vector3Pool = createpool(
    function()
        return UnityEngine.Vector3.NewFunc()
    end)
Vector4Pool = createpool(
    function()
        return UnityEngine.Vector4.NewFunc()
    end)
QuaternionPool = createpool(
    function()
        return UnityEngine.Quaternion.NewFunc()
    end)
ColorPool = createpool(
    function()
        return UnityEngine.Color.NewFunc()
    end)
Color32Pool = createpool(
    function()
        return Slua.CreateClass("UnityEngine.Color32", 0, 0, 0, 0)
    end)
RectPool = createpool(
    function()
        return Slua.CreateClass("UnityEngine.Rect", 0, 0, 0, 0)
    end)
    
function wrapenumerable(func)
    return function(...)
        local args = {...}
        return UnityEngine.WrapEnumerator(
            coroutine.create(
                function()
                    func(unpack(args))
                end
            )
        )
    end
end

function wrapyield(yieldVal, isEnumerableOrEnumerator, isUnityYield)
    UnityEngine.Yield(yieldVal)
end

function wrapconst(t, name)
    if name then
        return t[name]
    else
        UnityEngine.Debug.LogError("LogError_String", "[cs2lua] table index is nil")
    end
    return nil
end

function wrapchar(char, intVal)
    if intVal > 0 then
        return intVal
    else
        local str = tostring(char)
        local l = string.len(str)
        if l == 1 then
            local first = string.byte(str, 1, 1)
            return first
        elseif l == 2 then
            local first = string.byte(str, 1, 1)
            local second = string.byte(str, 2, 2)
            return first + second * 0x100
        else
            return 0
        end
    end
end

function wrapoutstruct(funcInfo, v, classObj)
    return classObj()
end

function wrapoutexternstruct(funcInfo, v, classObj)
    if classObj == System.Collections.Generic.KeyValuePair_TKey_TValue then
        return nil
    elseif classObj == UnityEngine.Vector2 then
        local obj = UnityEngine.Vector2.New(0,0)
        table.insert(funcInfo.v2_list, obj)
        return obj
    elseif classObj == UnityEngine.Vector3 then
        local obj = UnityEngine.Vector3.New(0,0,0)
        table.insert(funcInfo.v3_list, obj)
        return obj
    elseif classObj == UnityEngine.Vector4 then
        local obj = UnityEngine.Vector4.New(0,0,0,1)
        table.insert(funcInfo.v4_list, obj)
        return obj
    elseif classObj == UnityEngine.Quaternion then
        local obj = UnityEngine.Quaternion.New(0,0,0,1)
        table.insert(funcInfo.q_list, obj)
        return obj
    elseif classObj == UnityEngine.Color then
        local obj = UnityEngine.Color.New(0,0,0,1)
        table.insert(funcInfo.c_list, obj)
        return obj
    elseif classObj == UnityEngine.Color32 then
        local obj = Color32Pool.Alloc()
        table.insert(funcInfo.c32_list, obj)
        return obj
    else
        tanslationlog("need add handler for wrapoutexternstruct {0}", getclasstypename(classObj))
    end
    return classObj()
end

function wrapstruct(funcInfo, v, classObj)
    return v
end

function wrapexternstruct(funcInfo, v, classObj)
    if v then
        if classObj == UnityEngine.Vector2 then
            local obj = UnityEngine.Vector2.New(v.x,v.y)
            table.insert(funcInfo.v2_list, obj)
            return obj
        elseif classObj == UnityEngine.Vector3 then
            local obj = UnityEngine.Vector3.New(v.x,v.y,v.z)
            table.insert(funcInfo.v3_list, obj)
            return obj
        elseif classObj == UnityEngine.Vector4 then
            local obj = UnityEngine.Vector4.New(v.x,v.y,v.z,v.w)
            table.insert(funcInfo.v4_list, obj)
            return obj
        elseif classObj == UnityEngine.Quaternion then
            local obj = UnityEngine.Quaternion.New(v.x,v.y,v.z,v.w)
            table.insert(funcInfo.q_list, obj)
            return obj
        elseif classObj == UnityEngine.Color then
            local obj = UnityEngine.Color.New(v.r,v.g,v.b,v.a)
            table.insert(funcInfo.c_list, obj)
            return obj
        elseif classObj == UnityEngine.Color32 then
            local obj = Color32Pool.Alloc()
            obj.r = v.r or 0
            obj.g = v.g or 0
            obj.b = v.b or 0
            obj.a = v.a or 0
            table.insert(funcInfo.c_list, obj)
            return obj
        else
            tanslationlog("need add handler for wrapexternstruct {0}", getclasstypename(classObj))
        end
    end
    return v
end

function getexternstaticstructmember(funcInfo, symKind, class, member)
    if class==UnityEngine.Vector2 and member=="zero" then
        local obj = UnityEngine.Vector2.New(0,0)
        table.insert(funcInfo.v2_list, obj)
        return obj
    elseif class==UnityEngine.Vector2 and member=="one" then
        local obj = UnityEngine.Vector2.New(1,1)
        table.insert(funcInfo.v2_list, obj)
        return obj
    elseif class==UnityEngine.Vector3 and member=="zero" then
        local obj = UnityEngine.Vector3.New(0,0,0)
        table.insert(funcInfo.v3_list, obj)
        return obj
    elseif class==UnityEngine.Vector3 and member=="one" then
        local obj = UnityEngine.Vector3.New(1,1,1)
        table.insert(funcInfo.v3_list, obj)
        return obj
    elseif class==UnityEngine.Vector4 and member=="zero" then
        local obj = UnityEngine.Vector4.New(0,0,0,0)
        table.insert(funcInfo.v4_list, obj)
        return obj
    elseif class==UnityEngine.Vector4 and member=="one" then
        local obj = UnityEngine.Vector4.New(1,1,1,1)
        table.insert(funcInfo.v4_list, obj)
        return obj
    elseif class==UnityEngine.Quaternion and member=="identity" then
        local obj = UnityEngine.Quaternion.New(0,0,0,1)
        table.insert(funcInfo.q_list, obj)
        return obj
    else
        tanslationlog("need add handler for getexternstaticstructmember {0}.{1}", getclasstypename(class), member)
        return class[member]
    end
end

function getexterninstancestructmember(funcInfo, symKind, obj, class, member)
    tanslationlog("need add handler for getexterninstancestructmember {0}.{1}", getclasstypename(class), member)
    return obj[member]
end

function luatableremove(tb, val)
    for i,v in ipairs(tb) do
        if rawequal(v,val) then
            table.remove(tb, i)
        end
    end
end

function keepstructvalue(funcInfo, fieldType, val)
    if fieldType==UnityEngine.Vector2 then
        luatableremove(funcInfo.v2_list, val)
    elseif fieldType==UnityEngine.Vector3 then
        luatableremove(funcInfo.v3_list, val)
    elseif fieldType==UnityEngine.Vector4 then
        luatableremove(funcInfo.v4_list, val)
    elseif fieldType==UnityEngine.Quaternion then
        luatableremove(funcInfo.q_list, val)
    elseif fieldType==UnityEngine.Color then
        luatableremove(funcInfo.c_list, val)
    elseif fieldType==UnityEngine.Color32 then
        luatableremove(funcInfo.c32_list, val)
    elseif fieldType==UnityEngine.Rect then
        luatableremove(funcInfo.rt_list, val)
    end
end

function recyclestructvalue(funcInfo, fieldType, val)
    if val==nil then
        return
    end
    if fieldType==UnityEngine.Vector2 then
        Vector2Pool.Recycle(val)
    elseif fieldType==UnityEngine.Vector3 then
        Vector3Pool.Recycle(val)
    elseif fieldType==UnityEngine.Vector4 then
        Vector4Pool.Recycle(val)
    elseif fieldType==UnityEngine.Quaternion then
        QuaternionPool.Recycle(val)
    elseif fieldType==UnityEngine.Color then
        ColorPool.Recycle(val)
    elseif fieldType==UnityEngine.Color32 then
        Color32Pool.Recycle(val)
    elseif fieldType==UnityEngine.Rect then
        RectPool.Recycle(val)
    end
end

function keepstaticstructfield(funcInfo, fieldType, class, member)
    local val = class[member]
    keepstructvalue(funcInfo, fieldType, val)
end

function keepinstancestructfield(funcInfo, fieldType, obj, class, member)
    local val = obj[member]
    keepstructvalue(funcInfo, fieldType, val)
end

function recyclestaticstructfield(funcInfo, fieldType, class, member)
    local val = class[member]
    recyclestructvalue(funcInfo, fieldType, val)
end

function recycleinstancestructfield(funcInfo, fieldType, obj, class, member)
    local val = obj[member]
    recyclestructvalue(funcInfo, fieldType, val)
end

function callexterndelegationreturnstruct(funcInfo, funcobj, funcobjname, ...)
    tanslationlog("need add handler for callexterndelegationreturnstruct {0}", funcobjname)
    return funcobj(...)
end

function callexternextensionreturnstruct(funcInfo, class, member, ...)
    tanslationlog("need add handler for callexternextensionreturnstruct {0}.{1}", getclasstypename(class), member)
    return class[member](...)
end

function callexternstaticreturnstruct(funcInfo, class, member, ...)
    tanslationlog("need add handler for callexternstaticreturnstruct {0}.{1}", getclasstypename(class), member)
    return class[member](...)
end

function callexterninstancereturnstruct(funcInfo, obj, class, member, ...)
    tanslationlog("need add handler for callexterninstancereturnstruct {0}.{1}", getclasstypename(class), member)
    return obj[member](obj, ...)
end

function luainitialize()
    return FuncInfoPool.Alloc()
end

function luafinalize(funcInfo)
    if funcInfo then
        for i,v in ipairs(funcInfo.v2_list) do
            Vector2Pool.Recycle(v)
        end
        for i,v in ipairs(funcInfo.v3_list) do
            Vector3Pool.Recycle(v)
        end
        for i,v in ipairs(funcInfo.v4_list) do
            Vector4Pool.Recycle(v)
        end
        for i,v in ipairs(funcInfo.q_list) do
            QuaternionPool.Recycle(v)
        end
        for i,v in ipairs(funcInfo.c_list) do
            ColorPool.Recycle(v)
        end
        for i,v in ipairs(funcInfo.c32_list) do
            Color32Pool.Recycle(v)
        end
        for i,v in ipairs(funcInfo.rt_list) do
            RectPool.Recycle(v)
        end
        local ct
        ct = #(funcInfo.v2_list)
        for i=1,ct do
            funcInfo.v2_list[i]=nil
        end
        ct = #(funcInfo.v3_list)
        for i=1,ct do
            funcInfo.v3_list[i]=nil
        end
        ct = #(funcInfo.v4_list)
        for i=1,ct do
            funcInfo.v4_list[i]=nil
        end
        ct = #(funcInfo.q_list)
        for i=1,ct do
            funcInfo.q_list[i]=nil
        end
        ct = #(funcInfo.c_list)
        for i=1,ct do
            funcInfo.c_list[i]=nil
        end
        ct = #(funcInfo.c32_list)
        for i=1,ct do
            funcInfo.c32_list[i]=nil
        end
        ct = #(funcInfo.rt_list)
        for i=1,ct do
            funcInfo.rt_list[i]=nil
        end
        FuncInfoPool.Recycle(funcInfo)
    end
    return nil
end

Cs2LuaCustomData = {
	__new_object = function(...)
		local __cs2lua_newobj = newobject(nil, Cs2LuaCustomData, nil, nil, nil, nil, ...);
		return __cs2lua_newobj;
	end,
	__define_class = function()
		printMemDiff("Cs2LuaCustomData::__define_class begin");

		local class = Cs2LuaCustomData;

		local obj_methods = nil;
		local obj_build = function()
			return {
				CustomData = nil,
			};
		end;

		local __defineclass_return = defineclass(UnityEngine.Object, "Cs2LuaCustomData", "Cs2LuaCustomData", class, obj_methods, obj_build, false);
		printMemDiff("Cs2LuaCustomData::__define_class end");
		return __defineclass_return;
	end,
	__obj_fields = {
	    CustomData = true,
	},
};

settempmetatable(Cs2LuaCustomData);

function luatoobject(symKind, isStatic, symName, arg1, ...)
    if arg1 and symKind==SymbolKind.Field then
        local meta = getmetatable(arg1)
        if meta and rawget(meta, "__cs2lua_defined") then
            --lualog("luatoobject symKind:{0} {1} {2} {3}", symKind, isStatic, symName, meta.__cs2lua_fullname)
            --printStack()
            local o = Cs2LuaCustomData.__new_object()
            o.CustomData = arg1
            arg1 = o
        end
    end
    return arg1, ...
end

function objecttolua(arg1, ...)
    if arg1 then
        local meta = getmetatable(arg1)       
        if meta and rawget(meta, "__cs2lua_fullname")=="Cs2LuaCustomData" then   
            arg1 = arg1.CustomData
            --local metav = getmetatable(arg1)
            --lualog("objecttolua:{0} {1}", meta.__cs2lua_fullname, metav.__cs2lua_fullname)
            --printStack()
        end
    end
    return arg1, ...
end

__mt_delegation = {
    __is_delegation = true,
    __call = function(t, ...)
        local ret = nil
        for k, v in pairs(t) do
            if v then
                ret = v(...)
            end
        end
        return ret
    end
}

function wrapdelegation(handlers)
    return setmetatable(handlers, __mt_delegation)
end

function __get_obj_string(obj)
    if type(obj) == "table" then
        local oldTblMeta = getmetatable(obj)
        setmetatable(obj, nil)
        local s = tostring(obj)
        setmetatable(obj, oldTblMeta)
        return s
    else
        return tostring(obj)
    end
end

__cs2lua_delegations = setmetatable({},{ __mode = 'v' })

function calcdelegationkey(class_member_key, obj)
    local fk = class_member_key .. __get_obj_string(obj)
    return fk
end
function getdelegation(key)
    return rawget(__cs2lua_delegations, key)
end
function builddelegationonce(key, handler)
    local old = rawget(__cs2lua_delegations, key)
    if old~=handler then
        rawset(__cs2lua_delegations, key, handler)
    end
    return handler
end

function dumpdelegationtable()
    print("dumpdelegationtable")

    if next(__cs2lua_delegations) == nil then
        print("dumpdelegationtable empty")
        return
    end

    for k, v in pairs(__cs2lua_delegations) do
        print(k)
        print(v)
    end
end

function delegationwrap(handler)
    if handler then
        local meta = getmetatable(handler)
        if meta and rawget(meta, "__is_delegation") then
            return handler
        else
            return wrapdelegation {handler}
        end
    else
        return wrapdelegation {}
    end
end

function delegationcomparewithnil(isstatic, t, k, symKind, beequal)
    if not t then
        if beequal then
            return true
        else
            return false
        end
    end
    if type(t) == "function" then
        if beequal then
            return false
        else
            return true
        end
    end
    local v = t
    if k then
        if symKind == SymbolKind.Property then
            v = t[k](t)
        else
            v = t[k]
        end
    end
    if type(v) == "function" then
        if beequal then
            return false
        else
            return true
        end
    end
    local n = (v and #v) or 0
    if beequal and n == 0 then
        return true
    elseif not isqual and n > 0 then
        return true
    else
        return false
    end
end
function delegationset(isstatic, t, k, symKind, handler)
    local v = t
    if k then
        if symKind == SymbolKind.Property then
            v = t[k](t)
        else
            v = t[k]
        end
    end
    if not v or type(v) ~= "table" then
        --取不到值或者值不是表，则有可能是普通的特性访问
        --t[k] = handler;
        return handler
    else
        local n = #v
        for i = 1, n do
            table.remove(v)
        end
        table.insert(v, handler)
        return v
    end
end
function delegationadd(isstatic, t, k, symKind, handler)
    local v = t
    if k then
        if symKind == SymbolKind.Property then
            v = t[k](t)
        else
            v = t[k]
        end
    end
    if v == nil then
        v = delegationwrap(handler)
    else
        table.insert(v, handler)
    end
    return v
end
function delegationremove(isstatic, t, k, symKind, handler)
    local v = t
    if k then
        if symKind == SymbolKind.Property then
            v = t[k](t)
        else
            v = t[k]
        end
    end
    local find = false
    local pos = 1
    for k, h in pairs(v) do
        if h == handler then
            find = true
            break
        end
        pos = pos + 1
    end
    if find then
        table.remove(v, pos)
    end
    return v
end

function externdelegationcomparewithnil(isstatic, t, k, symKind, beequal)
    local v = t
    if k then
        return true
    end
    if beequal and not v then
        return true
    elseif not beequal and v then
        return true
    else
        return false
    end
end
function externdelegationset(isstatic, t, k, symKind, handler)
    if k then
        return handler
    else
        return handler
    end
end
function externdelegationadd(isstatic, t, k, symKind, handler)
    if k then
        return {"+=", handler}
    else
        return {"+=", handler}
    end
end
function externdelegationremove(isstatic, t, k, symKind, handler)
    local ret = nil
    if k then
        ret = {"-=", handler}
    else
        ret = {"-=", handler}
    end
    return ret
end

function __calc_table_count(tb)
    local count = 0
    for k, v in pairs(tb) do
        count = count + 1
    end
    return count
end

function __get_table_data(tb)
    local meta = getmetatable(tb)
    if meta and meta.__cs2lua_data then
        return meta.__cs2lua_data
    end
    error("can't find metatable or __cs2lua_data !")
end

function __set_table_count(tb, count)
    local meta = getmetatable(tb)
    if meta then
        meta.__count = count
    end
end

function __get_table_count(tb)
    local count = 0
    local meta = getmetatable(tb)
    if meta then
        if meta.__count then
            count = meta.__count
        else
            count = __calc_table_count(meta.__cs2lua_data)
            meta.__count = count
        end
    end
    return count
end

function __inc_table_count(tb)
    local meta = getmetatable(tb)
    if meta then
        if nil ~= meta.__count then
            meta.__count = meta.__count + 1
        else
            meta.__count = __calc_table_count(meta.__cs2lua_data)
        end
    end
end

function __dec_table_count(tb)
    local meta = getmetatable(tb)
    if meta then
        if meta.__count and meta.__count > 0 then
            meta.__count = meta.__count - 1
        else
            meta.__count = __calc_table_count(meta.__cs2lua_data)
        end
    end
end

function __clear_table(tb)
    local meta = getmetatable(tb)
    if meta then
        meta.__count = 0
        meta.__cs2lua_data = {}
    end
end

function __set_array_count(tb, count)
    local meta = getmetatable(tb)
    if meta then
        meta.__count = count
    end
end

function __get_array_count(tb)
    local count = 0
    local meta = getmetatable(tb)
    if meta then
        if meta.__count then
            count = meta.__count
        else
            count = #tb
            meta.__count = count
        end
    end
    return count
end

function __inc_array_count(tb)
    local meta = getmetatable(tb)
    if meta then
        if nil ~= meta.__count then
            meta.__count = meta.__count + 1
        else
            meta.__count = #tb
        end
    end
end

function __dec_array_count(tb)
    local meta = getmetatable(tb)
    if meta then
        if meta.__count and meta.__count > 0 then
            meta.__count = meta.__count - 1
        else
            meta.__count = #tb
        end
    end
end

__mt_index_of_array_table = {
    __exist = function(tb, fk) --禁用继承
            return false
        end,
    get_Length = function(obj) 
    		    return __get_table_count(obj) 
    		end,
    get_Count = function(obj) 
    		    return __get_table_count(obj) 
    		end,
    GetLength = function(obj, ix)
            local ret = 0
            local tb = obj
            for i = 0, ix do
                ret = __get_array_count(tb)
                tb = rawget(tb, 0)
            end
            return ret
        end,
    Add = function(obj, v)
            table.insert(obj, v)
            __inc_array_count(obj)
            -- assert(__get_array_count(obj) == #obj,"not match length count:"..__get_array_count(obj).." #len:"..#obj);
        end,
    Remove = function(obj, p)
            local pos = 0
            local ret = nil
            local ct = __get_array_count(obj)
            for i = 1, ct do
                local v = rawget(obj, i)
                if isequal(v, p) then
                    pos = i
                    ret = v
                    break
                end
            end
            if ret then
                table.remove(obj, pos)
                __dec_array_count(obj)
            end
            -- assert(__get_array_count(obj) == #obj,"not match length count:"..__get_array_count(obj).." #len:"..#obj)
            return ret
        end,
    RemoveAt = function(obj, ix)
            table.remove(obj, ix + 1)
            __dec_array_count(obj)
            -- assert(__get_array_count(obj) == #obj,"not match length count:"..__get_array_count(obj).." #len:"..#obj)
        end,
    RemoveAll = function(obj, pred)
            local deletes = {}
            local ct = __get_array_count(obj)
            for i = 1, ct do
                if pred(rawget(obj, i)) then
                    table.insert(deletes, i)
                end
            end
            for i, v in ipairs(deletes) do
                table.remove(obj, v)
                __dec_array_count(obj)
            end
            -- assert(__get_array_count(obj) == #obj,"not match length count:"..__get_array_count(obj).." #len:"..#obj)
        end,
    AddRange = function(obj, coll)
            local iter = newiterator(coll)
            for v in getiterator(iter) do
                table.insert(obj, v)
                __inc_array_count(obj)
            end
            -- assert(__get_array_count(obj) == #obj,"not match length count:"..__get_array_count(obj).." #len:"..#obj)
        end,
    Insert = function(obj, ix, p)
            table.insert(obj, ix + 1, p)
            __inc_array_count(obj)
            -- assert(__get_array_count(obj) == #obj,"not match length count:"..__get_array_count(obj).." #len:"..#obj)
        end,
    InsertRange = function(obj, ix, coll)
            local ct = 0
            local enumer = coll:GetEnumerator()
            while enumer:MoveNext() do
                table.insert(obj, ix + 1 + ct, enumer.Current)
                __inc_array_count(obj)
                ct = ct + 1
            end
            -- assert(__get_array_count(obj) == #obj,"not match length count:"..__get_array_count(obj).." #len:"..#obj)
        end,
    RemoveRange = function(obj, ix, ct)
            for i=1,ct do                
                table.remove(obj, ix + 1)
                __dec_array_count(obj)
            end
            -- assert(__get_array_count(obj) == #obj,"not match length count:"..__get_array_count(obj).." #len:"..#obj)
        end,
    IndexOf = function(obj, sig, p, start, count)
            local ct = __get_array_count(obj)
            if count==nil then
                count = ct
            end
            if start==nil then
                start = 0
            end
            for i = start+1, ct do
                local v = rawget(obj, i)
                if rawequal(v,p) then
                    return i - 1
                end
            end
            return -1
        end,
    LastIndexOf = function(obj, sig, p, start, count)
            local ct = __get_array_count(obj)
            if count==nil then
                count = ct
            end
            if start==nil then
                start = 0
            end
            for k = ct, start+1 do
                local v = rawget(obj, k)
                if rawequal(v,p) then
                    return k - 1
                end
            end
            return -1
        end,
    FindIndex = function(obj, sig, p1, p2, p3)
            local ct = __get_array_count(obj)
            local start = 0
            local count = ct
            local pred = p1
            if p1~=nil and p2~=nil and p3~=nil then
                start = p1
                count = p2
                pred = p3
            elseif p1~=nil and p2~=nil then
                start = p1
                pred = p2
            end
            for i = start+1, ct do
                local v = rawget(obj, i)
                if pred(v) then
                    return i - 1
                end
            end
            return -1
        end,
    Find = function(obj, predicate)
            local ct = __get_array_count(obj)
            for i = 1, ct do
                local v = rawget(obj, i)
                if predicate(v) then
                    return v
                end
            end
            return nil
        end,
    Contains = function(obj, p)
            local ret = false
            local ct = __get_array_count(obj)
            for i = 1, ct do
                local v = rawget(obj, i)
                if rawequal(v,p) then
                    ret = true
                    break
                end
            end
            return ret
        end,
    Peek = function(obj)
            local ct = __get_array_count(obj)
            local v = rawget(obj, ct)
            return v
        end,
    Enqueue = function(obj, v)
            table.insert(obj, 1, v)
            __inc_array_count(obj)
        end,
    Dequeue = function(obj)
            local ct = __get_array_count(obj)
            local v = rawget(obj, ct)
            table.remove(obj, ct)
            __dec_array_count(obj)
            return v
        end,
    Push = function(obj, v)
            table.insert(obj, v)
            __inc_array_count(obj)
        end,
    Pop = function(obj)
            local ct = __get_array_count(obj)
            local v = rawget(obj, ct)
            table.remove(obj, num)
            __dec_array_count(obj)
            return v
        end,
    CopyTo = function(obj, arr)
            local ct = __get_array_count(obj)
            for k = 1, ct do
                arr[k] = rawget(obj, k)
            end
        end,
    ToArray = function(obj)
            local ct = __get_array_count(obj)
            local ret = wraparray({}, ct)
            for k = 1, ct do
                ret[k] = rawget(obj, k)
            end
            return ret
        end,
    Clear = function(obj)
            local ct = __get_array_count(obj)
            for i = ct, 1, -1 do
                table.remove(obj, i)
            end
            __set_array_count(obj, 0)
            -- assert(__get_array_count(obj) == #obj,"not match length count:"..__get_array_count(obj).." #len:"..#obj)
        end,
    GetEnumerator = function(obj)
            return GetArrayEnumerator(obj)
        end,
    Sort = function(obj, sig, predicate)
            table.sort(
                obj,
                function(a, b)
                    return predicate(a, b) < 0
                end
            )
        end,
    Sort__System_Comparison_T = function(obj, predicate)
            table.sort(
                obj,
                function(a, b)
                    return predicate(a, b) < 0
                end
            )
        end,
    GetType = function(obj)
            local meta = getmetatable(obj)
            return meta.__class
        end,
}

__mt_index_of_array = function(t, k)
    if k == "Length" or k == "Count" then
        return __get_array_count(t)
    else
		    return __mt_index_of_array_table[k]
    end
end

__mt_index_of_dictionary_table = {
    __exist = function(tb, fk)    --禁用继承
            return false
        end,
    get_Count = function(obj)
            return __get_table_count(obj)
        end,
    Add = function(obj, p1, p2)
            up1 = __unwrap_if_string(p1)     
            local data = __get_table_data(obj)
            rawset(data, up1, {Key = p1,Value = p2})
            __inc_table_count(obj)
            return p2
        end,
    Remove = function(obj, p)
            up = __unwrap_if_string(p) 
            local data = __get_table_data(obj)     
            local v = rawget(data, up)
            local ret = nil
            if v then
                ret = v.Value
                rawset(data, up, nil)
            end
            __dec_table_count(obj)
            return ret
        end,
    ContainsKey = function(obj, p)
            up = __unwrap_if_string(p) 
            local data = __get_table_data(obj)     
            if rawget(data, up) then
                return true
            end
            return false
        end,
    ContainsValue = function(obj, p)
            local ret = false 
            local data = __get_table_data(obj)     
            for k, v in pairs(data) do
                if rawequal(v.Value,p) then
                    ret = true
                    break
                end
            end
            return ret
        end,
    TryGetValue = function(obj, p)
            up = __unwrap_if_string(p) 
            local data = __get_table_data(obj)     
            local v = rawget(data, up)
            if v then
                return true, v.Value
            else
                v = rawget(data, tostring(up))
                if v then
                    return true, v.Value
                end
            end
            local meta = getmetatable(obj)
            if meta.__cs2lua_typeargs and meta.__cs2lua_typekinds then
                if meta.__cs2lua_typekinds[2] == TypeKind.Struct then
                    local vt = meta.__cs2lua_typeargs[2]
                    if vt==System.Int32 or vt==System.UInt32 
                        or vt==System.Int64 or vt==System.UInt64 
                        or vt==System.Char or vt==System.Byte 
                        or vt==System.Int16 or vt==System.UInt16 then
                        return false, 0
                    end
                end
            end
            return false, nil
        end,
    Clear = function(obj)
            __clear_table(obj)
        end,
    GetEnumerator = function(obj)
            return GetDictEnumerator(obj)
        end,
    GetType = function(obj)
            local meta = getmetatable(obj)
            return meta.__class
        end,
}

__mt_index_of_dictionary = function(t, k)
    if k == "Count" then
        return __get_table_count(t)
    elseif k == "Keys" then
        local meta = getmetatable(t)
        if not meta.cachedKeyCollection then
            meta.cachedKeyCollection = NewKeyCollection(t)
        end
        return meta.cachedKeyCollection
    elseif k == "Values" then
        local meta = getmetatable(t)
        if not meta.cachedValueCollection then
            meta.cachedValueCollection = NewValueCollection(t)
        end
        return meta.cachedValueCollection
    else 
        local f = __mt_index_of_dictionary_table[k]
        if f then
            return f
        else
            uk = __unwrap_if_string(k) 
            local data = __get_table_data(t)     
            local v = rawget(data, uk)
            if v then
                return v.Value
            end
            return nil
        end
    end
end

__mt_newindex_of_dictionary = function(t, k, val) 
    uk = __unwrap_if_string(k) 
    local data = __get_table_data(t)     
    local v = rawget(data, uk)
    if not v then
        __inc_table_count(t)
        rawset(data, uk, {Key = k, Value = val})
    else
        v.Value = val;
    end
end

__mt_index_of_hashset_table = {
    __exist = function(tb, fk)  --禁用继承
            return false
        end,
    get_Count = function(obj)
             return __get_table_count(obj) 
        end,
    Add = function(obj, p)
            up = __unwrap_if_string(p) 
            local data = __get_table_data(obj)     
            rawset(data, up, true)
            __inc_table_count(obj)
            return true
        end,
    Remove = function(obj, p)
            up = __unwrap_if_string(p) 
            local data = __get_table_data(obj)     
            local ret = rawget(data, up)
            if ret then
                rawset(data, up, nil)
            end
            __dec_table_count(obj)
            return ret
        end,
    Contains = function(obj, p)
            up = __unwrap_if_string(p) 
            local data = __get_table_data(obj)     
            if rawget(data, up) then
                return true
            end
            return false
        end,
    CopyTo = function(obj, arr) 
            local data = __get_table_data(obj)     
            for k, v in pairs(data) do
                k = __wrap_if_string(k)
                table.insert(arr, k)
                __inc_array_count(arr)
            end
        end,
    Clear = function(obj)
            __clear_table(obj)
        end,
    GetEnumerator = function(obj)
            return GetHashsetEnumerator(obj)
        end,
    GetType = function(obj)
            local meta = getmetatable(obj)
            return meta.__class
        end,
}

__mt_index_of_hashset = function(t, k)
    if k == "Count" then
        return __get_table_count(t)
    else
        return __mt_index_of_hashset_table[k]
    end
end

__mt_index_of_keycollection_table = {
    CopyTo = function(obj, arr) 
        local t = obj.dict
        local data = __get_table_data(t)     
        for k, v in pairs(data) do
            wk = __wrap_if_string(k)
            table.insert(arr, wk)
            __inc_array_count(arr)
        end
    end,
    GetEnumerator = function(obj)
        return GetDictKeyEnumerator(obj)
    end,
}

__mt_index_of_keycollection = function(t, k)
    if k == "Count" then
        return t.dict.Count
    else
        return __mt_index_of_keycollection_table[k]
    end
end

function NewKeyCollection(dict)
    return setmetatable({
        dict = dict
    },
    {
        __index = __mt_index_of_keycollection,
        __cs2lua_defined = true,
    })
end

__mt_index_of_valuecollection_table = {
    CopyTo = function(obj, arr) 
        local t = obj.dict
        local data = __get_table_data(t)     
        for k, v in pairs(data) do
            table.insert(arr, v.Value)
            __inc_array_count(arr)
        end
    end,
    GetEnumerator = function(obj)
        return GetDictValueEnumerator(obj)
    end,
}

__mt_index_of_valuecollection = function(t, k)
    if k == "Count" then
        return t.dict.Count
    else
        return __mt_index_of_valuecollection_table[k]
    end
end

function NewValueCollection(dict)
    return setmetatable({
        dict = dict
    },
    {
        __index = __mt_index_of_valuecollection,
        __cs2lua_defined = true,
    })
end

function GetArrayEnumerator(tb)
    local function __get_Current(obj) return obj.current end
    return setmetatable(
        {
            Reset = function(this)
                this.index = 0
                this.current = nil
            end,
            MoveNext = function(this)
                local ltb = this.object
                local num = __get_array_count(ltb)
                if this.index < num then
                    this.index = this.index + 1
                    this.current = rawget(ltb, this.index)
                    return true
                else
                    return false
                end
            end,
            object = tb,
            index = 0,
            current = nil
        },
        {
            __index = function(t, k)
                if k == "Current" then
                    return t.current
                elseif k == "get_Current" then
                    return __get_Current
                end
                return nil
            end
        }
    )
end

function GetDictEnumerator(tb)
    local function __get_Current(obj) return obj.current end
    return setmetatable(
        {
            Reset = function(this)
                this.key = nil
                this.current = nil
            end,
            MoveNext = function(this)
                local ltb = this.object
                local v = nil 
                local data = __get_table_data(ltb)     
                this.key, v = next(data, this.key)
                this.current = v
                if this.key then
                    return true
                else
                    return false
                end
            end,
            object = tb,
            key = nil,
            current = nil
        },
        {
            __index = function(t, k)
                if k == "Current" then
                    return t.current
                elseif k == "get_Current" then
                    return __get_Current
                end
                return nil
            end
        }
    )
end

function GetDictKeyEnumerator(tb)
    local function __get_Current(obj) return obj.current end
    return setmetatable(
        {
            Reset = function(this)
                this.key = nil
                this.current = nil
            end,
            MoveNext = function(this)
                local ltb = this.object
                local v = nil 
                local data = __get_table_data(ltb)     
                this.key, v = next(data, this.key)
                this.current = __wrap_if_string(this.key)
                if this.key then
                    return true
                else
                    return false
                end
            end,
            object = tb.dict,
            key = nil,
            current = nil
        },
        {
            __index = function(t, k)
                if k == "Current" then
                    return t.current
                elseif k == "get_Current" then
                    return __get_Current
                end
                return nil
            end
        }
    )
end

function GetDictValueEnumerator(tb)
    local function __get_Current(obj) return obj.current end
    return setmetatable(
        {
            Reset = function(this)
                this.key = nil
                this.current = nil
            end,
            MoveNext = function(this)
                local ltb = this.object
                local v = nil 
                local data = __get_table_data(ltb)     
                this.key, v = next(data, this.key)
                this.current = v and v.Value
                if this.key then
                    return true
                else
                    return false
                end
            end,
            object = tb.dict,
            key = nil,
            current = nil
        },
        {
            __index = function(t, k)
                if k == "Current" then
                    return t.current
                elseif k == "get_Current" then
                    return __get_Current
                end
                return nil
            end
        }
    )
end

function GetHashsetEnumerator(tb)
    local function __get_Current(obj) return __wrap_if_string(obj.Key) end
    return setmetatable(
        {
            Reset = function(this)
                this.key = nil
            end,
            MoveNext = function(this)
                local ltb = this.object
                local v = nil 
                local data = __get_table_data(ltb)     
                this.key, v = next(data, this.key)
                if this.key then
                    return true
                else
                    return false
                end
            end,
            object = tb,
            key = nil
        },
        {
            __index = function(t, k)
                if k == "Current" then
                    return __wrap_if_string(t.key)
                elseif k == "get_Current" then
                    return __get_Current
                end
                return nil
            end
        }
    )
end

function newiterator(exp)
    local meta = getmetatable(exp)
    if meta and rawget(meta, "__cs2lua_defined") then
        if meta.cachedIters and meta.cachedIters[exp] and #(meta.cachedIters[exp])>0 then
            local iterInfo = table.remove(meta.cachedIters[exp], 1)
            iterInfo[2]:Reset()
            return iterInfo
        else
            local enumer = exp:GetEnumerator()
            local f = function()
                if enumer:MoveNext() then
                    return enumer.Current
                else
                    return nil
                end
            end
            return {f, enumer, exp}
        end
    elseif meta and rawget(meta, "__typename")=="LuaArray" then
        --lualog("LuaArray newiterator:{0} {1}", exp, exp.Length)
        --printStack()
        if meta.cachedIters and meta.cachedIters[exp] and #(meta.cachedIters[exp])>0 then
            local iterInfo = table.remove(meta.cachedIters[exp], 1)
            iterInfo[2]()
            return iterInfo
        else
            local arr = exp
            local curIx = 0
            local f = function()
                --lualog("LuaArray iterator arr:{0} length:{1}", arr, arr.Length)
                if curIx < arr.Length then
                    curIx = curIx + 1
                    local v = arr[curIx]
                    --lualog("LuaArray iterator:{0} {1}", curIx, v)
                    return v
                else
                    --lualog("LuaArray iterator:{0} nil", curIx)
                    return nil
                end
            end
            local reset = function()
                curIx = 0
            end
            return {f, reset, exp}
        end
    else
        return Slua.iter(exp)
    end
end

function getiterator(iterInfo)
    if type(iterInfo)=="table" then
        return iterInfo[1]
    else
        return iterInfo    
    end 
end

function recycleiterator(iterInfo)   
    if type(iterInfo)~="table" then
        return
    end 
    local exp = iterInfo[3]
    if exp then
        local meta = getmetatable(exp)
        if meta then
            if meta.cachedIters then
                if not meta.cachedIters[exp] then
                    meta.cachedIters[exp] = {}
                end
                table.insert(meta.cachedIters[exp], iterInfo)
            else
                meta.cachedIters = {}
                meta.cachedIters[exp] = {iterInfo}
            end
        end
    end
end

function wraparray(arr, size, classObj, typeKind)
    if not size then
        size = #arr
    end
    return setmetatable(
        arr,
        {
            __index = __mt_index_of_array,
            __count = size,
            __cs2lua_defined = true,
            __class = System.Collections.Generic.List_T
        }
    )
end

function newarraydim0(classObj, typeKind, defVal)
    error("illegal array !")
end

function newarraydim1(classObj, typeKind, defVal, size1)
    local arr = wraparray({}, size1, classObj, typeKind)
    for i = 1,size1 do
        arr[i] = defVal
    end
    return arr
end

function newarraydim2(classObj, typeKind, defVal, size1, size2)
    local arr = wraparray({}, size1, classObj, typeKind)
    for i = 1,size1 do
        arr[i] = wraparray({}, size2, classObj, typeKind)
        for j = 1,size2 do
            arr[i][j] = defVal
        end
    end
    return arr
end

function newarraydim3(classObj, typeKind, defVal, size1, size2, size3)
    local arr = wraparray({}, size1, classObj, typeKind)
    for i = 1,size1 do
        arr[i] = wraparray({}, size2, classObj, typeKind)
        for j = 1,size2 do
            arr[i][j] = wraparray({}, size3, classObj, typeKind)
            for k = 1,size3 do
                arr[i][j][k] = defVal
            end
        end
    end
    return arr
end

function wrapanonymousobject(dict)
    local obj = {}
    setmetatable(
        obj,
        {
            __index = __mt_index_of_dictionary,
            __newindex = __mt_newindex_of_dictionary,
            __cs2lua_defined = true,
            __class = System.Collections.Generic.Dictionary_TKey_TValue,
            __cs2lua_data = {}
        }
    )
    for k, v in pairs(dict) do
        obj:Add(k, v)
    end
    return obj
end

function wrapparams(arr, elementType, elementTypeKind)
    return wraparray(arr, nil, elementType, elementTypeKind)
end

function newdictionary(t, typeargs, typekinds, ctor, dict, ...)
    if dict then
        local obj = {}
        setmetatable(
            obj,
            {
                __index = __mt_index_of_dictionary,
                __newindex = __mt_newindex_of_dictionary,
                __cs2lua_defined = true,
                __class = t,
                __cs2lua_typeargs = typeargs,
                __cs2lua_typekinds = typekinds,
                __cs2lua_data = {}
            }
        )
        for k, v in pairs(dict) do
            obj:Add(k, v)
        end
        local arg1,arg2 = ...
        if type(arg1)=="string" or type(csstrtoluastr(arg1))=="string" then
            arg1 = arg2
        end
        if arg1 and (type(arg1)=="table" or type(arg1)=="userdata") then
            local iter = newiterator(arg1)
            for v in getiterator(iter) do
                obj:Add(v.Key, v.Value)
            end        
        end
        return obj
    end
end

function newlist(t, typeargs, typekinds, ctor, list, ...)
    if list then
        local obj = setmetatable(list, {__index = __mt_index_of_array, __count = #list, __cs2lua_defined = true, __class = t})
        local arg1,arg2 = ...
        if type(arg1)=="string" or type(csstrtoluastr(arg1))=="string" then
            arg1 = arg2
        end
        if arg1 and (type(arg1)=="table" or type(arg1)=="userdata") then
            --lualog("arg1:{0} {1}", arg1, type(arg1))
            local meta = getmetatable(arg1)
            if meta and not rawget(meta, "__cs2lua_defined") then
                local typename = rawget(meta, "__typename")
                if typename=="LuaArray" then
                    local ct = arg1.Length
                    for i = 1, ct do
                        obj:Add(arg1[i])
                    end
                end
            else
                obj:AddRange(arg1)                
            end
        end
        return obj
    end
end

function newcollection(t, typeargs, typekinds, ctor, coll, ...)
    if t == Cs2LuaList_T then
        return newlist(t, typeargs, typekinds, ctor, coll, ...)
    elseif t == Cs2LuaIntDictionary_TValue or t == Cs2LuaStringDictionary_TValue then
        return newdictionary(t, typeargs, typekinds, ctor, coll, ...)
    elseif coll then
        local obj = setmetatable({}, {__index = __mt_index_of_hashset, __cs2lua_defined = true, __class = t, __cs2lua_data = {}})
        for i, v in ipairs(coll) do
            obj:Add(v)
        end
        local arg1,arg2 = ...
        if type(arg1)=="string" or type(csstrtoluastr(arg1))=="string" then
            arg1 = arg2
        end
        if arg1 and (type(arg1)=="table" or type(arg1)=="userdata") then
            --lualog("arg1:{0} {1}", arg1, type(arg1))
            local meta = getmetatable(arg1)
            if meta and not rawget(meta, "__cs2lua_defined") then
                local typename = rawget(meta, "__typename")
                if typename=="LuaArray" then
                    local ct = arg1.Length
                    for i = 1, ct do
                        obj:Add(arg1[i])
                    end
                end
            else
                obj:AddRange(arg1)                
            end
        end
        return obj
    end
end

function newexterndictionary(t, typeargs, typekinds, dict, ...)
    if dict and t == System.Collections.Generic.Dictionary_TKey_TValue then
        return newdictionary(t, typeargs, typekinds, "ctor", dict, ...)
    else
        local obj = t(...)
        if obj then
            if dict ~= nil then
                for k, v in pairs(dict) do
                    obj:Add(k, v)
                end
            end
            return obj
        else
            return nil
        end
    end
end

function newexternlist(t, typeargs, typekinds, list, ...)
    if list and t == System.Collections.Generic.List_T then
        return newlist(t, typeargs, typekinds, "ctor", list, ...)
    else
        local obj = t(...)
        if obj then
            if list ~= nil then
                for i, v in ipairs(list) do
                    obj:Add(v)
                end
            end
            return obj
        else
            return nil
        end
    end
end

function newexterncollection(t, typeargs, typekinds, coll, ...)
    if coll and (t == System.Collections.Generic.Queue_T or t == System.Collections.Generic.Stack_T) then
        return newlist(t, typeargs, typekinds, "ctor", coll, ...)
    elseif coll and t == System.Collections.Generic.HashSet_T then
        return newcollection(t, typeargs, typekinds, "ctor", coll, ...)
    else
        local obj = t(...)
        if obj then
            if coll ~= nil then
                for i, v in ipairs(coll) do
                    obj:Add(v)
                end
            end
            return obj
        else
            return nil
        end
    end
end

function lshift(v, n)
    if bit then
        return bit.lshift(v, n)
    else
        for i = 1, n do
            v = v * 2
        end
        return v
    end
end

function rshift(v, n)
    if bit then
        return bit.rshift(v, n)
    else
        for i = 1, n do
            v = v / 2
        end
        return v
    end
end

function condexp(cv, tfIsSimple, tf, ffIsSimple, ff)
    if cv then
        if tfIsSimple then
            return tf
        else
            return tf()
        end
    else
        if ffIsSimple then
            return ff
        else
            return ff()
        end
    end
end

function condaccess(v, func)
    if v then
        return func()
    else
        return nil
    end
end

function nullcoalescing(v, func)
    if v then
        return v
    else
        return func()
    end
end

function bitnot(v)
    if bit then
        return bit.bnot(v)
    else
        return 0
    end
end

function bitand(v1, v2)
    if v1==true and v2==true then
        return true
    elseif v1==false and v2==false then
        return false
    elseif v1==true and v2==false then
        return false
    elseif v1==false and v2==true then
        return false
    end
    if bit then
        return bit.band(v1, v2)
    else
        return 0
    end
end

function bitor(v1, v2)
    if v1==true and v2==true then
        return true
    elseif v1==false and v2==false then
        return false
    elseif v1==true and v2==false then
        return true
    elseif v1==false and v2==true then
        return true
    end
    if bit then
        return bit.bor(v1, v2)
    else
        return 0
    end
end

function bitxor(v1, v2)
    if bit then
        return bit.bxor(v1, v2)
    else
        return 0
    end
end

LINQ = {}
LINQ.exec = function(linq)
    local paramList = {}
    local ix = 1
    return LINQ.execRecursively(linq, ix, paramList)
end
LINQ.execRecursively = function(linq, ix, paramList)
    local finalRs = {}
    local interRs = {}
    local itemNum = #linq
    while ix <= itemNum do
        local v = linq[ix]
        local key = v[1]
        ix = ix + 1

        if key == "from" then
            local nextIx = LINQ.getNextIndex(linq, ix)

            --获取目标集合
            local coll = v[2](unpack(paramList))
            LINQ.buildIntermediateResult(linq, ix, paramList, coll, interRs, finalRs)

            ix = nextIx
        elseif key == "where" then
            --在中间结果集上进行过滤处理
            local temp = interRs
            interRs = {}
            for i, val in ipairs(temp) do
                if v[2](unpack(val)) then
                    table.insert(interRs, val)
                end
            end
        elseif key == "orderby" then
            --排序（多关键字）
            table.sort(
                interRs,
                (function(l1, l2)
                    return LINQ.compare(l1, l2, v[2])
                end)
            )
        elseif key == "select" then
            --生成最终结果集
            for i, val in ipairs(interRs) do
                local r = v[2](unpack(val))
                table.insert(finalRs, r)
            end
        else
            --其它子句暂不支持。。
        end
    end
    return finalRs
end
LINQ.buildIntermediateResult = function(linq, ix, paramList, coll, interRs, finalRs)
    --遍历目标集合，处理连续的let与where (这时where条件可以在单个元素遍历时进行，不用等中间结果集构建后再过滤)
    --如果又遇到from，则递归调用自身来获取子集并合并到当前结果集
    for cv in getiterator(coll) do
        local newParamList = {unpack(paramList)}
        table.insert(newParamList, cv)
        local isMatch = true
        local newIx = ix
        local itemNum = #linq
        while newIx <= itemNum do
            local v = linq[newIx]
            local key = v[1]

            if key == "let" then
                table.insert(newParamList, v[2](unpack(newParamList)))
            elseif key == "where" then
                if not v[2](unpack(newParamList)) then
                    --不符合条件的记录不放到中间结果集
                    isMatch = false
                    break
                end
            elseif key == "from" then
                --再次遇到from，递归调用再合并结果集
                local ts = LINQ.execRecursively(linq, newIx, newParamList)
                for i, val in ipairs(ts) do
                    table.insert(finalRs, val)
                end
                isMatch = false
                break
            else
                --其它子句需要在中间结果集完成后再处理，这里跳过
                break
            end
            newIx = newIx + 1
        end
        if isMatch then
            table.insert(interRs, newParamList)
        end
    end
end
LINQ.compare = function(l1, l2, list)
    for i, v in ipairs(list) do
        local v1 = v[1](unpack(l1))
        local v2 = v[1](unpack(l2))
        local asc = v[2]
        if v1 ~= v2 then
            if asc then
                return v1 < v2
            else
                return v1 > v2
            end
        end
    end
    return true
end
LINQ.getNextIndex = function(linq, ix)
    local itemNum = #linq
    while ix <= itemNum do
        local v = linq[ix]
        local key = v[1]
        if key == "let" then
        elseif key == "where" then
        elseif key == "from" then
            return itemNum + 1
        else
            return ix
        end
        ix = ix + 1
    end
    return ix
end

function warmup(class)
    local ret = true
    if rawget(class, "__cs2lua_predefined") and not rawget(class, "__cs2lua_defined") then
        ret = class.__cs2lua_defined
    end
    return ret
end

function getobjfullname(obj)
    local ty = type(obj)
    if ty == "string" then
        return "System.String"
    elseif ty == "number" then
        return "System.Double"
    end
    local meta = getmetatable(obj)
    if meta then
        if rawget(meta, "__cs2lua_defined") then
            return rawget(meta, "__cs2lua_fullname")
        else
            local name = rawget(meta, "__fullname")
            local ix = string.find(name, ",")
            if ix == nil then
                return name
            else
                return string.sub(name, 1, ix - 1)
            end
        end
    else
        return nil
    end
end

function getobjtypename(obj)
    local ty = type(obj)
    if ty == "string" then
        return "System.String"
    elseif ty == "number" then
        return "System.Double"
    end
    local meta = getmetatable(obj)
    if meta then
        if rawget(meta, "__cs2lua_defined") then
            return rawget(meta, "__cs2lua_typename")
        else
            return rawget(meta, "__typename")
        end
    else
        return nil
    end
end

function getclassfullname(t)
    if t and type(t) ~= "string" then
        if type(t) ~= "table" then
            return tostring(t)
        else
            warmup(t)
            if rawget(t, "__cs2lua_defined") then
                return rawget(t, "__cs2lua_fullname")
            else
                local meta = getmetatable(t)
                if meta then
                    local name = rawget(meta, "__fullname")
                    local ix = string.find(name, ",")
                    if ix == nil then
                        return name
                    else
                        return string.sub(name, 1, ix - 1)
                    end
                else
                    return nil
                end
            end
        end
    else
        return t
    end
end

function getclasstypename(t)
    if t and type(t) ~= "string" then
        if type(t) ~= "table" then
            return tostring(t)
        else
            warmup(t)
            if rawget(t, "__cs2lua_defined") then
                return rawget(t, "__cs2lua_typename")
            else
                local meta = getmetatable(t)
                if meta then
                    return rawget(meta, "__typename")
                else
                    return nil
                end
            end
        end
    else
        return t
    end
end

function getobjparentclass(obj)
    local ty = type(obj)
    if ty == "string" or ty == "number" then
        return nil
    end
    local meta = getmetatable(obj)
    if meta then
        if rawget(meta, "__cs2lua_defined") then
            return rawget(meta, "__cs2lua_parent")
        else
            return rawget(meta, "__parent")
        end
    else
        return nil
    end
end

function getclassparentclass(t)
    if t and type(t) ~= "string" then
        if type(t) ~= "table" then
            return tostring(t)
        else
            warmup(t)
            if rawget(t, "__cs2lua_defined") then
                return rawget(t, "__cs2lua_parent")
            else
                return rawget(t, "__parent")
            end
        end
    else
        return nil
    end
end

function typecast(obj, t, tk)
    if t == System.String then
        return tostring(obj)
    elseif t == System.Single or t == System.Double then
        return tonumber(obj)
    elseif t == System.Int64 or t == System.UInt64 then
        local v = tonumber(obj)
        v = math.floor(v)
        return v
    elseif t == System.Int32 or t == System.UInt32 then
        local v = tonumber(obj)
        v = math.floor(v)
        if v > 0 then
            v = v % 0x100000000
        elseif v < 0 then
            v = -((-v) % 0x100000000)
        end
        if t == System.Int32 and v > 0x7fffffff then
            v = v - 0xffffffff - 1
        end
        return v
    elseif t == System.Int16 or t == System.UInt16 or t == System.Char then
        local v = tonumber(obj)
        v = math.floor(v)
        if v > 0 then
            v = v % 0x10000
        elseif v < 0 then
            v = -((-v) % 0x10000)
        end
        if t == System.Int16 and v > 0x7fff then
            v = v - 0xffff - 1
        end
        return v
    elseif t == System.SByte or t == System.Byte then
        local v = tonumber(obj)
        v = math.floor(v)
        if v > 0 then
            v = v % 0x100
        elseif v < 0 then
            v = -((-v) % 0x100)
        end
        if t == System.SByte and v > 0x7f then
            v = v - 0xff - 1
        end
        return v
    elseif t == System.Boolean then
        return obj
    elseif tk == TypeKind.Enum then
        return obj
    elseif typeis(obj, t, tk) then
        return obj
    else
        return obj
    end
end

function typeas(obj, t, tk)
    if t == System.String then
        return tostring(obj)
    elseif t == System.Single or t == System.Double then
        return tonumber(obj)
    elseif t == System.Int64 or t == System.UInt64 then
        local v = tonumber(obj)
        v = math.floor(v)
        return v
    elseif t == System.Int32 or t == System.UInt32 then
        return typecast(obj, t, tk)
    elseif t == System.Int16 or t == System.UInt16 or t == System.Char then
        return typecast(obj, t, tk)
    elseif t == System.SByte or t == System.Byte then
        return typecast(obj, t, tk)
    elseif t == System.Boolean then
        return obj
    elseif tk == TypeKind.Enum then
        return obj
    elseif typeis(obj, t, tk) then
        return obj
    elseif tk == TypeKind.Delegate then
        return obj
    else
        return nil
    end
end

function typeis(obj, t, tk)
    if obj == nil then
        return false
    end
    local meta = getmetatable(obj)
    local tn1 = getobjfullname(obj)
    local tn2 = getclassfullname(t)
    if meta then
        if type(obj) == "userdata" then
            if tn1 and tn1 == tn2 then
                return true
            end
            --check slua parent metatable chain
            local parent = rawget(meta, "__parent")
            while parent ~= nil do
                tn1 = rawget(parent, "__fullname")
                if tn1 and tn1 == tn2 then
                    return true
                end
                parent = rawget(parent, "__parent")
            end
        else
            if rawget(meta, "__class") == t then
                return true
            end
            local intfs = rawget(meta, "__interfaces")
            if intfs then
                for i, v in ipairs(intfs) do
                    if v == tn2 then
                        return true
                    end
                end
            end
            --check cs2lua base class chain
            local baseClass = rawget(meta, "__cs2lua_parent")
            local lastCheckedClass = meta
            while baseClass ~= nil do
                if baseClass == t then
                    return true
                end
                intfs = rawget(meta, "__interfaces")
                if intfs then
                    for i, v in ipairs(intfs) do
                        if v == tn2 then
                            return true
                        end
                    end
                end
                if rawget(baseClass, "__cs2lua_defined") then
                    baseClass = rawget(baseClass, "__cs2lua_parent")
                else
                    lastCheckedClass = baseClass
                    break
                end
            end
            --try slua base class and parent metatable chain
            if not rawget(lastCheckedClass, "__cs2lua_defined") then
                local meta3 = getmetatable(lastCheckedClass)
                if meta3 then
                    parent = rawget(meta3, "__parent")
                    while parent ~= nil do
                        tn1 = rawget(parent, "__fullname")
                        if tn1 and tn1 == tn2 then
                            return true
                        end
                        parent = rawget(parent, "__parent")
                    end
                end
            end
        end
    end
    if tk == TypeKind.Delegate and type(obj) == "function" then
        return true
    end
    return false
end

function __do_eq(v1, v2)
    return v1 == v2
end

function isequal(v1, v2)
    local succ, res = pcall(__do_eq, v1, v2)
    if succ then
        return res
    else
        return rawequal(v1, v2)
    end
end

function stringisequal(v1, v2)
    if v1==nil or v2==nil then
        return v1==v2
    elseif type(v1)=="string" and type(v2)=="string" then
        return v1==v2
    else
        local r = tostring(v1)==tostring(v2)
        --lualog("[debug] stringisequal [{0}],[{1}] {2}", v1, v2, r)
        --printStack()
        return r
    end
end

function __wrap_if_string(val)
    if type(val) == "string" then
        return System.String("String_Arr_Char", val)
    else
        return val
    end
end

function __unwrap_if_string(val)
    local meta = getmetatable(val)
    if type(val) == "userdata" and rawget(meta, "__typename") == "String" then
        return tostring(val)
    else
        return val
    end
end

function __find_base_class_key(k, base_class)
    if nil == k then
        UnityEngine.Debug.LogError("LogError_String", "[cs2lua] table index is nil")
        return false
    end
    if base_class then
        if rawget(base_class, "__cs2lua_defined") then
            if rawget(base_class, k) then
                return true
            else
                return base_class.__exist(k)
            end
        else
            return find_extern_class_or_obj_key(k,base_class)
        end
    end
    return false
end
function __find_class_key(k, class, class_fields, base_class)
    if nil == k then
        UnityEngine.Debug.LogError("LogError_String", "[cs2lua] table index is nil")
        return false
    end
    local ret
    ret = rawget(class, k)
    if nil ~= ret then
        return true
    end
    if class_fields then
        ret = class_fields[k]
        if nil ~= ret then
            return true
        end
    end
    return __find_base_class_key(k, base_class)
end
function __wrap_virtual_method(k, f)
    return function(this, ...)
        local child = rawget(this, "__child__")
        local final_nf = nil
        local final_child = nil
        while child do
            local nf = rawget(child, k)
            if nf then
                final_nf = nf
                final_child = child
            end
            child = rawget(child, "__child__")
        end
        if final_nf then
            return final_nf(final_child, ...)
        end
        return f(this, ...)
    end
end

function find_extern_class_or_obj_key(k, obj)
    local t=getmetatable(obj)
    repeat
        local fun=rawget(t,k)
        local tp=type(fun)        
        if tp=='function' then 
            return true 
        elseif tp=='table' then
            local f=fun[1]
            if f then
                return true
            end
        end
        t = rawget(t,'__parent')
    until t==nil
    return false
end

function __find_base_obj_key(k, baseObj)
    if nil == k then
        UnityEngine.Debug.LogError("LogError_String", "[cs2lua] table index is nil")
        return false
    end
    if baseObj then
        local meta = getmetatable(baseObj)
        if meta and rawget(meta, "__cs2lua_defined") then
            if rawget(baseObj, k) then
                return true
            else
                return baseObj:__exist(k)
            end
        else
            return find_extern_class_or_obj_key(k,baseObj)
        end
    end
    return false
end
function __find_obj_key(k, obj, obj_fields, obj_methods, baseObj)
    if nil == k then
        UnityEngine.Debug.LogError("LogError_String", "[cs2lua] table index is nil")
        return false
    end
    local ret
    ret = rawget(obj, k)
    if nil ~= ret then
        return true
    end
    if obj_fields then
        ret = obj_fields[k]
        if nil ~= ret then
            return true
        end
    end
    if obj_methods then
        ret = obj_methods[k]
        if nil ~= ret then
            return true
        end
    end
    return __find_base_obj_key(k, baseObj)
end
 
function defineclass(
    base,
    fullName,
    typeName,
    class,    
    obj_methods,
    obj_build,
    is_value_type)
        
    local base_class = base
    local mt = getmetatable(base_class)
    
    local class_fields = class.__class_fields
    local obj_fields = class.__obj_fields

    local interfaces = class.__interfaces
    local method_info = class.__method_info
    
    rawset(class, "__cs2lua_defined", true)
    rawset(class, "__cs2lua_fullname", fullName)
    rawset(class, "__cs2lua_typename", typeName)
    rawset(class, "__cs2lua_parent", base_class)
    rawset(class, "__is_value_type", is_value_type)
    rawset(class, "__interfaces", interfaces)
    
    --为继承与重载构建辅助函数
    if obj_methods then
        local temp_methods = {}
        for k, v in pairs(obj_methods) do
            temp_methods[k] = v
            if method_info then
                local minfo = method_info[k]
                if minfo then
                    if minfo["abstract"] or minfo["virtual"] or minfo["override"] then
                        temp_methods[k] = __wrap_virtual_method(k, v)
                    end
                    local result = string.find(k,"ctor",1,true)
                    if (result==1) or ((not minfo["private"]) and (not minfo["sealed"])) then
                        temp_methods["__self__" .. k] = v
                    end
                end
            end
        end
        obj_methods = temp_methods
    end

    local function __exist(fk)
        return __find_class_key(fk, class, class_fields, base_class)
    end
    local function __obj_exist(tb, fk)
        return __find_obj_key(fk, tb, obj_fields, obj_methods, rawget(tb, "base"))
    end

    local function obj_GetType(tb)
        return class
    end
    
    local obj_meta = {
        __class = class,
        __cs2lua_defined = true,
        __cs2lua_fullname = fullName,
        __cs2lua_typename = typeName,
        __cs2lua_parent = base_class,
        __is_value_type = is_value_type,
        __interfaces = interfaces,
        
        __index = function(t, k)
            if obj_methods then
                local f = obj_methods[k]
                if f then
                    return f
                end
            end
            if k == "__exist" then
                return __obj_exist
            elseif k == "GetType" then
                return obj_GetType
            elseif nil == k then
                UnityEngine.Debug.LogError("LogError_String", "[cs2lua] table index is nil")
                return nil
            end
            if obj_fields then
                local ret = obj_fields[k]
                if nil ~= ret then
                    --是当前类字段但仍走到元方法里了，则表明当前字段值为空
                    return nil
                end
            end
            local baseObj = rawget(t, "base")
            if __find_base_obj_key(k, baseObj) then
                return baseObj[k]
            end
            lualog("lookup meta for {0}.{1} base_class {2} baseObj {3}", t:GetType().FullName, k, base_class and base_class.FullName, baseObj)
            printStack()
            return nil
        end,
        __newindex = function(t, k, v)
            if nil == k then
                UnityEngine.Debug.LogError("LogError_String", "[cs2lua] table index is nil")
                return
            end
            if obj_fields then
                local ret = obj_fields[k]
                if nil ~= ret then
                    rawset(t, k, v)
                    return
                end
            end
            local baseObj = rawget(t, "base")
            if __find_base_obj_key(k, baseObj) then
                baseObj[k] = v
                return
            end
            rawset(t, k, v)
        end,
        __setbase = function(self, base)
            rawset(self, "base", base)
        end,
    }
    
    local class_meta = {
        __index = function(t, k)
            if k == "__exist" then
                return __exist
            elseif k == "Name" then
                return typeName
            elseif k == "FullName" then
                return fullName
            elseif nil == k then
                UnityEngine.Debug.LogError("LogError_String", "[cs2lua] table index is nil")
                return nil
            end
            if class_fields then
                local ret = class_fields[k]
                if nil ~= ret then
                    --字段存在仍然调到元方法，表明值为空
                    return nil
                end
            end
            if __find_base_class_key(k, base_class) then
                return base_class[k]
            end
            return nil
        end,
        __newindex = function(t, k, v)
            if nil == k then
                UnityEngine.Debug.LogError("LogError_String", "[cs2lua] table index is nil")
                return
            end
            if class_fields then
                local ret = class_fields[k]
                if nil ~= ret then
                    rawset(t, k, v)
                    return
                end
            end
            if __find_base_class_key(k, base_class) then
                base_class[k] = v
                return
            end
            rawset(t, k, v)
        end,
        __call = function()
            local baseObj = nil
            if base_class == UnityEngine.MonoBehaviour then
                baseObj = nil
            elseif mt then
                baseObj = mt.__call()
            end
            local obj = obj_build()
            if not obj then
                obj = {}
            end
            
            rawset(obj, "base", baseObj)
            if baseObj then
                rawset(baseObj, "__child__", obj)
            end

            setmetatable(obj, obj_meta)
            return obj
        end
    }

    setmetatable(class, class_meta)
    if class.cctor then
        class.cctor()
    end
    
    return class
end

function defineentry(class)
    _G.main = function()
        return class
    end
end

function newstruct(funcInfo, class, typeargs, typekinds, ctor, initializer, ...)
    local obj = class()
    if ctor then
        obj[ctor](obj, ...)
    end
    if obj and initializer then
        initializer(obj)
    end
    return obj
end

function newexternstruct(funcInfo, class, typeargs, typekinds, initializer, ...)
    local obj = nil
    local arg1,arg2 = ...
    if class == System.Nullable_T then
        return nil
    elseif class == System.Collections.Generic.KeyValuePair_TKey_TValue then
        return {Key = arg1, Value = arg2}
    end
    if class == UnityEngine.Vector2 then
        local x,y = ...
        obj = UnityEngine.Vector2.New(x,y)
        table.insert(funcInfo.v2_list, obj)
    elseif class == UnityEngine.Vector3 then
        local _,x,y,z = ...
        obj = UnityEngine.Vector3.New(x,y,z)
        table.insert(funcInfo.v3_list, obj)
    elseif class == UnityEngine.Vector4 then
        local _,x,y,z,w = ...
        obj = UnityEngine.Vector4.New(x,y,z,w)
        table.insert(funcInfo.v4_list, obj)
    elseif class == UnityEngine.Quaternion then
        local x,y,z,w = ...
        obj = UnityEngine.Quaternion.New(x,y,z,w)
        table.insert(funcInfo.q_list, obj)
    elseif class == UnityEngine.Color then
        local _,r,g,b,a = ...
        obj = UnityEngine.Color.New(r,g,b,a)
        table.insert(funcInfo.c_list, obj)
    elseif class == UnityEngine.Color32 then
        local r,g,b,a = ...
        obj = Color32Pool.Alloc()
        obj.r=r or 0
        obj.g=g or 0
        obj.b=b or 0
        obj.a=a or 1
        table.insert(funcInfo.c32_list, obj)
    else
        translationlog("need add handler for newexternstruct {0}", getclasstypename(class))
        obj = class(...)
    end
    if obj and initializer then
        initializer(obj)
    end
    return obj
end

function newobject(funcInfo, class, typeargs, typekinds, ctor, initializer, ...)
    local obj = class()
    if ctor then
        obj[ctor](obj, ...)
    end
    if obj and initializer then
        initializer(obj)
    end
    return obj
end

function newexternobject(funcInfo, class, typeargs, typekinds, initializer, ...)
    local obj = nil
    local arg1,arg2 = ...
    if class == System.Nullable_T then
        return nil
    elseif class == System.Collections.Generic.KeyValuePair_TKey_TValue then
        return {Key = arg1, Value = arg2}
    end
    if class == UnityEngine.Vector3 then
        obj = class(...)
    elseif class == UnityEngine.Vector4 then
        obj = class(...)
    elseif class == UnityEngine.Color then
        obj = class(...)
    else
        obj = class(...)
    end
    if obj and initializer then
        initializer(obj)
    end
    return obj
end

function newtypeparamobject(t)
    local obj = t()
    if rawget(t, "__cs2lua_defined") then
        if obj.ctor then
            obj:ctor()
        end
    end
    return obj
end

function defaultvalue(t, typename, isExtern)
    if t == UnityEngine.Vector3 then
        return UnityEngine.Vector3.zero
    elseif t == UnityEngine.Vector2 then
        return UnityEngine.Vector2.zero
    elseif t == UnityEngine.Vector4 then
        return UnityEngine.Vector4.zero
    elseif t == UnityEngine.Quaternion then
        return UnityEngine.Quaternion.identity
    elseif t == UnityEngine.Color then
        return UnityEngine.Color.black
    elseif t == UnityEngine.Color32 then
        return UnityEngine.Color32(0, 0, 0, 0)
    elseif isExtern then
        return t()
    else
        return t.__new_object()
    end
end

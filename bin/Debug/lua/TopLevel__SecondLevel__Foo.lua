require "cs2lua__utility";
require "cs2lua__attributes";
require "cs2lua__namespaces";
require "cs2lua__externenums";
require "cs2lua__interfaces";
require "TopLevel__SecondLevel__FooBase";
require "TopLevel__TestStruct";
require "TopLevel__Singleton_TopLevel_SecondLevel_Foo";
require "TopLevel__SecondLevel__GenericClass_TopLevel_SecondLevel_Foo_Test1__InnerGenericClass_TopLevel_SecondLevel_Foo_Test2";
require "TopLevel__Runnable";

TopLevel.SecondLevel.Foo = {
	__new_object = function(...)
		return newobject(TopLevel.SecondLevel.Foo, "ctor", nil, ...);
	end,
	__define_class = function()
		local static = TopLevel.SecondLevel.Foo;

		local static_methods = {
			add_StaticEventBridge = function(value)
			end,
			remove_StaticEventBridge = function(value)
			end,
			op_Addition__TopLevel_SecondLevel_Foo__TopLevel_SecondLevel_Foo = function(self, other)
				self.m_Test = invokeintegeroperator(2, "+", self.m_Test, other.m_Test, System.Int32, System.Int32);
				return self;
			end,
			op_Addition__TopLevel_SecondLevel_Foo__System_Int32 = function(self, val)
				self.m_Test = invokeintegeroperator(2, "+", self.m_Test, val, System.Int32, System.Int32);
				return self;
			end,
			op_Explicit = function(a)
				local f; f = newobject(TopLevel.SecondLevel.Foo, "ctor", nil);
				f.m_Test = a;
				f.Val = newobject(TopLevel.TestStruct, "ctor", nil);
				local ts; ts = f.Val;
				ts = wrapvaluetype(ts);
				setinstanceindexer(f, nil, "set_Item", ts, ts, 123);
				local r; r = getinstanceindexer(f, nil, "get_Item", ts, ts);
				condaccess(f, (function() return setinstanceindexer(f, nil, "set_Item", ts, ts, 123); end));
				r = condaccess(f, (function() return getinstanceindexer(f, nil, "get_Item", ts, ts); end));
				local result; result = TopLevel.Singleton_TopLevel_SecondLevel_Foo.instance:Test123(1, 2);
				TopLevel.Singleton_TopLevel_SecondLevel_Foo.instance = nil;
				return f;
			end,
			cctor = function()
				TopLevel.SecondLevel.FooBase.cctor(this);
			end,
		};

		local static_fields_build = function()
			local static_fields = {
				__attributes = TopLevel__SecondLevel__Foo__Attrs,
			};
			return static_fields;
		end;
		local static_props = nil;
		local static_events = {
			StaticEventBridge = {
				add = static_methods.add_StaticEventBridge,
				remove = static_methods.remove_StaticEventBridge,
			},
		};

		local instance_methods = {
			add_EventBridge = function(this, value)
			end,
			remove_EventBridge = function(this, value)
			end,
			get_Val = function(this)
				return this.m_TS;
			end,
			set_Val = function(this, value)
				value = wrapvaluetype(value);
				this.m_TS = value;
			end,
			get_Item = function(this, ...)
				local args = wrapvaluetypearray{...};
				return 1;
			end,
			set_Item = function(this, ...)
				local args = {...};
				local value = table.remove(args);
				args = wrapvaluetypearray(args);
			end,
			ctor = function(this)
				this:ctor__System_Int32(0);
				return this;
			end,
			ctor__System_Int32 = function(this, v)
				this.base.ctor(this);
				this.m_Test = v;
				return this;
			end,
			ctor__System_Int32__System_Int32 = function(this, a, b)
				this.base.ctor(this);
				return this;
			end,
			Test123 = function(this, a, b)
				return (a + b);
			end,
			GTest__TopLevel_SecondLevel_GenericClass_System_Int32 = function(this, arg)
			end,
			GTest__TopLevel_SecondLevel_GenericClass_System_Single = function(this, arg)
			end,
			Iterator = wrapenumerable(function(this)
				wrapyield(nil, false, false);
				wrapyield(newexternobject(UnityEngine.WaitForSeconds, "UnityEngine.WaitForSeconds", "ctor", nil, 3), false, true);
				return nil;
			end),
			Iterator2 = wrapenumerable(function(this)
				wrapyield(nil, false, false);
				return nil;
			end),
			Test = function(this)
				this:Test123(1, wrapconst(System.Single, "NegativeInfinity"));
				local abc; abc = wrapconst(System.Single, "NegativeInfinity");
				local t; t = newobject(TopLevel.SecondLevel.GenericClass_TopLevel_SecondLevel_Foo_Test1.InnerGenericClass_TopLevel_SecondLevel_Foo_Test2, "ctor", nil, newobject(TopLevel.SecondLevel.Foo.Test1, "ctor", nil), newobject(TopLevel.SecondLevel.Foo.Test2, "ctor", nil));
				t:Test(System.Int32, 123);
				t:Test2(System.Int32, newobject(TopLevel.SecondLevel.Foo.Test1, "ctor", nil), newobject(TopLevel.SecondLevel.Foo.Test2, "ctor", nil));
				local v;
				local vv; vv, v = this:TestLocal(__cs2lua_out);
				local ts; ts = newobject(TopLevel.TestStruct, "ctor", nil);
				ts = wrapvaluetype(ts);
				ts.A = 1;
				ts.B = 2;
				ts.C = 3;
				local ts2; ts2 = ts;
				ts2 = wrapvaluetype(ts2);
				local ts3;
				ts3 = ts;
				ts3 = wrapvaluetype(ts3);
				this:TestValueArg(ts);
				if delegationcomparewithnil(true, this, nil, "OnSimple", false) then
					this.OnSimple();
				end;
				local f; f = delegationwrap(this.OnSimple);
				if delegationcomparewithnil(false, f, nil, nil, false) then
					f();
				end;
			end,
			TestLocal = function(this, v)
				local ir; ir = newobject(TopLevel.Runnable, "ctor", nil);
								v = 1;
				return 2, v;
			end,
			TestValueArg = function(this, ts)
				ts = wrapvaluetype(ts);
				ts.A = 4;
				ts.B = 5;
				ts.C = 6;
			end,
			TestContinueAndReturn = function(this)
				local i; i = 0;
				while (i < 100) do
				repeat
					if (i < 10) then
						break;
					end;
					do
					return i;
					end;
				until true;
				i = invokeintegeroperator(2, "+", i, 1, System.Int32, System.Int32);
				end;
				return -1;
			end,
			TestSwitch = function(this)
				local i; i = 10;
				local __compiler_switch_556 = i;
				if (__compiler_switch_556 == 1) or (__compiler_switch_556 == 3) then
					return ;
				elseif __compiler_switch_556 == 2 then
					return ;
				else
					return ;
				end;
				if (i > 3) then
					return ;
				end;
				if typeis(this, TopLevel.SecondLevel.FooBase, false) then
					return ;
				end;
			end,
			__ctor = function(this)
				if this.__ctor_called then
					return;
				else
					this.__ctor_called = true;
				end
				this.m_TS = new TopLevel.TestStruct();
			end,
		};

		local instance_fields_build = function()
			local instance_fields = {
				OnSimple = wrapdelegation{},
				OnSimple2 = wrapdelegation{},
				m_Test = 0,
				m_Test2 = 0,
				m_TS = defaultvalue(TopLevel.TestStruct, "TopLevel.TestStruct", false),
				m_HashSet = newexterncollection(System.Collections.Generic.HashSet_T, "System.Collections.Generic.HashSet_T", "ctor", {"one", "two", "three"}),
				__attributes = TopLevel__SecondLevel__Foo__Attrs,
				__ctor_called = false,
			};
			return instance_fields;
		end;

		local instance_props = {
			Val = {
				get = instance_methods.get_Val,
				set = instance_methods.set_Val,
			},
		};

		local instance_events = {
			EventBridge = {
				add = instance_methods.add_EventBridge,
				remove = instance_methods.remove_EventBridge,
			},
		};

		local interfaces = nil;
		local interface_map = nil;

		return defineclass(TopLevel.SecondLevel.FooBase, "TopLevel.SecondLevel.Foo", static, static_methods, static_fields_build, static_props, static_events, instance_methods, instance_fields_build, instance_props, instance_events, interfaces, interface_map, false);
	end,
};




TopLevel.SecondLevel.Foo.Test1 = {
	__new_object = function(...)
		return newobject(TopLevel.SecondLevel.Foo.Test1, nil, nil, ...);
	end,
	__define_class = function()
		local static = TopLevel.SecondLevel.Foo.Test1;

		local static_methods = {
			cctor = function()
			end,
		};

		local static_fields_build = function()
			local static_fields = {
			};
			return static_fields;
		end;
		local static_props = nil;
		local static_events = nil;

		local instance_methods = {
			ctor = function(this)
			end,
		};

		local instance_fields_build = function()
			local instance_fields = {
			};
			return instance_fields;
		end;
		local instance_props = nil;
		local instance_events = nil;
		local interfaces = nil;
		local interface_map = nil;

		return defineclass(nil, "TopLevel.SecondLevel.Foo.Test1", static, static_methods, static_fields_build, static_props, static_events, instance_methods, instance_fields_build, instance_props, instance_events, interfaces, interface_map, false);
	end,
};



TopLevel.SecondLevel.Foo.Test1.__define_class();

TopLevel.SecondLevel.Foo.Test2 = {
	__new_object = function(...)
		return newobject(TopLevel.SecondLevel.Foo.Test2, nil, nil, ...);
	end,
	__define_class = function()
		local static = TopLevel.SecondLevel.Foo.Test2;

		local static_methods = {
			cctor = function()
			end,
		};

		local static_fields_build = function()
			local static_fields = {
			};
			return static_fields;
		end;
		local static_props = nil;
		local static_events = nil;

		local instance_methods = {
			ctor = function(this)
			end,
		};

		local instance_fields_build = function()
			local instance_fields = {
			};
			return instance_fields;
		end;
		local instance_props = nil;
		local instance_events = nil;
		local interfaces = nil;
		local interface_map = nil;

		return defineclass(nil, "TopLevel.SecondLevel.Foo.Test2", static, static_methods, static_fields_build, static_props, static_events, instance_methods, instance_fields_build, instance_props, instance_events, interfaces, interface_map, false);
	end,
};



TopLevel.SecondLevel.Foo.Test2.__define_class();

TopLevel.SecondLevel.Foo.FooChild = {
	__new_object = function(...)
		return newobject(TopLevel.SecondLevel.Foo.FooChild, nil, nil, ...);
	end,
	__define_class = function()
		local static = TopLevel.SecondLevel.Foo.FooChild;

		local static_methods = {
			cctor = function()
			end,
		};

		local static_fields_build = function()
			local static_fields = {
			};
			return static_fields;
		end;
		local static_props = nil;
		local static_events = nil;

		local instance_methods = {
			ctor = function(this)
			end,
		};

		local instance_fields_build = function()
			local instance_fields = {
				m_Test1 = 123,
				m_Test2 = 456,
			};
			return instance_fields;
		end;
		local instance_props = nil;
		local instance_events = nil;
		local interfaces = nil;
		local interface_map = nil;

		return defineclass(nil, "TopLevel.SecondLevel.Foo.FooChild", static, static_methods, static_fields_build, static_props, static_events, instance_methods, instance_fields_build, instance_props, instance_events, interfaces, interface_map, false);
	end,
};



TopLevel.SecondLevel.Foo.FooChild.__define_class();
TopLevel.SecondLevel.Foo.__define_class();

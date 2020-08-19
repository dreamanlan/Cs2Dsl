require "cs2lua__utility";
require "cs2lua__namespaces";
require "cs2lua__externenums";
require "cs2lua__interfaces";

TopLevel.Singleton_TopLevel_SecondLevel_Foo = {
	__new_object = function(...)
		return newobject(TopLevel.Singleton_TopLevel_SecondLevel_Foo, "ctor", nil, ...);
	end,
	__define_class = function()
		local static = TopLevel.Singleton_TopLevel_SecondLevel_Foo;

		local static_methods = {
			get_instance = function()
				if (TopLevel.Singleton_TopLevel_SecondLevel_Foo.ms_instance == nil) then
					TopLevel.Singleton_TopLevel_SecondLevel_Foo.ms_instance = newtypeparamobject(TopLevel.SecondLevel.Foo);
				end;
				return TopLevel.Singleton_TopLevel_SecondLevel_Foo.ms_instance;
			end,
			set_instance = function(value)
			end,
			Delete = function()
				TopLevel.Singleton_TopLevel_SecondLevel_Foo.ms_instance = nil;
			end,
			cctor = function()
			end,
		};

		local static_fields_build = function()
			local static_fields = {
				ms_instance = __cs2lua_nil_field_value,
			};
			return static_fields;
		end;

		local static_props = {
			instance = {
				get = static_methods.get_instance,
				set = static_methods.set_instance,
			},
		};

		local static_events = nil;

		local instance_methods = {
			ctor = function(this)
				if (TopLevel.Singleton_TopLevel_SecondLevel_Foo.ms_instance ~= nil) then
					UnityEngine.Debug.LogError("Cannot have two instances in singleton");
					return this;
				end;
				TopLevel.Singleton_TopLevel_SecondLevel_Foo.ms_instance = typecast(typecast(this, System.Object, false), TopLevel.SecondLevel.Foo, false);
				return this;
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

		return defineclass(nil, "TopLevel.Singleton_TopLevel_SecondLevel_Foo", static, static_methods, static_fields_build, static_props, static_events, instance_methods, instance_fields_build, instance_props, instance_events, interfaces, interface_map, false);
	end,
};



TopLevel.Singleton_TopLevel_SecondLevel_Foo.__define_class();

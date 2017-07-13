require("cs2dsl__utility");
require("cs2dsl__attributes");
require("cs2dsl__namespaces");
require("cs2dsl__externenums");
require("MyImpl");
require("DelegateTest");

class(ConvTest) {
	static_methods {
		__new_object = function(...){
			return(newobject(ConvTest, null, null, ...));
		};
		cctor = function(){
		};
	};
	static_fields {
		__attributes = ConvTest__Attrs;
	};
	static_props {};
	static_events {};

	instance_methods {
		get_Item = function(this, ix){
			return(0);
		},
		set_Item = function(this, ix, value){
		},
		get_Prop = function(this){
			return(this.m_Val);
		},
		set_Prop = function(this, value){
			this.m_Val = value;
		},
		TestConv = function(this, a, b){
			local(arr); arr = (function(){ local{arr = initializer(array){}}; local(d0); d0 = 1; for(i0 = 1,d0 ){ arr[i0] = 0; }; return(arr); })();
			local(dt); dt = newobject(DelegateTest, "ctor", null);
			setinstance(this, "Prop", invokeoperator(DelegateTest, "op_Implicit__DelegateTest", dt));
			setinstanceindexer(this, null, "set_Item", 0, invokeoperator(DelegateTest, "op_Implicit__DelegateTest", dt));
			arr[0] = invokeoperator(DelegateTest, "op_Implicit__DelegateTest", dt);
			local(v); v = invokeoperator(DelegateTest, "op_Implicit__System_Int32", getinstance(this, "Prop"));
			local(v2); v2 = invokeoperator(DelegateTest, "op_Implicit__System_Int32", getinstanceindexer(this, null, "get_Item", 0));
			local(v3); v3 = invokeoperator(DelegateTest, "op_Implicit__System_Int32", arr[0]);
			local(vv);
			local(vv2);
			local(vv3);
			vv = invokeoperator(DelegateTest, "op_Implicit__System_Int32", getinstance(this, "Prop"));
			vv2 = invokeoperator(DelegateTest, "op_Implicit__System_Int32", getinstanceindexer(this, null, "get_Item", 0));
			vv3 = invokeoperator(DelegateTest, "op_Implicit__System_Int32", arr[0]);
			local(v4); v4 = invokeoperator(DelegateTest, "op_Implicit__System_Int32", arr[0]);
			local(arr2); arr2 = null;
			v4 = invokeoperator(DelegateTest, "op_Implicit__System_Int32", condaccess(arr2, (function(){ return(arr2[0]); })));
			local(vv4); vv4 = invokeoperator(DelegateTest, "op_Implicit__System_Int32", condaccess(arr2, (function(){ return(arr2[0]); })));
			local(ct); ct = null;
			dt = invokeoperator(DelegateTest, "op_Implicit__System_Int32", condaccess(ct, (function(){ return(ct.Prop); })));
			dt = invokeoperator(DelegateTest, "op_Implicit__System_Int32", condaccess(ct, (function(){ return(getinstanceindexer(ct, null, "get_Item", 0)); })));
			local(v5); v5 = invokeoperator(DelegateTest, "op_Implicit__System_Int32", condaccess(ct, (function(){ return(getinstanceindexer(ct, null, "get_Item", 0)); })));
			local(vv5);
			vv5 = invokeoperator(DelegateTest, "op_Implicit__System_Int32", condaccess(ct, (function(){ return(getinstanceindexer(ct, null, "get_Item", 0)); })));
			return(null);
		};
		TestConv2 = function(this, a, b, out(c)){
			local(obj); obj = null;
			local(arr); arr = arrayinit(invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), invokeexternoperator(UnityEngine.Object, "op_Implicit", obj));
			local(dict); dict = newexterndictionary(System.Collections.Generic.Dictionary_TKey_TValue, "System.Collections.Generic.Dictionary_TKey_TValue", "ctor", dictionaryinit("1" -> invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), "2" -> invokeexternoperator(UnityEngine.Object, "op_Implicit", obj)));
			local(list); list = newexternlist(System.Collections.Generic.List_T, "System.Collections.Generic.List_T", "ctor", listinit(invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), invokeexternoperator(UnityEngine.Object, "op_Implicit", obj)));
			local(tarr);
			tarr = arrayinit(invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), invokeexternoperator(UnityEngine.Object, "op_Implicit", obj));
			local(tdict);
			tdict = newexterndictionary(System.Collections.Generic.Dictionary_TKey_TValue, "System.Collections.Generic.Dictionary_TKey_TValue", "ctor", dictionaryinit("1" -> invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), "2" -> invokeexternoperator(UnityEngine.Object, "op_Implicit", obj)));
			local(tlist);
			tlist = newexternlist(System.Collections.Generic.List_T, "System.Collections.Generic.List_T", "ctor", listinit(invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), invokeexternoperator(UnityEngine.Object, "op_Implicit", obj)));
			local(f); f = newexternlist(System.Collections.Generic.List_T, "System.Collections.Generic.List_T", "ctor", listinit(complexinit(obj, obj), complexinit(obj, obj)));
			c = 1;
			c = execbinary("/", c, 2, System.Int32, System.Int32, Struct, Struct, False, False);
			c = execbinary("/", c, 2, System.Int32, System.Int32, Struct, Struct, False, False);
			c = execbinary("%", c, 2, System.Int32, System.Int32, Struct, Struct, False, False);
			c = execbinary("%", c, 2, System.Int32, System.Int32, Struct, Struct, False, False);
			c = execbinary("-", c, 1, System.Int32, System.Int32, Struct, Struct, False, False);
			c = execbinary("-", c, 1, System.Int32, System.Int32, Struct, Struct, False, False);
			local(d); d = execunary("~", c, System.Int32, Struct, False);
			local(cc); cc = ( (function(){ c = execbinary("-", c, 1, System.Int32, System.Int32, Struct, Struct, False, False); return(c); })() );
			cc = ( c );
			c = execbinary("-", c, 1, System.Int32, System.Int32, Struct, Struct, False, False);
			return(null, c);
		};
		TestConv3 = function(this, a, b, out(c)){
			return(myTestConv4(this, a, b, out(c)));
		};
		TestConv4 = function(this, v, out(v2)){
			local(__compiler_expbody_88); __compiler_expbody_88 = execbinary("&&", execbinary("==", ( (function(){ v2 = 1; return(v2); })() ), 1, System.Int32, System.Int32, Struct, Struct, False, False), invokeexternoperator(UnityEngine.Object, "op_Implicit", newexternobject(UnityEngine.GameObject, "UnityEngine.GameObject", "ctor", null)), System.Boolean, System.Boolean, Struct, Struct, False, False); return(__compiler_expbody_88, v2);
		};
		ctor = function(this){
		};
	};
	instance_fields {
		OnHandleValue = wrapdelegation{};
		OnHandle = delegationwrap(this.OnHandleValue);
		m_Val = invokeoperator(DelegateTest, "op_Implicit__DelegateTest", newobject(DelegateTest, "ctor", null));
		__attributes = ConvTest__Attrs;
	};
	instance_props {
		Prop = {
			get = instance_methods.get_Prop,
			set = instance_methods.set_Prop,
		},
	};
	instance_events {};

	interfaces {};
	interface_map {};
};




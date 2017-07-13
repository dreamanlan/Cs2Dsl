require("cs2dsl__utility");
require("cs2dsl__namespaces");
require("cs2dsl__externenums");
require("ConvTest");

class(DelegateTest, UnityEngine.MonoBehaviour) {
	static_methods {
		__new_object = function(...){
			return(newobject(DelegateTest, null, null, ...));
		};
		op_Implicit__DelegateTest = function(thisObj){
			return(0);
		};
		op_Implicit__System_Int32 = function(v){
			return(newobject(DelegateTest, "ctor", null));
		};
		cctor = function(){
		};
	};
	static_fields {
	};
	static_props {};
	static_events {};

	instance_methods {
		get_Item = function(this, ix){
			return(null);
		},
		set_Item = function(this, ix, value){
		},
		get_ObjProp = function(this){
			return(null);
		},
		set_ObjProp = function(this, value){
		},
		NormalEnumerator = wrapenumerable(function(this){
			local(obj); obj = null;
			if( execunary("!", invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), System.Boolean, Struct, False) ){
				return(null);
			};
			wrapyield(0, false, false);
			callinstance(this, "Test2", invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), invokeexternoperator(UnityEngine.Object, "op_Implicit", obj));
			wrapyield(1, false, false);
			local(v1); v1 = invokeexternoperator(UnityEngine.Object, "op_Implicit", obj);
			local(v2); v2 = invokeexternoperator(UnityEngine.Object, "op_Implicit", obj);
			local(v3);
			v3 = invokeexternoperator(UnityEngine.Object, "op_Implicit", obj);
			local(vv); vv = callinstance(this, "Test", invokeexternoperator(UnityEngine.Object, "op_Implicit", obj));
			local(v4); v4 = 0;
			v4 = execbinary("+", v4, typecast(typecast(invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), System.Object, false), System.Int32, false), System.Int32, System.Int32, Struct, Struct, False, False);
			return(null);
		});
		Test = function(this, v){
			delegationset(false, false, "DelegateTest:Fading", this, null, "Fading", (function(){ local(__compiler_delegation_128); __compiler_delegation_128 = (function(){ return(this.NormalEnumerator()); }); setdelegationkey(__compiler_delegation_128, "DelegateTest:NormalEnumerator", this, this.NormalEnumerator); return(__compiler_delegation_128); })());
			callinstance(this, "StartCoroutine", this.Fading());
			local(obj); obj = null;
			local(v0); v0 = condexp(invokeexternoperator(UnityEngine.Object, "op_Implicit", ( obj )), true, 1, true, 0);
			local(f0); f0 = delegationwrap((function(vv) { return(invokeexternoperator(UnityEngine.Object, "op_Implicit", obj)); };));
			local(f); f = delegationwrap((function(vv){ return(invokeexternoperator(UnityEngine.Object, "op_Implicit", obj)); }));
			local(td); td = delegationwrap((function(a, ref(b), out(c)){ local(__compiler_lambda_134); __compiler_lambda_134 = execbinary("+", execbinary("+", a, ( (function(){ b = 2; return(b); })() ), System.Int32, System.Int32, Struct, Struct, False, False), ( (function(){ c = 1; return(c); })() ), System.Int32, System.Int32, Struct, Struct, False, False); return(__compiler_lambda_134, b, c); }));
			if( invokeexternoperator(UnityEngine.Object, "op_Implicit", obj) ){
				callinstance(this, "Test2", true, false);
			};
			do{
			}while(invokeexternoperator(UnityEngine.Object, "op_Implicit", obj));
			while( invokeexternoperator(UnityEngine.Object, "op_Implicit", obj) ){
			};
			while( invokeexternoperator(UnityEngine.Object, "op_Implicit", obj) ){
			};
			local(ct); ct = newobject(ConvTest, "ctor", (function(newobj){ newobj.m_Val = invokeoperator(DelegateTest, "op_Implicit__DelegateTest", newobject(DelegateTest, "ctor", null)); }));
			return(invokeexternoperator(UnityEngine.Object, "op_Implicit", obj));
		};
		Test2 = function(this, v1, v2){
			if( execbinary("||", v1, v2, System.Boolean, System.Boolean, Struct, Struct, False, False) ){
				callstatic(UnityEngine.Debug, "Log", v1);
				callstatic(UnityEngine.Debug, "Log", v2);
			};
			local(tc); tc = newobject(ConvTest, "ctor", null);
			local(vv); vv = invokeexternoperator(UnityEngine.Object, "op_Implicit", callinstance(tc, "TestConv", 1, invokeoperator(DelegateTest, "op_Implicit__DelegateTest", this)));
			local(vv2);
			vv2 = invokeexternoperator(UnityEngine.Object, "op_Implicit", callinstance(tc, "TestConv", 1, 2));
			local(r);
			local(vv3); vv3 = invokeexternoperator(UnityEngine.Object, "op_Implicit", (function(){ local(__compiler_localdecl_160); multiassign(__compiler_localdecl_160, r) = callinstance(tc, "TestConv2", 1, invokeoperator(DelegateTest, "op_Implicit__DelegateTest", this), __cs2dsl_out); return(__compiler_localdecl_160); })());
			local(vv4);
			vv4 = invokeexternoperator(UnityEngine.Object, "op_Implicit", (function(){ local(__compiler_assigninvoke_162); multiassign(__compiler_assigninvoke_162, r) = callinstance(tc, "TestConv2", 2, invokeoperator(DelegateTest, "op_Implicit__DelegateTest", this), __cs2dsl_out); return(__compiler_assigninvoke_162); })());
			local(vv5);
			vv5 = invokeoperator(DelegateTest, "op_Implicit__DelegateTest", (function(){ local(__compiler_assigninvoke_164); multiassign(__compiler_assigninvoke_164, r) = callinstance(tc, "TestConv3", 3, invokeoperator(DelegateTest, "op_Implicit__DelegateTest", this), __cs2dsl_out); return(__compiler_assigninvoke_164); })());
			setinstance(tc, "Prop", 123);
			local(vvv); vvv = getinstance(tc, "Prop");
			callinstance(this, "Test", invokeexternoperator(UnityEngine.Object, "op_Implicit", callinstance(tc, "TestConv", invokeoperator(DelegateTest, "op_Implicit__DelegateTest", this), 2)));
			local(arr); arr = arrayinit(null);
			callinstance(this, "Test", invokeexternoperator(UnityEngine.Object, "op_Implicit", arr[0]));
			callinstance(this, "Test", invokeexternoperator(UnityEngine.Object, "op_Implicit", getinstance(this, "ObjProp")));
			callinstance(this, "Test", invokeexternoperator(UnityEngine.Object, "op_Implicit", getinstanceindexer(this, null, "get_Item", 0)));
		};
		ctor = function(this){
		};
	};
	instance_fields {
		Fading = wrapdelegation{};
	};
	instance_props {
		ObjProp = {
			get = instance_methods.get_ObjProp,
			set = instance_methods.set_ObjProp,
		},
	};
	instance_events {};

	interfaces {};
	interface_map {};
};




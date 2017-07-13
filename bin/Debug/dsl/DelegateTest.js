require("cs2dsl__utility.js");
require("cs2dsl__namespaces.js");
require("cs2dsl__externenums.js");
require("ConvTest.js");

function DelegateTest(){
	UnityEngine.MonoBehaviour.call(this);

	this.get_Item = function(ix){
		return null;
	}
	this.set_Item = function(ix, value){
	}
	this.get_ObjProp = function(){
		return null;
	}
	this.set_ObjProp = function(value){
	}
	this.Test = function(v){
		delegationset(false, false, "DelegateTest:Fading", this, null, "Fading", (function(){
			var __compiler_delegation_128;
			__compiler_delegation_128 = (function(){
				return this.NormalEnumerator();
			});
			setdelegationkey(__compiler_delegation_128, "DelegateTest:NormalEnumerator", this, this.NormalEnumerator);
			return __compiler_delegation_128;
		})());
		this.StartCoroutine(this.Fading());
		var obj;
		obj = null;
		var v0;
		v0 = condexp(invokeexternoperator(UnityEngine.Object, "op_Implicit", (obj)), true, 1, true, 0);
		var f0;
		f0 = delegationwrap((function(vv){
			return invokeexternoperator(UnityEngine.Object, "op_Implicit", obj);
		}));
		var f;
		f = delegationwrap((function(vv){
			return invokeexternoperator(UnityEngine.Object, "op_Implicit", obj);
		}));
		var td;
		td = delegationwrap((function(a, ref(b), out(c)){
			var __compiler_lambda_134;
			__compiler_lambda_134 = a + ((function(){
				b = 2;
				return b;
			})()) + ((function(){
				c = 1;
				return c;
			})());
			return [__compiler_lambda_134, b, c];
		}));
		if(invokeexternoperator(UnityEngine.Object, "op_Implicit", obj)){
			this.Test2(true, false);
		};
		do{
		}while(invokeexternoperator(UnityEngine.Object, "op_Implicit", obj)) ;
		while(invokeexternoperator(UnityEngine.Object, "op_Implicit", obj)){
		};
		while(invokeexternoperator(UnityEngine.Object, "op_Implicit", obj)){
		};
		var ct;
		ct = newobject(ConvTest, "ctor", (function(newobj){
			newobj.m_Val = invokeoperator(DelegateTest, "op_Implicit__DelegateTest", newobject(DelegateTest, "ctor", null));
		}));
		return invokeexternoperator(UnityEngine.Object, "op_Implicit", obj);
	}
	this.Test2 = function(v1, v2){
		if(v1 || v2){
			UnityEngine.Debug.Log(v1);
			UnityEngine.Debug.Log(v2);
		};
		var tc;
		tc = newobject(ConvTest, "ctor", null);
		var vv;
		vv = invokeexternoperator(UnityEngine.Object, "op_Implicit", tc.TestConv(1, invokeoperator(DelegateTest, "op_Implicit__DelegateTest", this)));
		var vv2;
		vv2 = invokeexternoperator(UnityEngine.Object, "op_Implicit", tc.TestConv(1, 2));
		var r;
		var vv3;
		vv3 = invokeexternoperator(UnityEngine.Object, "op_Implicit", (function(){
			var __compiler_localdecl_160;
			var __compiler_multiassign_83 = tc.TestConv2(1, invokeoperator(DelegateTest, "op_Implicit__DelegateTest", this), __cs2dsl_out);__compiler_localdecl_160 = __compiler_multiassign_83[0];r = __compiler_multiassign_83[1];
			return __compiler_localdecl_160;
		})());
		var vv4;
		vv4 = invokeexternoperator(UnityEngine.Object, "op_Implicit", (function(){
			var __compiler_assigninvoke_162;
			var __compiler_multiassign_85 = tc.TestConv2(2, invokeoperator(DelegateTest, "op_Implicit__DelegateTest", this), __cs2dsl_out);__compiler_assigninvoke_162 = __compiler_multiassign_85[0];r = __compiler_multiassign_85[1];
			return __compiler_assigninvoke_162;
		})());
		var vv5;
		vv5 = invokeoperator(DelegateTest, "op_Implicit__DelegateTest", (function(){
			var __compiler_assigninvoke_164;
			var __compiler_multiassign_87 = tc.TestConv3(3, invokeoperator(DelegateTest, "op_Implicit__DelegateTest", this), __cs2dsl_out);__compiler_assigninvoke_164 = __compiler_multiassign_87[0];r = __compiler_multiassign_87[1];
			return __compiler_assigninvoke_164;
		})());
		tc.Prop = 123;
		var vvv;
		vvv = tc.Prop;
		this.Test(invokeexternoperator(UnityEngine.Object, "op_Implicit", tc.TestConv(invokeoperator(DelegateTest, "op_Implicit__DelegateTest", this), 2)));
		var arr;
		arr = [null];
		this.Test(invokeexternoperator(UnityEngine.Object, "op_Implicit", arr[0]));
		this.Test(invokeexternoperator(UnityEngine.Object, "op_Implicit", this.ObjProp));
		this.Test(invokeexternoperator(UnityEngine.Object, "op_Implicit", getinstanceindexer(this, null, "get_Item", 0)));
	}
	this.ctor = function(){
	}
	this.Fading = wrapdelegation{
	};
}

(function(){
	DelegateTest.__new_object = function(){
		return newobject(DelegateTest, null, null, getParams(0));
	}
	DelegateTest.op_Implicit__DelegateTest = function(thisObj){
		return 0;
	}
	DelegateTest.op_Implicit__System_Int32 = function(v){
		return newobject(DelegateTest, "ctor", null);
	}
	DelegateTest.cctor = function(){
	}
})()

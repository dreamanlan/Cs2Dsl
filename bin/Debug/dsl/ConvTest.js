require("cs2dsl__utility.js");
require("cs2dsl__attributes.js");
require("cs2dsl__namespaces.js");
require("cs2dsl__externenums.js");
require("MyImpl.js");
require("DelegateTest.js");

function ConvTest(){
	this.get_Item = function(ix){
		return 0;
	}
	this.set_Item = function(ix, value){
	}
	this.get_Prop = function(){
		return this.m_Val;
	}
	this.set_Prop = function(value){
		this.m_Val = value;
	}
	this.TestConv = function(a, b){
		var arr;
		arr = (function(){
			var arr = initializer(array){
			};
			var d0;
			d0 = 1;
			for(i0 = 1, d0){
				arr[i0] = 0;
			};
			return arr;
		})();
		var dt;
		dt = newobject(DelegateTest, "ctor", null);
		this.Prop = invokeoperator(DelegateTest, "op_Implicit__DelegateTest", dt);
		setinstanceindexer(this, null, "set_Item", 0, invokeoperator(DelegateTest, "op_Implicit__DelegateTest", dt));
		arr[0] = invokeoperator(DelegateTest, "op_Implicit__DelegateTest", dt);
		var v;
		v = invokeoperator(DelegateTest, "op_Implicit__System_Int32", this.Prop);
		var v2;
		v2 = invokeoperator(DelegateTest, "op_Implicit__System_Int32", getinstanceindexer(this, null, "get_Item", 0));
		var v3;
		v3 = invokeoperator(DelegateTest, "op_Implicit__System_Int32", arr[0]);
		var vv;
		var vv2;
		var vv3;
		vv = invokeoperator(DelegateTest, "op_Implicit__System_Int32", this.Prop);
		vv2 = invokeoperator(DelegateTest, "op_Implicit__System_Int32", getinstanceindexer(this, null, "get_Item", 0));
		vv3 = invokeoperator(DelegateTest, "op_Implicit__System_Int32", arr[0]);
		var v4;
		v4 = invokeoperator(DelegateTest, "op_Implicit__System_Int32", arr[0]);
		var arr2;
		arr2 = null;
		v4 = invokeoperator(DelegateTest, "op_Implicit__System_Int32", condaccess(arr2, (function(){
			return arr2[0];
		})));
		var vv4;
		vv4 = invokeoperator(DelegateTest, "op_Implicit__System_Int32", condaccess(arr2, (function(){
			return arr2[0];
		})));
		var ct;
		ct = null;
		dt = invokeoperator(DelegateTest, "op_Implicit__System_Int32", condaccess(ct, (function(){
			return ct.Prop;
		})));
		dt = invokeoperator(DelegateTest, "op_Implicit__System_Int32", condaccess(ct, (function(){
			return getinstanceindexer(ct, null, "get_Item", 0);
		})));
		var v5;
		v5 = invokeoperator(DelegateTest, "op_Implicit__System_Int32", condaccess(ct, (function(){
			return getinstanceindexer(ct, null, "get_Item", 0);
		})));
		var vv5;
		vv5 = invokeoperator(DelegateTest, "op_Implicit__System_Int32", condaccess(ct, (function(){
			return getinstanceindexer(ct, null, "get_Item", 0);
		})));
		return null;
	}
	this.TestConv2 = function(a, b, c){
		var obj;
		obj = null;
		var arr;
		arr = [invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), invokeexternoperator(UnityEngine.Object, "op_Implicit", obj)];
		var dict;
		dict = newexterndictionary(System.Collections.Generic.Dictionary_TKey_TValue, "System.Collections.Generic.Dictionary_TKey_TValue", "ctor", {1"invokeexternoperator" : UnityEngine.Object, 2"invokeexternoperator" : UnityEngine.Object});
		var list;
		list = newexternlist(System.Collections.Generic.List_T, "System.Collections.Generic.List_T", "ctor", [invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), invokeexternoperator(UnityEngine.Object, "op_Implicit", obj)]);
		var tarr;
		tarr = [invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), invokeexternoperator(UnityEngine.Object, "op_Implicit", obj)];
		var tdict;
		tdict = newexterndictionary(System.Collections.Generic.Dictionary_TKey_TValue, "System.Collections.Generic.Dictionary_TKey_TValue", "ctor", {1"invokeexternoperator" : UnityEngine.Object, 2"invokeexternoperator" : UnityEngine.Object});
		var tlist;
		tlist = newexternlist(System.Collections.Generic.List_T, "System.Collections.Generic.List_T", "ctor", [invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), invokeexternoperator(UnityEngine.Object, "op_Implicit", obj), invokeexternoperator(UnityEngine.Object, "op_Implicit", obj)]);
		var f;
		f = newexternlist(System.Collections.Generic.List_T, "System.Collections.Generic.List_T", "ctor", [complexinit(obj, obj), complexinit(obj, obj)]);
		c = 1;
		c = c / 2;
		c = c / 2;
		c = c % 2;
		c = c % 2;
		c = c - 1;
		c = c - 1;
		var d;
		d = ~ c;
		var cc;
		cc = ((function(){
			c = c - 1;
			return c;
		})());
		cc = (c);
		c = c - 1;
		return [null, c];
	}
	this.TestConv3 = function(a, b, c){
		return myTestConv4(this, a, b, out(c));
	}
	this.TestConv4 = function(v, v2){
		var __compiler_expbody_88;
		__compiler_expbody_88 = ((function(){
			v2 = 1;
			return v2;
		})()) == 1 && invokeexternoperator(UnityEngine.Object, "op_Implicit", newexternobject(UnityEngine.GameObject, "UnityEngine.GameObject", "ctor", null));
		return [__compiler_expbody_88, v2];
	}
	this.ctor = function(){
	}
	this.OnHandleValue = wrapdelegation{
	};
	this.OnHandle = delegationwrap(this.OnHandleValue);
	this.m_Val = invokeoperator(DelegateTest, "op_Implicit__DelegateTest", newobject(DelegateTest, "ctor", null));
	this.__attributes = ConvTest__Attrs;
}

(function(){
	ConvTest.__new_object = function(){
		return newobject(ConvTest, null, null, getParams(0));
	}
	ConvTest.cctor = function(){
	}
	ConvTest.__attributes = ConvTest__Attrs;
})()

require("cs2dsl__lualib");
require("cs2dsl__namespaces");
require("cs2dsl__externenums");
require("cs2dsl__interfaces");

class(Mandelbrot) {
	static_methods {
		__new_object = function(...){
			return(newobject(Mandelbrot, typeargs(), typekinds(), "ctor", null, ...));
		};
		Test = function(){
			callinstance(newobject(Mandelbrot, typeargs(), typekinds(), "ctor", null), "Exec");
		};
		cctor = function(){
			callstatic(Mandelbrot, "__cctor");
		};
		__cctor = function(){
			if(Mandelbrot.__cctor_called){
				return;
			}else{
				Mandelbrot.__cctor_called = true;
			};
		};
	};
	static_fields {
		__cctor_called = false;
	};
	static_props {};
	static_events {};

	instance_methods {
		Exec = function(this){
			local(width); width = 50;
			local(height); height = width;
			local(maxiter); maxiter = 50;
			local(limit); limit = 4.00;
			local(y); y = 0;
			while( execbinary("<", y, height, System.Int32, System.Int32, TypeKind.Struct, TypeKind.Struct) ){
				local(Ci); Ci = execbinary("-", execbinary("/", execbinary("*", 2.00, y, System.Double, System.Double, TypeKind.Struct, TypeKind.Struct), height, System.Double, System.Double, TypeKind.Struct, TypeKind.Struct), 1.00, System.Double, System.Double, TypeKind.Struct, TypeKind.Struct);
				local(x); x = 0;
				while( execbinary("<", x, width, System.Int32, System.Int32, TypeKind.Struct, TypeKind.Struct) ){
					local(Zr); Zr = 0.00;
					local(Zi); Zi = 0.00;
					local(Cr); Cr = execbinary("-", execbinary("/", execbinary("*", 2.00, x, System.Double, System.Double, TypeKind.Struct, TypeKind.Struct), width, System.Double, System.Double, TypeKind.Struct, TypeKind.Struct), 1.50, System.Double, System.Double, TypeKind.Struct, TypeKind.Struct);
					local(i); i = maxiter;
					local(isInside); isInside = true;
					do{
						local(Tr); Tr = execbinary("+", execbinary("-", execbinary("*", Zr, Zr, System.Double, System.Double, TypeKind.Struct, TypeKind.Struct), execbinary("*", Zi, Zi, System.Double, System.Double, TypeKind.Struct, TypeKind.Struct), System.Double, System.Double, TypeKind.Struct, TypeKind.Struct), Cr, System.Double, System.Double, TypeKind.Struct, TypeKind.Struct);
						Zi = execbinary("+", execbinary("*", execbinary("*", 2.00, Zr, System.Double, System.Double, TypeKind.Struct, TypeKind.Struct), Zi, System.Double, System.Double, TypeKind.Struct, TypeKind.Struct), Ci, System.Double, System.Double, TypeKind.Struct, TypeKind.Struct);
						Zr = Tr;
						if( execbinary(">", execbinary("+", execbinary("*", Zr, Zr, System.Double, System.Double, TypeKind.Struct, TypeKind.Struct), execbinary("*", Zi, Zi, System.Double, System.Double, TypeKind.Struct, TypeKind.Struct), System.Double, System.Double, TypeKind.Struct, TypeKind.Struct), limit, System.Double, System.Double, TypeKind.Struct, TypeKind.Struct) ){
							isInside = false;
							break;
						};
					}while(execbinary(">", (function(){ i = execbinary("-", i, 1, System.Int32, System.Int32, TypeKind.Struct, TypeKind.Struct); return(i); })(), 0, System.Int32, System.Int32, TypeKind.Struct, TypeKind.Struct));
					if( isInside ){
						callinstance(this, "Output", execbinary("/", execbinary("*", x, 1.00, System.Single, System.Single, TypeKind.Struct, TypeKind.Struct), width, System.Single, System.Single, TypeKind.Struct, TypeKind.Struct), execbinary("/", execbinary("*", y, 1.00, System.Single, System.Single, TypeKind.Struct, TypeKind.Struct), height, System.Single, System.Single, TypeKind.Struct, TypeKind.Struct));
					};
				x = execbinary("+", x, 1, System.Int32, System.Int32, TypeKind.Struct, TypeKind.Struct);
				};
			y = execbinary("+", y, 1, System.Int32, System.Int32, TypeKind.Struct, TypeKind.Struct);
			};
		};
		Output = function(this, x, y){
			callstatic(JsConsole, "Print", x, y);
		};
		ctor = function(this){
			callinstance(this, "__ctor");
		};
		__ctor = function(this){
			if(getinstance(SymbolKind.Field, this, "__ctor_called")){
				return;
			}else{
				setinstance(SymbolKind.Field, this, "__ctor_called", true);
			};
			this.datas = literalarray(System.Int32, 1, 2, 3, 4, 5, 6);
			this.dicts = newexterndictionary(System.Collections.Generic.Dictionary_TKey_TValue, typeargs(System.Int32, System.Int32), typekinds(TypeKind.Struct, TypeKind.Struct), literaldictionary(1 => 1, 2 => 2, 3 => 3, 4 => 4, 5 => 5), "System.Collections.Generic.Dictionary_TKey_TValue:ctor");
			this.dicts2 = newexterndictionary(System.Collections.Generic.Dictionary_TKey_TValue, typeargs(System.Int32, System.Int32), typekinds(TypeKind.Struct, TypeKind.Struct), literaldictionary(), "System.Collections.Generic.Dictionary_TKey_TValue:ctor__Int32", 128);
		};
	};
	instance_fields {
		r = 10;
		scale = 3.00;
		datas = null;
		dicts = null;
		dicts2 = null;
		__ctor_called = false;
	};
	instance_props {};
	instance_events {};

	interfaces {};
	interface_map {};

	class_info(TypeKind.Class, Accessibility.Internal) {
	};
	method_info {
		Exec(MethodKind.Ordinary, Accessibility.Public){
		};
		Output(MethodKind.Ordinary, Accessibility.Private){
		};
		Test(MethodKind.Ordinary, Accessibility.Public){
			static(true);
		};
		ctor(MethodKind.Constructor, Accessibility.Public){
		};
	};
	property_info {};
	event_info {};
	field_info {
		r(Accessibility.Private){
		};
		scale(Accessibility.Private){
		};
		datas(Accessibility.Private){
		};
		dicts(Accessibility.Private){
		};
		dicts2(Accessibility.Private){
		};
	};
};




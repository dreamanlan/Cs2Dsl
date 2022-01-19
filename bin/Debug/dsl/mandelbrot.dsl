require("cs2dsl__syslib");
require("cs2dsl__namespaces");
require("cs2dsl__externenums");
require("cs2dsl__interfaces");

class(Mandelbrot) {
	static_methods {
		__new_object = deffunc(1)args(...){
			local(__cs2dsl_newobj);__cs2dsl_newobj = newobject(Mandelbrot, "g_Mandelbrot", typeargs(), typekinds(), "ctor", 0, null, ...);
			return(__cs2dsl_newobj);
		}options[needfuncinfo(false)];
		Test = deffunc(0)args(){
			callinstance(newobject(Mandelbrot, "g_Mandelbrot", typeargs(), typekinds(), "ctor", 0, null), Mandelbrot, "Exec");
		}options[needfuncinfo(false), rettype(return, System.Void, TypeKind.Unknown, 0, false)];
		cctor = deffunc(0)args(){
			callstatic(Mandelbrot, "__cctor");
		};
		__cctor = deffunc(0)args(){
			if(Mandelbrot.__cctor_called){
				return();
			}else{
				Mandelbrot.__cctor_called = true;
			};
		}options[needfuncinfo(false)];
	};
	static_fields {
		__cctor_called = false;
	};
	static_props {};
	static_events {};

	instance_methods {
		Exec = deffunc(0)args(this){
			local(width); width = 50;
			local(height); height = width;
			local(maxiter); maxiter = 50;
			local(limit); limit = 4.0000000000000000;
			local(y); y = 0;
			while( execbinary("<", y, height, System.Int32, System.Int32, TypeKind.Structure, TypeKind.Structure) ){
				local(Ci); Ci = execbinary("-", execbinary("/", execbinary("*", 2.0000000000000000, y, System.Double, System.Double, TypeKind.Structure, TypeKind.Structure), height, System.Double, System.Double, TypeKind.Structure, TypeKind.Structure), 1.0000000000000000, System.Double, System.Double, TypeKind.Structure, TypeKind.Structure);
				local(x); x = 0;
				while( execbinary("<", x, width, System.Int32, System.Int32, TypeKind.Structure, TypeKind.Structure) ){
					local(Zr); Zr = 0.0000000000000000;
					local(Zi); Zi = 0.0000000000000000;
					local(Cr); Cr = execbinary("-", execbinary("/", execbinary("*", 2.0000000000000000, x, System.Double, System.Double, TypeKind.Structure, TypeKind.Structure), width, System.Double, System.Double, TypeKind.Structure, TypeKind.Structure), 1.5000000000000000, System.Double, System.Double, TypeKind.Structure, TypeKind.Structure);
					local(i); i = maxiter;
					local(isInside); isInside = true;
					do{
						local(Tr); Tr = execbinary("+", execbinary("-", execbinary("*", Zr, Zr, System.Double, System.Double, TypeKind.Structure, TypeKind.Structure), execbinary("*", Zi, Zi, System.Double, System.Double, TypeKind.Structure, TypeKind.Structure), System.Double, System.Double, TypeKind.Structure, TypeKind.Structure), Cr, System.Double, System.Double, TypeKind.Structure, TypeKind.Structure);
						Zi = execbinary("+", execbinary("*", execbinary("*", 2.0000000000000000, Zr, System.Double, System.Double, TypeKind.Structure, TypeKind.Structure), Zi, System.Double, System.Double, TypeKind.Structure, TypeKind.Structure), Ci, System.Double, System.Double, TypeKind.Structure, TypeKind.Structure);
						Zr = Tr;
						if( execbinary(">", execbinary("+", execbinary("*", Zr, Zr, System.Double, System.Double, TypeKind.Structure, TypeKind.Structure), execbinary("*", Zi, Zi, System.Double, System.Double, TypeKind.Structure, TypeKind.Structure), System.Double, System.Double, TypeKind.Structure, TypeKind.Structure), limit, System.Double, System.Double, TypeKind.Structure, TypeKind.Structure), 34_20_37_21 ){
							isInside = false;
							block{
							break;
							};
						};
					}while(execbinary(">", prefixoperator(true, i, execbinary("-", i, 1, null, null, null, null)), 0, System.Int32, System.Int32, TypeKind.Structure, TypeKind.Structure));
					if( isInside, 40_16_42_17 ){
						callinstance(this, Mandelbrot, "Output", execbinary("/", execbinary("*", x, 1.00000000, System.Single, System.Single, TypeKind.Structure, TypeKind.Structure), width, System.Single, System.Single, TypeKind.Structure, TypeKind.Structure), execbinary("/", execbinary("*", y, 1.00000000, System.Single, System.Single, TypeKind.Structure, TypeKind.Structure), height, System.Single, System.Single, TypeKind.Structure, TypeKind.Structure));
					};
				x = execbinary("+", x, 1, null, null, null, null);
				};
			y = execbinary("+", y, 1, null, null, null, null);
			};
		}options[needfuncinfo(false), rettype(return, System.Void, TypeKind.Unknown, 0, false)];
		Output = deffunc(0)args(this, x, y){
			callstatic(JsConsole, "Print", x, y);
		}options[needfuncinfo(false), rettype(return, System.Void, TypeKind.Unknown, 0, false), paramtype(x, System.Single, TypeKind.Structure, 0, true), paramtype(y, System.Single, TypeKind.Structure, 0, true)];
		ctor = deffunc(0)args(this){
			callinstance(this, Mandelbrot, "__ctor");
		};
		__ctor = deffunc(0)args(this){
			if(getinstance(SymbolKind.Field, this, Mandelbrot, "__ctor_called")){
				return();
			}else{
				setinstance(SymbolKind.Field, this, Mandelbrot, "__ctor_called", true);
			};
			setinstance(SymbolKind.Field, this, Mandelbrot, "datas", literalarray(System.Int32, TypeKind.Structure, 1, 2, 3, 4, 5, 6));
			setinstance(SymbolKind.Field, this, Mandelbrot, "dicts", newexterndictionary(System.Collections.Generic.Dictionary_TKey_TValue, "g_System_Collections_Generic_Dictionary_System_Int32_System_Int32", typeargs(System.Int32, System.Int32), typekinds(TypeKind.Structure, TypeKind.Structure), "ctor", 0, literaldictionary("g_System_Collections_Generic_Dictionary_System_Int32_System_Int32", typeargs(System.Int32, System.Int32), typekinds(TypeKind.Structure, TypeKind.Structure), 1 => 1, 2 => 2, 3 => 3, 4 => 4, 5 => 5)));
			setinstance(SymbolKind.Field, this, Mandelbrot, "dicts2", newexterndictionary(System.Collections.Generic.Dictionary_TKey_TValue, "g_System_Collections_Generic_Dictionary_System_Int32_System_Int32", typeargs(System.Int32, System.Int32), typekinds(TypeKind.Structure, TypeKind.Structure), "ctor__Int32", 0, literaldictionary("g_System_Collections_Generic_Dictionary_System_Int32_System_Int32", typeargs(System.Int32, System.Int32), typekinds(TypeKind.Structure, TypeKind.Structure)), 128));
		}options[needfuncinfo(false)];
	};
	instance_fields {
		r = 10;
		scale = 3.00000000;
		datas = null;
		dicts = null;
		dicts2 = null;
		__ctor_called = false;
	};
	instance_props {};
	instance_events {};

	interfaces {};

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
};




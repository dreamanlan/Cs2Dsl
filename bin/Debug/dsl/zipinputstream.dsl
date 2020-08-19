require("cs2dsl__lualib");
require("cs2dsl__namespaces");
require("cs2dsl__externenums");
require("cs2dsl__interfaces");

class(ZipInputStream) {
	static_methods {
		__new_object = function(...){
			return(newobject(ZipInputStream, typeargs(), typekinds(), "ctor", null, ...));
		};
		cctor = function(){
			callstatic(ZipInputStream, "__cctor");
		};
		__cctor = function(){
			if(ZipInputStream.__cctor_called){
				return;
			}else{
				ZipInputStream.__cctor_called = true;
			};
		};
	};
	static_fields {
		__cctor_called = false;
	};
	static_props {};
	static_events {};

	instance_methods {
		ctor = function(this, ms){
			callinstance(this, "__ctor");
			return(this);
		},
		__ctor = function(this){
			if(getinstance(this, "__ctor_called")){
				return;
			}else{
				setinstance(this, "__ctor_called", true);
			};
		};
	};
	instance_fields {
		__ctor_called = false;
	};
	instance_props {};
	instance_events {};

	interfaces {};
	interface_map {};

	class_info(TypeKind.Class, Accessibility.Internal) {
	};
	method_info {
		ctor(MethodKind.Constructor, Accessibility.Public){
		};
	};
	property_info {};
	event_info {};
	field_info {};
};




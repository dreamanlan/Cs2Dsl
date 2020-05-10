require("cs2js__lualib.js");
require("cs2js__namespaces.js");
require("cs2js__externenums.js");
require("cs2js__interfaces.js");

function Mandelbrot(){
	this.Exec = function(){
		var width;
		width = 50;
		var height;
		height = width;
		var maxiter;
		maxiter = 50;
		var limit;
		limit = 4.00;
		var y;
		y = 0;
		while(y < height){
			var Ci;
			Ci = 2.00 * y / height - 1.00;
			var x;
			x = 0;
			while(x < width){
				var Zr;
				Zr = 0.00;
				var Zi;
				Zi = 0.00;
				var Cr;
				Cr = 2.00 * x / width - 1.50;
				var i;
				i = maxiter;
				var isInside;
				isInside = true;
				do{
					var Tr;
					Tr = Zr * Zr - Zi * Zi + Cr;
					Zi = 2.00 * Zr * Zi + Ci;
					Zr = Tr;
					if(Zr * Zr + Zi * Zi > limit){
						isInside = false;
						break;
					};
				}while((function(){
					i = i - 1;
					return i;
				})() > 0) ;
				if(isInside){
					this.Output(x * 1.00 / width, y * 1.00 / height);
				};
				x = x + 1;
			};
			y = y + 1;
		};
	}
	this.Output = function(x, y){
		JsConsole.Print(x, y);
	}
	this.ctor = function(){
		this.__ctor();
	}
	this.__ctor = function(){
		if(this.__ctor_called){
			return;
		}else{
			this.__ctor_called = true;
		};
		this.datas = [1, 2, 3, 4, 5, 6];
		this.dicts = newexterndictionary(System.Collections.Generic.Dictionary_TKey_TValue, [System.Int32, System.Int32], [TypeKind.Struct, TypeKind.Struct], {1 : 1, 2 : 2, 3 : 3, 4 : 4, 5 : 5}, "System.Collections.Generic.Dictionary_TKey_TValue:ctor");
		this.dicts2 = newexterndictionary(System.Collections.Generic.Dictionary_TKey_TValue, [System.Int32, System.Int32], [TypeKind.Struct, TypeKind.Struct], {}, "System.Collections.Generic.Dictionary_TKey_TValue:ctor__Int32", 128);
	}
	this.r = 10;
	this.scale = 3.00;
	this.datas = null;
	this.dicts = null;
	this.dicts2 = null;
	this.__ctor_called = false;
}

(function(){
	Mandelbrot.__new_object = function(){
		return newobject(Mandelbrot, null, null, "ctor", null, getParams(0));
	}
	Mandelbrot.Test = function(){
		newobject(Mandelbrot, null, null, "ctor", null).Exec();
	}
	Mandelbrot.cctor = function(){
		Mandelbrot.__cctor();
	}
	Mandelbrot.__cctor = function(){
		if(Mandelbrot.__cctor_called){
			return;
		}else{
			Mandelbrot.__cctor_called = true;
		};
	}
	Mandelbrot.__cctor_called = false;
})()

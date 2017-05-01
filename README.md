# Cs2Dsl
CSharp代码转dsl，一个经由Cs2Lua来的实验项目，用于研究将C#语言转换为一个通用的中间语言，在此基础上能够比较方便的翻译为不同的目标语言。
中间语言使用我的另一个开源项目Dsl【https://github.com/dreamanlan/DSL】，是一种通用的领域特定语言及其解析器。

【命令行】

    Cs2Dsl [-enableinherit] [-enablelinq] [-outputresult] [-d macro] [-refbyname dllname alias] [-refbypath dllpath alias] [-src] csfile|csprojfile

其中:

    macro = 宏定义，会影响被转化的c#代码里的#if/#elif/#else/#endif语句的结果。

    dllname = 以名字（Assembly Name）提供的被引用的外部dotnet DLL，cs2dsl尝试从名字获取这些DLL的路径（一般只有dotnet系统提供的DLL才可以这么用）。

    dllpath = 以文件全路径提供的被引用的外部dotnet DLL。

    alias = 外部dll顶层名空间别名，默认为global, 别名在c#代码里由'extern alias 名字;'语句使用。
    
    enableinherit = 此选项指明是否允许继承。
    
 
    enablelinq = 此选项指明是否允许使用LINQ语法。

    outputresult = 此选项指明是否在控制台输出最终转化的结果（合并为单一文件样式）。

    src = 此选项仅用在refbyname/refbypath选项未指明alias参数的情形，此时需要此选项在csfile|csprojfile前明确表明后面的参数是输入文件。
    
Cs2Dsl的输出主要包括：

    1、对应c#代码的转换出的dsl代码，每个c#顶层类对应一个dsl文件。

    2、所有名字空间的定义dsl文件，此文件被1中文件引用，输出文件为cs2dsl_namespaces.dsl。

    3、Cs2Dsl依赖的dsllib文件utility.dsl，输出文件名为cs2dsl_utility.dsl。

    4、在c#代码里使用Cs2Dsl.Require明确指明要依赖的dsllib文件，这些文件需要自己放到Cs2Dsl.exe所在目录的子目录dsllib里，之后自动拷到输出目录。

【源由】

1、Cs2Lua采取基于c# Roslyn开源编译器的语法树直接翻译为lua的方式，由于c#语言的更新很快，在引入vs2017新语法的时候遇到了不少阻碍。同时考虑到也可能会有翻译为其它
目标语言的需求，定义一个适合转换的中间语言并将c#翻译为这个中间语言可能是一条相对便捷的路。

2、这个工程主要是实验目的，目前包括了一个简单的html javascript生成器用于测试。
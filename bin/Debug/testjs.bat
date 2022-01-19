rem working directory
set workdir=%~dp0

cd %workdir%
del /s/f/q dsl\*.dsl
del /s/f/q dsl\*.txt
del /s/f/q dsl\*.js
del /s/f/q lua\*.txt
Cs2Dsl -js -enableinherit -enablelinq -src test_js.cs



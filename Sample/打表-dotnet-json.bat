dotnet Luban/Bin/Luban.dll ^
-t client ^
--conf Luban/luban.conf ^
--customTemplateDir ../Templates ^
-x outputCodeDir=TestProject/Config/GenCode ^
-x pathValidator.rootDir=TestProject ^
-c cs-dotnet-json ^
-d json ^
-x outputDataDir=TestProject/Config/Json ^
-d const-cs ^
-x const-cs.outputDataDir=TestProject/Config/GenConst ^
--validationFailAsError
pause
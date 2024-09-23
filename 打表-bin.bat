dotnet Luban/Bin/Luban.dll ^
-t client ^
--conf Luban/luban.conf ^
--customTemplateDir Luban/Templates ^
-x outputCodeDir=../Assets/Scripts/Config/GenCode ^
-x pathValidator.rootDir=../Assets/Res ^
-c cs-bin ^
-d bin ^
-x outputDataDir=../Assets/Res/Config/Bin ^
-d const-cs ^
-x const-cs.outputDataDir=../Assets/Scripts/Config/GenConst ^
--validationFailAsError
pause
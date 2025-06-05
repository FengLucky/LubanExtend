dotnet Luban/Bin/Luban.dll \
-t client \
--conf Luban/luban.conf \
--customTemplateDir ../Templates \
-x outputCodeDir=TestProject/Config/GenCode \
-x pathValidator.rootDir=TestProject \
-c cs-bin \
-d bin \
-x outputDataDir=TestProject/Config/Bin \
-d const-cs \
-x const-cs.outputDataDir=TestProject/Config/GenConst \
--validationFailAsError
read -p "Press any key to continue..." 
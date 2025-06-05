@echo off

REM dotnet 版本要求
set required_dotnet_version=8
REM 构建目录
set build_dir="Build"
set luban_name="luban"

REM 更改代码页为 UTF-8
chcp 65001 >nul

REM 检测 dotnet 版本
REM 运行 dotnet --version 并捕获输出
for /f "delims=" %%i in ('dotnet --version') do set dotnet_version=%%i

REM 输出获取到的 .NET 版本
echo dotnet 版本: %dotnet_version%

REM 解析主版本号
for /f "tokens=1 delims=." %%j in ("%dotnet_version%") do set main_version=%%j

if %main_version% LSS %required_dotnet_version% (
    echo dotnet 版本小于 %required_dotnet_version%，请安装 %required_dotnet_version%或更新版本
    exit /b 1
) 

REM 检测 git 是否安装
where git >nul 2>nul
if %errorlevel% neq 0 (
	echo 未安装 git
	exit /b 1
)

if not exist %build_dir% (
	mkdir %build_dir%
	if %errorlevel% neq 0 (
		echo 创建 %build_dir% 目录失败
		exit /b 1
	)
)

if not exist "%build_dir%/%luban_name%" (
	echo 开始克隆 luban 项目
	cd %build_dir%
	git clone --depth 1 https://gitee.com/focus-creative-games/luban.git
	if not exist %luban_name% (
		echo 克隆 luban 项目失败
		exit /b 1
	)
	
	cd ../
	
) else (
    echo 开始更新项目
    cd %build_dir%
    git pull
    if %errorlevel% neq 0 (
		echo 更新 luban 项目失败
		exit /b 1
	)
	cd ../
)

xcopy "Module" "%build_dir%\luban\src" /E /I /Y
if %errorlevel% neq 0 (
    echo 复制 插件 项目失败，请清理 %build_dir% 目录后重试
    exit /b 1
)

cd %build_dir%\luban\src
dotnet sln add Luban.DataTarget.Const/Luban.DataTarget.Const.csproj

REM 添加扩展项目项目对LuBan.Core的引用
cd Luban.DataTarget.Const
dotnet add reference ../Luban.Core/Luban.Core.csproj
cd ../

REM 添加 Luban 对扩展项目的引用
cd Luban
dotnet add reference ../Luban.DataTarget.Const/Luban.DataTarget.Const.csproj
cd ../

REM 返回初始目录
cd ../../../

cd %build_dir%\luban\src
dotnet publish Luban/Luban.csproj -c Release -o ../../publish -p:DebugType=none
if %errorlevel% neq 0 (
	echo 编译 luban 项目失败
	exit /b 1
)

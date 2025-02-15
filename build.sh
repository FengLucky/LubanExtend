#!/bin/bash

# dotnet 版本要求
required_dotnet_version=8
# 构建目录
build_dir="Build"
luban_name="luban"

# 设置UTF-8环境
export LANG=C.UTF-8

# 检测 dotnet 版本
if ! command -v dotnet &> /dev/null; then
    echo "未安装 dotnet"
    exit 1
fi

dotnet_version=$(dotnet --version)
echo "dotnet 版本: $dotnet_version"

# 解析主版本号
main_version=$(echo "$dotnet_version" | cut -d '.' -f1)

if [ "$main_version" -lt "$required_dotnet_version" ]; then
    echo "dotnet 版本小于 $required_dotnet_version，请安装 $required_dotnet_version 或更新版本"
    exit 1
fi

# 检测 git 是否安装
if ! command -v git &> /dev/null; then
    echo "未安装 git"
    exit 1
fi

# 创建构建目录
mkdir -p "$build_dir" || {
    echo "创建 $build_dir 目录失败"
    exit 1
}

if [ ! -d "$build_dir/$luban_name" ]; then
    echo "开始克隆 luban 项目"
    (cd "$build_dir" && git clone --depth 1 https://gitee.com/focus-creative-games/luban.git) || {
        echo "克隆 luban 项目失败"
        exit 1
    }

    cp -r "Module/Luban.DataTarget.Const" "$build_dir/luban/src/Luban.DataTarget.Const" || {
        echo "复制插件项目失败，请清理 $build_dir 目录后重试"
        exit 1
    }

    (cd "$build_dir/luban/src" && \
    dotnet sln add Luban.DataTarget.Const/Luban.DataTarget.Const.csproj && \
    cd Luban.DataTarget.Const && \
    dotnet add reference ../Luban.Core/Luban.Core.csproj && \
    cd .. && \
    cd Luban && \
    dotnet add reference ../Luban.DataTarget.Const/Luban.DataTarget.Const.csproj) || {
        echo "添加项目引用失败"
        exit 1
    }
else
    echo "开始更新项目"
    (cd "$build_dir/luban" && git pull) || {
        echo "更新 luban 项目失败"
        exit 1
    }
fi

# 编译项目
(cd "$build_dir/luban/src" && \
dotnet publish Luban/Luban.csproj -c Release -o ../../publish/exe -r win-x64 -p:PublishSingleFile=true -p:DebugType=none --self-contained true && \
dotnet publish Luban/Luban.csproj -c Release -o ../../publish/dll -p:DebugType=none) || {
    echo "编译 luban 项目失败"
    exit 1
}

echo "构建完成"
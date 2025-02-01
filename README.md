# BundleWorkFlow
a simple but useful solution for using AssetBundle in Unity   

# Summary

# Installation
through Window/PackageManager, you can:   
- __add package from git URL__, which is https://github.com/Chendiancong/bundle-workflow.git
- clone this repository into your local path, and __add package from disk__

# design

## AssetBundle构建过程
- 指定一个根目录，在根目录中以文件夹为单位构建ab包
- 生成 __资源目录__ => __ab包__ 的映射
- 生成所有ab包的128位md5版本列表

## AssetBundle资源系统初始化

## AssetBundle加载与内存管理

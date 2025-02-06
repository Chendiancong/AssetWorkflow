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
- 针对打包的配置生成Setting文件

## AssetBundle资源系统初始化
- 从服务器下载Version，进行本地资源更新
- 从服务器下载AssetMap，记录文件映射
- 读取Setting，读取打包配置

## AssetBundle加载与内存管理
- 使用一个包装类来对已经加载的AssetBundle进行管理，记录它的加载状态和引用计数
- 使用一个包装类来对从AssetBundle加载的资源进行管理，记录它所引用的AssetBundle包装类、加载状态以及引用计数

## 优势或特点
- 通过文件目录的方式分配AssetBundle，相对于在资源中进行配置的方式更加便捷，而且天生具备解决资源重复引用的问题
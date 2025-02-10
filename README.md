# AssetWorkflow
AssetWorkow旨在为Unity开发者提供一个一站式资源管理框架，包括AssetBundle构建，类似**Addressables**的可寻址AssetBundle资源加载，运行时缓存管理，dlc及资源热更新等功能

## 安装
通过**Unity包管理器（Window/PackageManager）**安装:   
1. **add package from git url**，地址为git@github.com:Chendiancong/AssetWorkflow.git
2. clone仓库到本地，通过**add package from dist**进行安装
3. 也可以直接作为子模块添加到Assets目录下，这样在使用的同时也可以对他进行修改

## 使用
- 工具栏Asset Workflow->Operation中可以进行配置和操作资源包清理和构建

## AssetBundle构建过程
- 指定一个根目录，在根目录中以文件夹为单位构建ab包
- 生成 __资源目录__ => __ab包__ 的映射
- 生成所有ab包的128位md5版本列表
- 针对打包的配置生成Setting文件

## AssetBundle资源系统初始化
- 加载Setting文件，读取到打包配置
- 读取本地的Version文件
- 从服务器下载Version到内存中
- 比较文件的Version，下载新的资源（最好能够支持多线程下载，提高效率），持续写入Version到本地
- 从本地加载已经更新的AssetMap，记录文件映射
- AssetMgr初始化完成

## AssetBundle加载与内存管理
- 使用一个包装类来对已经加载的AssetBundle进行管理，记录它的加载状态和引用计数
- 使用一个包装类来对从AssetBundle加载的资源进行管理，记录它所引用的AssetBundle包装类、加载状态以及引用计数

## 优势或特点
- 通过文件目录的方式分配AssetBundle，相对于在资源中进行配置的方式更加便捷，而且天生具备解决资源重复引用的问题

## 存在问题
- [x] 因为Setting.json优先于热更新加载，而热更过程中有可能会更新一个新的Setting.json到本地，所以当前Setting有可能和服务器端的不同步，Setting首先需要提供资源服务器的url用于热更资源，保证这些信息正确的前提下，先尝试尝试重新加载Setting
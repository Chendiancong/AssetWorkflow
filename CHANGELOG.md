# [0.0.1] - 2024-01-15
- initial

# [0.0.2] - 2024-2-6
- 提供针对AssetBundle构建的配置和基本操作，可以通过Asset Workflow->Operation在工具窗口中进行
- 根据构建产生的版本文件进行热更新
- 提供了一个统一的加载接口，在内部处理好缓存、包依赖以及引用计数
- 不同平台，远程资源会下载到不同的位置，具体可以见**AssetMgrHepler.cs->AssetMgrHelper.AssetSavePath**
- ab包执行统一的命名规则，它们的原名称会经过md5进行128位哈希，并储存资源发布目录中以自身头两个字母命名的目录中
- 现在，在ab包构建目录及其子目录中右键选择Asset Folder Configure，可以打开一个窗口设定该目录的打包配置，当前可以选择打包的粒度
  - No 该目录不打包
  - Normal 该目录打包成一个assetbundle
  - Single 该目录下每一个文件都是一个单独的assetbundle
- 新增AssetRef类，支持在编辑器中对资源拖拽配置
- 新增AssetKeeper类，支持对资源的引用计数的自动增减
- 新增一些基于AssetRef工作流的元件
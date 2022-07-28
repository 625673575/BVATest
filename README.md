# TestDemo For BVA

这是BVA格式的测试Demo，Demo分为两部分。
\Assets\Scenes\Case1内有GLFT官方测试底层的能力覆盖测试。
\Assets\Scenes\Case2主要测试三个方向：
- Avatar类型模型加载
- Scene类型模型加载
- MultiScene混合模型加载

## Case1

Case1将模型内置，可以直接打包测试，适用于PC，Android，iOS三端

## Case2

Case2可以读取本地服务器上的模型文件进行加载，避免了模型文件内置需要多次打包的麻烦，仅需要将模型文件添加到本地服务器相应的文件夹内即可。

本地服务器需要有以下目录：
- BVAAvatarRuntimeLoad
- BVASceneRuntimeLoad
- MultiSceneRuntimeLoad

模型文件放到相应的文件夹内，通过编辑器扩展（[MenuItem("BVA Test/SaveJsonCase2")]）生成config.json文件，选择服务器目录Path即可。

## Example

首先选择一个目录作为本地服务器，例如：
> D:\TestServer

通过编辑器扩展"BVA Test/SaveJsonCase2"，选择该目录，点击SaveJson，就会在该目录下生成子文件夹和config.json初始文件。
将模型文件放到对应的文件夹中，再次点击SaveJson就能在config.json中保存到模型文件的具体路径信息。

这里使用的是Python3的http.server功能。确保机器已经安装Python3，并且设置了环境变量，然后输入指令（最后为端口号）
进入到D:\TestServer，执行以下命令行
> python -m http.server 7777

之后在Unity Runtime下输入 ip:端口号 点击Link就能连接到服务器获取文件信息（本地可以直接输入localhost:7777或127.0.0.1:7777，局域网内都能访问）
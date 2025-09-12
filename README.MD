# Sharp Loader

基于 JNI 实现，开发中...

---

## Example

> **当前仍在开发中，代码变动频繁，请注意！**

[示例模组](https://github.com/YELANDAOKONG/SharpLoader2-Example)

> 构建前请修改 `SharpLoader.Core.Java.JniVersion.Convention` 以适配目标平台。

### 环境变量

| 名称 | 示例                                                | 描述                |
| --- |---------------------------------------------------|-------------------|
| JVM | `jvm.dll `                                        | JVM.dll 路径        |
| GAME | `D:\Minecraft\`                                   | Minecraft 游戏主目录   |
| MAIN | `net.fabricmc.loader.impl.launch.knot.KnotClient` | 主类（**必须通过此变量设置**） 
| MAPPING | `YARN`                                            | JAVA 混淆表类型        |
| MAPPINGS | `D:\Downloads\yarn-1.21.8.zip`                    | 混淆文件路径 |

#### 其他环境变量 (Loader 侧)

| 名称                            | 示例  | 描述                 |
|-------------------------------|-----|--------------------|
| LOGGER_NOCOLORFUL | `1` | 存在且非空时禁用控制台日志的彩色输出 |
| MAPPINGS_EXPORT               | `D:\debug.json` | 存在且非空时将内存中的混淆表以 JSON 格式输出 |

#### 其他环境变量 (Agent 侧)

| 名称                            | 示例  | 描述               |
|-------------------------------|-----|------------------|
| LOGGER_CLASS_LOADED | `1` | 存在且非空时输出详细的类加载日志 |
| LOGGER_NO_CLASS_MODIFY_LOG               | `1` | 存在且非空时不输出类修改日志   |

### 启动游戏

1. 在 HMCL 等启动器中导出启动脚本；
2. 修改脚本并定义运行所需的环境变量；
3. **添加 SharpAgent 作为 Java Agent**；
4. 修改替换 JAVA.exe 为 SharpLoader.exe；
5. 将 `-cp ...` 修改为 `-Djava.class.path=...`（否则有解析BUG无法找到类）；
6. 找到 JVM 参数和程序参数的交界处（即主类后），添加 `--`（如 `net.fabricmc.loader.impl.launch.knot.KnotClient -- --username ...`）；
7. 启动脚本即可运行游戏；

### 模块开发

> 使用 “模块” 与原版 “模组” 进行区分。

模块主类是实现了 `IModule` 接口或继承了 `ModuleBase` 的类。

具体可参考 [示例模组](https://github.com/YELANDAOKONG/SharpLoader2-Example)。

### 模块文件

模块是一个拥有 `sharp.json` 文件的 ZIP 压缩包。

Sharp.JSON （使用大写驼峰）必须包含 `Id` 和 `Namespace`；

如果入口点未定义，则使用默认 `default.dll`；
如果主类未定义，则使用反射查找到的第一个实现了 `IModule` 的类。

```json
{
  "Id": "SharpLoaderExample",
  "Namespace": "SharpLoaderExample",
  
  "Version": {
    "Major": 1,
    "Minor": 0,
    "Patch": 0
  },
  
  "EntryPoint": "SharpLoaderExample.dll",
  "MainClass": "SharpLoaderExample.ModuleMain",
  
  "Title": "SharpLoader Example",
  "Description": "Sharp Loader Example Module",
  "Authors": [
    "You"
  ]
}
```

### 模块位置

模块放置在 `GAME` 环境变量（即游戏主目录）下的 `modules` 文件夹中。


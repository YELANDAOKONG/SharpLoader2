# Sharp Loader

基于 JNI 实现，开发中...

---

## Example

> 当前仍在开发中，代码变动频繁，请注意！

### 环境变量

| 名称 | 示例                                                | 描述                |
| --- |---------------------------------------------------|-------------------|
| JVM | `jvm.dll `                                        | JVM.dll 路径        |
| GAME | `D:\Minecraft\`                                   | Minecraft 游戏主目录   |
| MAIN | `net.fabricmc.loader.impl.launch.knot.KnotClient` | 主类（**必须通过此变量设置**） 
| MAPPING | `YARN`                                            | JAVA 混淆表类型        |
| MAPPINGS | `D:\Downloads\yarn-1.21.8.zip`                    | 混淆文件路径 |

### 启动游戏

1. 在 HMCL 等启动器中到处启动脚本；
2. 修改脚本并定义运行所需的环境变量；
3. **添加 SharpAgent 作为 Java Agent**；
4. 修改替换 JAVA.exe 为 SharpLoader.exe；
5. 将 `-cp ...` 修改为 `-Djava.class.path=...`（否则有解析BUG无法找到类）；
6. 找到 JVM 参数和程序参数的交界处（即主类后），添加 `--`（如 `net.fabricmc.loader.impl.launch.knot.KnotClient -- --username ...`）；
7. 启动脚本即可运行游戏；




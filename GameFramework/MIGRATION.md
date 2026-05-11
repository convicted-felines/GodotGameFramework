# GameFramework → Godot 迁移进度

> 源仓库：https://github.com/EllanJiang/GameFramework  
> 目标引擎：Godot 4.6（C# / net8.0）  
> 最后更新：2026-05-11

---

## 项目结构

```
framework/
├── Framework.csproj          # Godot 游戏项目（Godot.NET.Sdk）
├── Framework.sln             # 解决方案（含两个 csproj）
├── NewScript.cs              # 示例 Godot Node 脚本
│
├── GameFramework/            # 核心层：纯 C# 类库（引擎无关）
│   └── GameFramework.csproj  # Microsoft.NET.Sdk / net8.0
│
└── GodotGameFramework/       # 适配层：Godot 具体实现（待开发）
```

### 关键配置说明

- `GameFramework.csproj` 使用 `Microsoft.NET.Sdk`，不依赖 Godot，可独立编译
- `Framework.csproj` 通过 `<ProjectReference>` 引用 GameFramework，同时用 `<Compile Remove="GameFramework\**">` 防止 Godot SDK 重复 glob 源文件
- `GenerateAssemblyInfo=false` 避免与原 `Properties/AssemblyInfo.cs` 冲突

---

## 已完成

### 基础设施

- [x] 项目目录结构创建（`GameFramework/`、`GodotGameFramework/`）
- [x] 通过 `git sparse-checkout` 从原仓库完整拉取所有模块源码
- [x] `GameFramework.csproj` 升级为 SDK 风格（net8.0，`AllowUnsafeBlocks`）
- [x] `Framework.csproj` 添加 ProjectReference，排除子目录重复编译
- [x] `Framework.sln` 加入 GameFramework 项目，配置所有 Build 配置
- [x] **两个项目编译全部通过（0 错误）**

### 已迁移模块（直接可用，零改动）

| 模块 | 路径 | 说明 |
|------|------|------|
| **Base** | `GameFramework/Base/` | 框架核心：入口、异常、链表、多字典、序列化器 |
| **Base/EventPool** | `GameFramework/Base/EventPool/` | 内部通用事件池（与 Event 模块的游戏事件池不同） |
| **Base/ReferencePool** | `GameFramework/Base/ReferencePool/` | 引用池，减少 GC |
| **Base/TaskPool** | `GameFramework/Base/TaskPool/` | 任务池，支持优先级排队 |
| **Base/DataProvider** | `GameFramework/Base/DataProvider/` | 数据提供者基础（Config/DataTable 依赖） |
| **Base/Variable** | `GameFramework/Base/Variable/` | 泛型变量包装（DataNode 使用） |
| **Base/Log** | `GameFramework/Base/Log/` | 日志接口，需注入 `ILogHelper` 实现 |
| **Event** | `GameFramework/Event/` | 游戏事件管理器，订阅/发布模式 |
| **Fsm** | `GameFramework/Fsm/` | 有限状态机，完全引擎无关 |
| **Procedure** | `GameFramework/Procedure/` | 流程管理（基于 FSM），游戏生命周期编排 |
| **ObjectPool** | `GameFramework/ObjectPool/` | 对象池管理器，支持单/多 Spawn |
| **Config** | `GameFramework/Config/` | 全局只读配置（需 `IConfigHelper` 实现加载） |
| **DataNode** | `GameFramework/DataNode/` | 树形数据节点，运行时数据存储 |
| **DataTable** | `GameFramework/DataTable/` | 数据表（需 `IDataTableHelper` 实现解析） |
| **Network** | `GameFramework/Network/` | TCP 网络（IPv4/IPv6），心跳，粘包处理 |
| **Download** | `GameFramework/Download/` | 下载管理，断点续传，速度统计 |
| **WebRequest** | `GameFramework/WebRequest/` | HTTP GET/POST 短连接请求 |
| **FileSystem** | `GameFramework/FileSystem/` | 虚拟文件系统（需 `IFileSystemHelper` 实现） |
| **Utility** | `GameFramework/Utility/` | 工具集：Assembly、Converter、Json、Marshal、Path、Random、Text、Verifier、Compression、Encryption |

---

## 适配层进度（`GodotGameFramework/`）

### 已完成（第一优先级 + 核心 Helpers）

| 文件 | 说明 |
|------|------|
| `Base/ShutdownType.cs` | 关闭类型枚举（None / Restart / Quit） |
| `Base/GameFrameworkComponent.cs` | 所有框架 Node 的抽象基类，对应 `MonoBehaviour`，在 `_Ready` 注册到 `GameEntry` |
| `Base/GameEntry.cs` | 静态服务定位器，管理所有 `GameFrameworkComponent` |
| `Base/BaseComponent.cs` | 框架核心 Node：初始化所有 Helper，驱动 `GameFrameworkEntry.Update` / `Shutdown` |
| `Utility/GodotLogHelper.cs` | `ILogHelper` → `GD.Print` / `GD.PushWarning` / `GD.PushError` |
| `Utility/DefaultJsonHelper.cs` | `IJsonHelper` → `System.Text.Json` |
| `Utility/DefaultCompressionHelper.cs` | `ICompressionHelper` → `System.IO.Compression.DeflateStream` |
| `Utility/DefaultTextHelper.cs` | `ITextHelper` → `string.Format` |
| `Event/EventComponent.cs` | 封装 `IEventManager`，提供 Subscribe / Fire / FireNow |
| `Procedure/ProcedureComponent.cs` | 封装 `IProcedureManager`，通过 `[Export]` 配置流程列表和入口流程 |

### 使用方式

在 Godot 场景中创建如下 Node 树（顺序即 `_Ready` 执行顺序）：

```
GameFramework (Node)
├── BaseComponent      # 必须最先初始化
├── EventComponent
└── ProcedureComponent # 最后调用 StartProcedures()
```

`BaseComponent` 的 `_Process` 会自动调用 `GameFrameworkEntry.Update`，无需手动驱动。

---

## 待完成

### 适配层开发（`GodotGameFramework/`）

每个模块需要在 `GodotGameFramework/` 下实现对应的 Helper/Bridge 类，将 GameFramework 的接口绑定到 Godot API。

#### 第一优先级（框架可运行的最低要求）

| 任务 | 接口 | Godot 对应 API |
|------|------|---------------|
| 日志适配 | `ILogHelper` | `GD.Print` / `GD.PushError` |
| 框架入口挂载 | `GameFrameworkEntry` | 继承 `Node`，在 `_Process` 中调用 `Update` |
| 流程启动 | `ProcedureManager` + 首个 `ProcedureBase` | 配合入口 Node 初始化 |

#### 第二优先级（数据驱动）

| 任务 | 接口 | Godot 对应 API / 说明 |
|------|------|----------------------|
| Config 加载 | `IConfigHelper` | 解析 `.json` / `.csv`，用 `FileAccess` 读取 |
| DataTable 解析 | `IDataTableHelper` | 同上，按行解析为 `IDataRow` |
| Setting 持久化 | `ISettingHelper` | `ConfigFile` 存取（替代 Unity `PlayerPrefs`） |
| FileSystem 流 | `IFileSystemHelper` | `FileAccess` 实现 `FileSystemStream` |

#### 第三优先级（资源与场景）

| 任务 | 接口 | Godot 对应 API |
|------|------|---------------|
| 资源加载 | `IResourceManager` / `ILoadResourceAgentHelper` | `ResourceLoader.Load` / `ResourceLoader.LoadThreadedRequest` |
| 场景管理 | `ISceneManager` 适配 | `SceneTree.ChangeSceneToFile` / `SceneTree.ChangeSceneToPacked` |
| 实体管理 | `IEntityHelper` / `IEntityGroupHelper` | `PackedScene.Instantiate`，挂到 `Node` 树 |

#### 第四优先级（音频与 UI）

| 任务 | 接口 | Godot 对应 API |
|------|------|---------------|
| 音频播放 | `ISoundHelper` / `ISoundAgentHelper` | `AudioStreamPlayer` / `AudioStreamPlayer3D` |
| UI 管理 | `IUIFormHelper` / `IUIGroupHelper` | `Control` 节点，`CanvasLayer` 分层 |
| 本地化 | `ILocalizationHelper` | 解析语言表，替换 `Label.Text` |

#### 第五优先级（调试与工具）

| 任务 | 说明 |
|------|------|
| Debugger 窗口 | 用 Godot `Control` 实现 `IDebuggerWindow`，在编辑器内叠层显示 |
| 网络 Helper | 实现 `INetworkChannelHelper`（粘包协议、心跳间隔按项目定） |
| 下载 Helper | 实现 `IDownloadAgentHelper`（可用 `HttpClient` 或 Godot `HTTPRequest`） |
| WebRequest Helper | 实现 `IWebRequestAgentHelper`（推荐 `HttpClient`） |
| Json Helper | 实现 `Utility.IJsonHelper`（推荐 `System.Text.Json`） |
| 压缩 Helper | 实现 `Utility.ICompressionHelper`（推荐 `System.IO.Compression`） |

---

## 架构说明

```
┌─────────────────────────────────────────────┐
│              Godot 游戏逻辑层                 │
│         (Framework.csproj / Node 脚本)        │
└────────────────┬────────────────────────────┘
				 │ 调用
┌────────────────▼────────────────────────────┐
│          GodotGameFramework 适配层            │  ← 待开发
│   实现各 IXxxHelper 接口，绑定 Godot API      │
└────────────────┬────────────────────────────┘
				 │ 实现接口
┌────────────────▼────────────────────────────┐
│         GameFramework 核心层                  │  ← 已完成
│    纯 C# 逻辑，无任何引擎依赖，0 编译错误      │
└─────────────────────────────────────────────┘
```

### 重要说明

- **GameFramework 核心层不需要修改**，所有 Godot 适配通过实现接口完成
- `Resource` 模块的接口（`IResourceManager`）已存在于核心层，但 **`ResourceManager` 的实现层（Asset Bundle 体系）不适用于 Godot**，适配层应直接实现 `IResourceManager` 接口而非继承 `ResourceManager`
- 同理，`Scene`、`Sound`、`UI`、`Entity` 的 Manager 实现也建议直接实现接口，不继承原有 Manager

# GameFramework → Godot 迁移进度

> 源仓库：https://github.com/EllanJiang/GameFramework  
> 目标引擎：Godot 4.6（C# / net8.0）  
> 最后更新：2026-05-15

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
├── GodotGameFramework/       # 适配层：Godot 具体实现
│   ├── Base/                 # 框架入口、组件基类
│   ├── DataTable/            # DataTable 组件 + 默认 Helper
│   ├── Entity/               # 实体组件 + Helper
│   ├── Event/                # 事件组件
│   ├── Procedure/            # 流程组件
│   ├── UI/                   # UI 组件 + Helper
│   └── Utility/              # Log / Json / Compression / Text Helper
│
└── Tools/
	└── DataTableGenerator/   # 独立命令行工具（.NET 8 控制台程序）
		├── DataTableGenerator.csproj
		├── DataTableGenerator.cs    # Excel→TSV / TSV→bytes / TSV→C# 生成逻辑
		├── DataTableProcessor.cs    # TSV 解析核心
		├── DataTableProcessor.*.cs  # 各类型处理器（id/string/int/bool/float…）
		└── Program.cs               # 命令行入口
```

### 关键配置说明

- `GameFramework.csproj` 使用 `Microsoft.NET.Sdk`，不依赖 Godot，可独立编译
- `Framework.csproj` 通过 `<ProjectReference>` 引用 GameFramework，同时用 `<Compile Remove="GameFramework\**">` 和 `<Compile Remove="Tools\**">` 防止 Godot SDK 重复 glob 源文件
- `GenerateAssemblyInfo=false` 避免与原 `Properties/AssemblyInfo.cs` 冲突
- `Tools/DataTableGenerator` 是独立的 `Microsoft.NET.Sdk` 控制台项目，依赖 `DocumentFormat.OpenXml` NuGet 包，与游戏项目完全隔离

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
| `Entity/EntityLogic.cs` | 实体逻辑基类（继承 Node + IEntity），用户实体脚本继承此类 |
| `Entity/EntityGroupHelper.cs` | 实体组容器节点，每组独立挂载在 EntityComponent 下 |
| `Entity/EntityHelper.cs` | PackedScene 实例化/创建/释放，将 Node 接入对象池管理 |
| `Entity/EntityComponent.cs` | 封装 `IEntityManager`，[Export] 配置实体组，透传显示/隐藏/父子操作 |
| `UI/UIFormLogic.cs` | UI 界面逻辑基类（继承 Control + IUIForm），根节点脚本，支持编辑器拖拽引用 |
| `UI/UIGroupHelper.cs` | UI 组容器节点（CanvasLayer），`SetDepth` 映射到 `Layer` 属性控制渲染层级 |
| `UI/UIFormHelper.cs` | PackedScene 实例化/创建/释放，强制要求根节点为 UIFormLogic（不做递归查找） |
| `UI/UIComponent.cs` | 封装 `IUIManager`，[Export] 配置 UI 组及对象池参数，透传打开/关闭/激活操作 |
| `DataTable/DataTableComponent.cs` | 封装 `IDataTableManager`，提供 `LoadDataTable<T>(path)` 直接从文件加载并解析 |
| `DataTable/DefaultDataProviderHelper.cs` | 实现 `IDataProviderHelper<DataTableBase>`，支持二进制流和 TSV 字符串两种解析方式 |
| `DataTable/DefaultDataTableHelper.cs` | `IDataTableHelper` 标记接口实现 |
| `Resource/GodotResourceManager.cs` | 直接实现 `IResourceManager`，PackageMode（`res://`）+ UpdatableMode（`user://` + `.pck` 热更） |
| `Resource/GodotLoadResourceAgentHelper.cs` | `ILoadResourceAgentHelper`，驱动 `LoadThreadedRequest` 异步流水线，含逐帧轮询 |
| `Resource/GodotResourceHelper.cs` | `IResourceHelper`，`FileAccess` 读字节 + additive 场景卸载 + 资源释放 |
| `Resource/GodotResourceGroup.cs` | `IResourceGroup` 元数据容器（Godot 资源始终就绪） |
| `Resource/GodotResourceGroupCollection.cs` | `IResourceGroupCollection` 聚合视图 |
| `Resource/ResourceComponent.cs` | 封装 `GodotResourceManager`，`[Export]` 配置模式/Agent 数/路径，`_Process` 驱动异步轮询 |
| `Config/DefaultConfigHelper.cs` | `IConfigHelper` + `IDataProviderHelper<IConfigManager>`，支持 TSV（`Key\tValue`）和 JSON 扁平对象两种格式 |
| `Config/ConfigComponent.cs` | 封装 `IConfigManager`，`LoadConfig(path)` 同步读取 `res://`/`user://` 配置文件 |
| `Setting/DefaultSettingHelper.cs` | `ISettingHelper`，使用 Godot `ConfigFile` 持久化到 `user://settings.cfg`，对象值 JSON 序列化 |
| `Setting/SettingComponent.cs` | 封装 `ISettingManager`，`_Ready` 自动加载，`_ExitTree` 自动保存，`[Export]` 可配置路径 |
| `FileSystem/GodotFileSystemStream.cs` | 继承 `FileSystemStream`，将 `user://`/`res://` 解析为绝对路径后用 `System.IO.FileStream` 操作 |
| `FileSystem/GodotFileSystemHelper.cs` | `IFileSystemHelper`，工厂方法创建 `GodotFileSystemStream` |
| `FileSystem/FileSystemComponent.cs` | 封装 `IFileSystemManager`，管理虚拟文件系统包（`.vfs`）的创建/加载/销毁 |

### 使用方式

在 Godot 场景中创建如下 Node 树（顺序即 `_Ready` 执行顺序）：

```
GameFramework (Node)
├── BaseComponent       # 必须最先初始化
├── EventComponent
├── ResourceComponent   # 资源组件（Entity/UI 组件依赖，需在它们之前）
├── ConfigComponent     # 全局只读配置（TSV / JSON）
├── SettingComponent    # 玩家持久化设置（user://settings.cfg）
├── FileSystemComponent # 虚拟文件系统包管理
├── DataTableComponent  # 数据表组件
└── ProcedureComponent  # 最后调用 StartProcedures()
```

`BaseComponent` 的 `_Process` 会自动调用 `GameFrameworkEntry.Update`，无需手动驱动。

**DataTable 加载示例：**

```csharp
// 在 Procedure 或其他组件中
var dt = GameEntry.GetComponent<DataTableComponent>();

// 从二进制文件加载（推荐，运行时性能最优）
var heroTable = dt.LoadDataTable<DRHero>("res://DataTables/Bytes/Hero.bytes");

// 按 Id 读取
var hero = heroTable[1001];
GD.Print(hero.Name, hero.Level);
```

**Config 加载示例：**

```csharp
var cfg = GameEntry.GetComponent<ConfigComponent>();

// TSV 格式（res://Configs/game.tsv）：
//   # 注释行
//   MaxLevel\t100
//   ServerAddr\t127.0.0.1
// JSON 格式（res://Configs/game.json）：
//   {"MaxLevel": 100, "ServerAddr": "127.0.0.1"}
cfg.LoadConfig("res://Configs/game.json");

int maxLevel = cfg.GetInt("MaxLevel");
string addr  = cfg.GetString("ServerAddr", "localhost");
```

**Setting 持久化示例：**

```csharp
var setting = GameEntry.GetComponent<SettingComponent>();

// 写入（自动在 _ExitTree 保存；也可手动调用 Save）
setting.SetInt("MusicVolume", 80);
setting.SetBool("Fullscreen", true);
setting.SetObject("PlayerProfile", new PlayerProfile { Name = "Hero" });

// 读取（带默认值）
int vol        = setting.GetInt("MusicVolume", 100);
bool fullscreen = setting.GetBool("Fullscreen", false);
var profile    = setting.GetObject<PlayerProfile>("PlayerProfile");
```

**FileSystem 虚拟包示例：**

```csharp
var fs = GameEntry.GetComponent<FileSystemComponent>();

// 创建新包（写入模式）
IFileSystem vfs = fs.CreateFileSystem("user://data.vfs",
	FileSystemAccess.ReadWrite, maxFileCount: 128, maxBlockCount: 256);

// 写入文件
byte[] data = System.Text.Encoding.UTF8.GetBytes("Hello VFS");
vfs.WriteFile("greeting.txt", data);

// 销毁（关闭句柄，保留物理文件）
fs.DestroyFileSystem(vfs);

// 加载已有包（只读）
IFileSystem loaded = fs.LoadFileSystem("user://data.vfs", FileSystemAccess.Read);
byte[] result = loaded.ReadFile("greeting.txt");
```

**Resource 加载示例：**

```csharp
// PackedScene 异步加载
var res = GameEntry.GetComponent<ResourceComponent>();
res.LoadAsset("res://Prefabs/Hero.tscn",
	new LoadAssetCallbacks(
		onSuccess: (name, asset, duration, userData) => {
			var scene = (PackedScene)asset;
			AddChild(scene.Instantiate());
		},
		onFailure: (name, status, msg, userData) => {
			GD.PrintErr($"Load failed: {msg}");
		}
	)
);

// 二进制文件异步加载
res.LoadBinary("res://Data/config.bytes",
	new LoadBinaryCallbacks(
		(name, bytes, duration, userData) => { /* 处理 bytes */ }
	)
);

// PackageMode 初始化（自动，无需手动调用）
// UpdatableMode 热更示例：
res.ApplyResources("user://patch_v2.pck",
	(packPath, success) => GD.Print($"Patch applied: {success}")
);
```

### DataTableGenerator 工具使用方式

```bash
# 进入工具目录并构建
cd Tools/DataTableGenerator
dotnet build

# 一键：Excel → TSV → .bytes + C# 代码
dotnet run -- all GameData.xlsx \
  --text   Assets/DataTables/Text   \
  --bytes  Assets/DataTables/Bytes  \
  --code   Assets/Scripts/DataTable \
  --namespace MyGame

# 分步执行
dotnet run -- excel GameData.xlsx --text DataTables/Text  # 导出 Excel Sheet 为 TSV
dotnet run -- bytes                                        # TSV → .bytes
dotnet run -- code  --namespace MyGame                     # TSV → C# 代码
```

**TSV 文件格式（行索引从 0 开始）：**

```
# 第0行：列名（首列固定为注释列）
# 第1行：列类型（id / int / string / bool / float / long / ...）
# 第2行：默认值（可留空）
# 第3行：列说明注释
# 第4行起：数据行（首列以 # 开头的行为注释行，跳过）

#注释   Id     Name    Level   Attack
		id     string  int     float
							   0
		编号   名称    等级    攻击力
1001    英雄1  15      25.5
1002    英雄2  20      30.0
```

---

## 待完成

### 适配层开发（`GodotGameFramework/`）

每个模块需要在 `GodotGameFramework/` 下实现对应的 Helper/Bridge 类，将 GameFramework 的接口绑定到 Godot API。

#### 第一优先级（框架可运行的最低要求）✅ 已全部完成

#### 第二优先级（数据驱动）

| 任务 | 接口 | Godot 对应 API / 说明 | 状态 |
|------|------|----------------------|------|
| ~~DataTable 组件~~ | ~~`IDataTableHelper` / `IDataProviderHelper`~~ | ✅ 已完成：`DataTableComponent` / `DefaultDataProviderHelper` | ✅ |
| ~~DataTable 生成工具~~ | — | ✅ 已完成：`Tools/DataTableGenerator`（Excel→TSV→bytes+C#） | ✅ |
| ~~Config 加载~~ | ~~`IConfigHelper`~~ | ✅ 已完成：`ConfigComponent` / `DefaultConfigHelper`（JSON+TSV） | ✅ |
| ~~Setting 持久化~~ | ~~`ISettingHelper`~~ | ✅ 已完成：`SettingComponent` / `DefaultSettingHelper`（Godot `ConfigFile`） | ✅ |
| ~~FileSystem 流~~ | ~~`IFileSystemHelper`~~ | ✅ 已完成：`FileSystemComponent` / `GodotFileSystemHelper` / `GodotFileSystemStream` | ✅ |

#### 第三优先级（资源与场景）

| 任务 | 接口 | Godot 对应 API | 状态 |
|------|------|---------------|------|
| 资源加载 | `IResourceManager` / `ILoadResourceAgentHelper` | `ResourceLoader.Load` / `ResourceLoader.LoadThreadedRequest` | ✅ |
| 场景管理 | `ISceneManager` 适配 | `SceneTree.ChangeSceneToFile` / `SceneTree.ChangeSceneToPacked` | ⬜ |
| ~~实体管理~~ | ~~`IEntityHelper` / `IEntityGroupHelper`~~ | ✅ 已完成：`EntityComponent` / `EntityHelper` / `EntityLogic` | ✅ |

#### 第四优先级（音频与 UI）

| 任务 | 接口 | Godot 对应 API | 状态 |
|------|------|---------------|------|
| 音频播放 | `ISoundHelper` / `ISoundAgentHelper` | `AudioStreamPlayer` / `AudioStreamPlayer3D` | ⬜ |
| ~~UI 管理~~ | ~~`IUIFormHelper` / `IUIGroupHelper`~~ | ✅ 已完成：`UIComponent` / `UIFormHelper` / `UIFormLogic` | ✅ |
| 本地化 | `ILocalizationHelper` | 解析语言表，替换 `Label.Text` | ⬜ |

#### 第五优先级（调试与网络）

| 任务 | 说明 | 状态 |
|------|------|------|
| Debugger 窗口 | 用 Godot `Control` 实现 `IDebuggerWindow`，在编辑器内叠层显示 | ⬜ |
| 网络 Helper | 实现 `INetworkChannelHelper`（粘包协议、心跳间隔按项目定） | ⬜ |
| 下载 Helper | 实现 `IDownloadAgentHelper`（可用 `HttpClient` 或 Godot `HTTPRequest`） | ⬜ |
| WebRequest Helper | 实现 `IWebRequestAgentHelper`（推荐 `HttpClient`） | ⬜ |

---

## 架构说明

```
┌─────────────────────────────────────────────┐
│              Godot 游戏逻辑层                 │
│         (Framework.csproj / Node 脚本)        │
└────────────────┬────────────────────────────┘
				 │ 调用
┌────────────────▼────────────────────────────┐
│          GodotGameFramework 适配层            │  ← 持续开发中
│   实现各 IXxxHelper 接口，绑定 Godot API      │
│   DataTable / Entity / UI / Event 已完成     │
└────────────────┬────────────────────────────┘
				 │ 实现接口
┌────────────────▼────────────────────────────┐
│         GameFramework 核心层                  │  ← 已完成（0 修改）
│    纯 C# 逻辑，无任何引擎依赖，0 编译错误      │
└─────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│         Tools/DataTableGenerator             │  ← 独立命令行工具
│  Excel(.xlsx) → TSV → .bytes + C# IDataRow  │
│  依赖 DocumentFormat.OpenXml，与游戏项目隔离  │
└─────────────────────────────────────────────┘
```

### 重要说明

- **GameFramework 核心层不需要修改**，所有 Godot 适配通过实现接口完成
- `Resource` 模块的接口（`IResourceManager`）已存在于核心层，但 **`ResourceManager` 的实现层（Asset Bundle 体系）不适用于 Godot**，适配层应直接实现 `IResourceManager` 接口而非继承 `ResourceManager`
- 同理，`Scene`、`Sound`、`UI`、`Entity` 的 Manager 实现也建议直接实现接口，不继承原有 Manager

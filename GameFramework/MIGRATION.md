# GameFramework → Godot 迁移进度

> 源仓库：https://github.com/EllanJiang/GameFramework  
> 目标引擎：Godot 4.6（C# / net8.0）  
> 最后更新：2026-05-25

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
│   ├── Base/                 # 框架入口、组件基类、枚举
│   ├── Config/               # Config 组件 + Helper
│   ├── DataTable/            # DataTable 组件 + Helper
│   ├── Download/             # Download 组件 + Helper
│   ├── Entity/               # 实体组件 + Helper + Config
│   ├── Event/                # 事件组件
│   ├── FileSystem/           # 虚拟文件系统组件 + Helper
│   ├── Localization/         # 本地化组件 + Helper
│   ├── Network/              # 网络组件 + Helper
│   ├── ObjectPool/           # 对象池组件
│   ├── Procedure/            # 流程组件
│   ├── ReferencePool/        # 引用池组件
│   ├── Resource/             # 资源组件 + Helper
│   ├── Scene/                # 场景组件
│   ├── Setting/              # Setting 组件 + Helper
│   ├── Sound/                # 音频组件 + Helper
│   ├── UI/                   # UI 组件 + Helper
│   ├── Utility/              # Log / Json / Compression / Text Helper
│   ├── Variable/             # 变量类型包装（Var*）
│   └── WebRequest/           # WebRequest 组件 + Helper
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

### ✅ 已全部完成

#### 基础组件

| 文件 | 说明 |
|------|------|
| `Base/ShutdownType.cs` | 关闭类型枚举（None / Restart / Quit） |
| `Base/HelperTypeEnums.cs` | 各 Helper 类型选择枚举（Log / Json / Compression / Text 等），用于 Inspector 下拉配置 |
| `Base/GameFrameworkComponent.cs` | 所有框架 Node 的抽象基类，对应 `MonoBehaviour`，在 `_Ready` 注册到 `GameEntry` |
| `Base/GameEntry.cs` | 静态服务定位器，管理所有 `GameFrameworkComponent` |
| `Base/BaseComponent.cs` | 框架核心 Node：通过枚举下拉选择并实例化各 Helper，驱动 `GameFrameworkEntry.Update` / `Shutdown` |
| `Utility/Log.cs` | 日志统一入口，封装 `GameFramework.Log`，避免命名空间冲突 |
| `Utility/GodotLogHelper.cs` | `ILogHelper` → `GD.Print` / `GD.PushWarning` / `GD.PushError` |
| `Utility/DefaultJsonHelper.cs` | `IJsonHelper` → `System.Text.Json` |
| `Utility/DefaultCompressionHelper.cs` | `ICompressionHelper` → `System.IO.Compression.DeflateStream` |
| `Utility/DefaultTextHelper.cs` | `ITextHelper` → `string.Format` |
| `Utility/LogHelperBase.cs` | `ILogHelper` 抽象基类（用于自定义扩展） |
| `Utility/JsonHelperBase.cs` | `IJsonHelper` 抽象基类 |
| `Utility/CompressionHelperBase.cs` | `ICompressionHelper` 抽象基类 |
| `Utility/TextHelperBase.cs` | `ITextHelper` 抽象基类 |

#### 变量类型（Variable）

| 说明 |
|------|
| `Variable/Var*.cs`（14 个）— 包装所有基础类型：Boolean / Byte / ByteArray / Char / CharArray / DateTime / Decimal / Double / Int16 / Int32 / Int64 / SByte / Single / String / UInt16 / UInt32 / UInt64 / Object |

#### 引用池组件

| 文件 | 说明 |
|------|------|
| `ReferencePool/ReferenceStrictCheckType.cs` | 严格检查类型枚举 |
| `ReferencePool/ReferencePoolComponent.cs` | 封装 `ReferencePool`，`[Export]` 配置严格检查模式 |

#### 对象池组件

| 文件 | 说明 |
|------|------|
| `ObjectPool/ObjectPoolComponent.cs` | 封装 `IObjectPoolManager`，`[Export]` 配置容量/过期时间/检查间隔 |

#### 事件 / 流程组件

| 文件 | 说明 |
|------|------|
| `Event/EventComponent.cs` | 封装 `IEventManager`，提供 Subscribe / Fire / FireNow |
| `Procedure/ProcedureComponent.cs` | 封装 `IProcedureManager`，通过 `[Export]` 配置流程列表和入口流程 |

#### 实体组件

| 文件 | 说明 |
|------|------|
| `Entity/EntityLogic.cs` | 实体逻辑基类（继承 Node + IEntity），用户实体脚本继承此类 |
| `Entity/EntityGroupConfig.cs` | 实体组配置数据类（组名、Helper 类型、对象池参数等） |
| `Entity/EntityHelperBase.cs` | `IEntityHelper` 抽象基类 |
| `Entity/EntityGroupHelperBase.cs` | `IEntityGroupHelper` 抽象基类 |
| `Entity/EntityHelper.cs` | PackedScene 实例化/创建/释放，将 Node 接入对象池管理 |
| `Entity/EntityGroupHelper.cs` | 实体组容器节点，每组独立挂载在 EntityComponent 下 |
| `Entity/EntityComponent.cs` | 封装 `IEntityManager`，`[Export]` 配置实体组，透传显示/隐藏/父子操作 |

#### UI 组件

| 文件 | 说明 |
|------|------|
| `UI/UIFormLogic.cs` | UI 界面逻辑基类（继承 Control + IUIForm），根节点脚本 |
| `UI/UIFormHelperBase.cs` | `IUIFormHelper` 抽象基类 |
| `UI/UIGroupHelperBase.cs` | `IUIGroupHelper` 抽象基类 |
| `UI/UIFormHelper.cs` | PackedScene 实例化/创建/释放，强制根节点为 UIFormLogic |
| `UI/UIGroupHelper.cs` | UI 组容器节点（CanvasLayer），`SetDepth` 映射到 `Layer` 属性 |
| `UI/UIComponent.cs` | 封装 `IUIManager`，`[Export]` 配置 UI 组及对象池参数 |

#### 音频组件

| 文件 | 说明 |
|------|------|
| `Sound/SoundHelperBase.cs` | `ISoundHelper` 抽象基类 |
| `Sound/SoundGroupHelperBase.cs` | `ISoundGroupHelper` 抽象基类 |
| `Sound/SoundAgentHelperBase.cs` | `ISoundAgentHelper` 抽象基类 |
| `Sound/SoundHelper.cs` | 默认音频 Helper |
| `Sound/SoundGroupHelper.cs` | 音频组容器节点 |
| `Sound/SoundAgentHelper.cs` | `AudioStreamPlayer` + Tween 淡入淡出 |
| `Sound/SoundComponent.cs` | 封装 `ISoundManager`，`[Export]` 配置音频组和 Agent 数量 |

#### 资源组件

| 文件 | 说明 |
|------|------|
| `Resource/ResourceHelperBase.cs` | `IResourceHelper` 抽象基类 |
| `Resource/LoadResourceAgentHelperBase.cs` | `ILoadResourceAgentHelper` 抽象基类 |
| `Resource/GodotResourceManager.cs` | 直接实现 `IResourceManager`，PackageMode（`res://`）+ UpdatableMode（`user://` + `.pck` 热更） |
| `Resource/GodotLoadResourceAgentHelper.cs` | `ILoadResourceAgentHelper`，驱动 `LoadThreadedRequest` 异步流水线，逐帧轮询 |
| `Resource/GodotResourceHelper.cs` | `IResourceHelper`，`FileAccess` 读字节 + additive 场景卸载 + 资源释放 |
| `Resource/GodotResourceGroup.cs` | `IResourceGroup` 元数据容器（Godot 资源始终就绪） |
| `Resource/GodotResourceGroupCollection.cs` | `IResourceGroupCollection` 聚合视图 |
| `Resource/ResourceComponent.cs` | 封装 `GodotResourceManager`，`[Export]` 配置模式/Agent 数/路径，`_Process` 驱动异步轮询 |

#### 场景组件

| 文件 | 说明 |
|------|------|
| `Scene/SceneComponent.cs` | 基于 ResourceComponent additive 加载，管理场景切换 |

#### 数据 / 配置组件

| 文件 | 说明 |
|------|------|
| `DataTable/DataTableHelperBase.cs` | `IDataTableHelper` 抽象基类 |
| `DataTable/DataProviderHelperBase.cs` | `IDataProviderHelper` 抽象基类 |
| `DataTable/DefaultDataTableHelper.cs` | `IDataTableHelper` 标记接口实现 |
| `DataTable/DefaultDataProviderHelper.cs` | 支持二进制流和 TSV 字符串两种解析方式 |
| `DataTable/DataTableComponent.cs` | 封装 `IDataTableManager`，`LoadDataTable<T>(path)` 直接从文件加载并解析 |
| `Config/ConfigHelperBase.cs` | `IConfigHelper` 抽象基类 |
| `Config/DefaultConfigHelper.cs` | `IConfigHelper` + `IDataProviderHelper<IConfigManager>`，支持 TSV 和 JSON 扁平对象两种格式 |
| `Config/ConfigComponent.cs` | 封装 `IConfigManager`，`LoadConfig(path)` 同步读取 `res://`/`user://` 配置文件 |
| `Setting/SettingHelperBase.cs` | `ISettingHelper` 抽象基类 |
| `Setting/DefaultSettingHelper.cs` | `ISettingHelper`，使用 Godot `ConfigFile` 持久化到 `user://settings.cfg` |
| `Setting/SettingComponent.cs` | 封装 `ISettingManager`，`_Ready` 自动加载，`_ExitTree` 自动保存 |

#### 文件系统组件

| 文件 | 说明 |
|------|------|
| `FileSystem/FileSystemHelperBase.cs` | `IFileSystemHelper` 抽象基类 |
| `FileSystem/GodotFileSystemStream.cs` | 继承 `FileSystemStream`，将 `user://`/`res://` 解析为绝对路径后用 `System.IO.FileStream` 操作 |
| `FileSystem/GodotFileSystemHelper.cs` | `IFileSystemHelper`，工厂方法创建 `GodotFileSystemStream` |
| `FileSystem/FileSystemComponent.cs` | 封装 `IFileSystemManager`，管理虚拟文件系统包（`.vfs`）的创建/加载/销毁 |

#### 本地化组件

| 文件 | 说明 |
|------|------|
| `Localization/LocalizationHelperBase.cs` | `ILocalizationHelper` 抽象基类 |
| `Localization/LocalizationDataProviderHelperBase.cs` | `IDataProviderHelper<ILocalizationManager>` 抽象基类 |
| `Localization/GodotLocalizationHelper.cs` | 默认本地化 Helper |
| `Localization/DefaultLocalizationDataProviderHelper.cs` | TSV + JSON 双格式解析 |
| `Localization/LocalizationComponent.cs` | 封装 `ILocalizationManager`，`[Export]` 配置语言和字典文件路径 |

#### 网络 / 下载 / WebRequest 组件

| 文件 | 说明 |
|------|------|
| `Network/NetworkChannelHelperBase.cs` | `INetworkChannelHelper` 抽象基类 |
| `Network/DefaultNetworkChannelHelper.cs` | 4 字节长度前缀协议，可继承扩展 |
| `Network/NetworkComponent.cs` | 封装 `INetworkManager`，`[Export]` 配置协议类型和心跳间隔 |
| `Download/DownloadAgentHelperBase.cs` | `IDownloadAgentHelper` 抽象基类 |
| `Download/HttpDownloadAgentHelper.cs` | `HttpClient` 实现，支持 Range 断点续传 |
| `Download/DownloadComponent.cs` | 封装 `IDownloadManager`，`[Export]` 配置并发数和超时时间 |
| `WebRequest/WebRequestAgentHelperBase.cs` | `IWebRequestAgentHelper` 抽象基类 |
| `WebRequest/HttpWebRequestAgentHelper.cs` | `HttpClient` 实现 GET/POST |
| `WebRequest/WebRequestComponent.cs` | 封装 `IWebRequestManager`，`[Export]` 配置并发数和超时时间 |

#### 待完成

| 任务 | 说明 | 状态 |
|------|------|------|
| Debugger 窗口 | 用 Godot `Control` 实现 `IDebuggerWindow`，在编辑器内叠层显示 | ⬜ |

---

## 使用方式

### 场景 Node 树配置

在 Godot 场景中创建如下 Node 树（顺序即 `_Ready` 执行顺序）：

```
GameFramework (Node)
├── BaseComponent         # 必须最先初始化，配置所有 Helper 类型
├── ReferencePoolComponent
├── ObjectPoolComponent
├── EventComponent
├── ResourceComponent     # 资源组件（Entity/UI/Sound 组件依赖，需在它们之前）
├── ConfigComponent       # 全局只读配置（TSV / JSON）
├── SettingComponent      # 玩家持久化设置（user://settings.cfg）
├── FileSystemComponent   # 虚拟文件系统包管理
├── DataTableComponent    # 数据表组件
├── LocalizationComponent # 本地化组件
├── EntityComponent       # 实体管理
├── UIComponent           # UI 管理
├── SoundComponent        # 音频管理
├── SceneComponent        # 场景管理
├── NetworkComponent      # TCP 网络
├── DownloadComponent     # 下载管理
├── WebRequestComponent   # HTTP 请求
└── ProcedureComponent    # 最后调用 StartProcedures()
```

`BaseComponent` 的 `_Process` 会自动调用 `GameFrameworkEntry.Update`，无需手动驱动。

### BaseComponent Helper 配置

`BaseComponent` 通过 `[Export]` 暴露枚举类型（`HelperTypeEnums`），在 Godot Inspector 中下拉选择即可，无需手动编写代码注入：

```
BaseComponent (Inspector)
├── LogHelperType:         GodotLog（默认）/ 自定义扩展
├── JsonHelperType:        SystemTextJson（默认）
├── CompressionHelperType: Deflate（默认）
└── TextHelperType:        StringFormat（默认）
```

### DataTable 加载示例

```csharp
var dt = GameEntry.GetComponent<DataTableComponent>();

// 从二进制文件加载（推荐，运行时性能最优）
var heroTable = dt.LoadDataTable<DRHero>("res://DataTables/Bytes/Hero.bytes");

// 按 Id 读取
var hero = heroTable[1001];
GD.Print(hero.Name, hero.Level);
```

### Config 加载示例

```csharp
var cfg = GameEntry.GetComponent<ConfigComponent>();

// TSV 格式（Key\tValue，支持 # 注释行）
// JSON 格式（{"Key": Value} 扁平对象）
cfg.LoadConfig("res://Configs/game.json");

int maxLevel = cfg.GetInt("MaxLevel");
string addr  = cfg.GetString("ServerAddr", "localhost");
```

### Setting 持久化示例

```csharp
var setting = GameEntry.GetComponent<SettingComponent>();

// 写入（_ExitTree 自动保存；也可手动调用 Save）
setting.SetInt("MusicVolume", 80);
setting.SetBool("Fullscreen", true);
setting.SetObject("PlayerProfile", new PlayerProfile { Name = "Hero" });

// 读取（带默认值）
int vol         = setting.GetInt("MusicVolume", 100);
bool fullscreen = setting.GetBool("Fullscreen", false);
var profile     = setting.GetObject<PlayerProfile>("PlayerProfile");
```

### FileSystem 虚拟包示例

```csharp
var fs = GameEntry.GetComponent<FileSystemComponent>();

// 创建新包（写入模式）
IFileSystem vfs = fs.CreateFileSystem("user://data.vfs",
    FileSystemAccess.ReadWrite, maxFileCount: 128, maxBlockCount: 256);

vfs.WriteFile("greeting.txt", System.Text.Encoding.UTF8.GetBytes("Hello VFS"));
fs.DestroyFileSystem(vfs);

// 加载已有包（只读）
IFileSystem loaded = fs.LoadFileSystem("user://data.vfs", FileSystemAccess.Read);
byte[] result = loaded.ReadFile("greeting.txt");
```

### Resource 加载示例

```csharp
var res = GameEntry.GetComponent<ResourceComponent>();

// PackedScene 异步加载
res.LoadAsset("res://Prefabs/Hero.tscn",
    new LoadAssetCallbacks(
        onSuccess: (name, asset, duration, userData) => {
            AddChild(((PackedScene)asset).Instantiate());
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

// UpdatableMode 热更
res.ApplyResources("user://patch_v2.pck",
    (packPath, success) => GD.Print($"Patch applied: {success}")
);
```

### DataTableGenerator 工具使用方式

```bash
cd Tools/DataTableGenerator
dotnet build

# 一键：Excel → TSV → .bytes + C# 代码
dotnet run -- all GameData.xlsx \
  --text   Assets/DataTables/Text   \
  --bytes  Assets/DataTables/Bytes  \
  --code   Assets/Scripts/DataTable \
  --namespace MyGame

# 分步执行
dotnet run -- excel GameData.xlsx --text DataTables/Text  # Excel Sheet → TSV
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

## 架构说明

```
┌─────────────────────────────────────────────┐
│              Godot 游戏逻辑层                 │
│         (Framework.csproj / Node 脚本)        │
└────────────────┬────────────────────────────┘
                 │ 调用
┌────────────────▼────────────────────────────┐
│          GodotGameFramework 适配层            │  ← 除 Debugger 外已全部完成
│   实现各 IXxxHelper 接口，绑定 Godot API      │
│   所有 Helper 通过枚举下拉在 Inspector 配置   │
│   DataTable / Entity / UI / Event / Sound   │
│   Scene / Localization / Network / Download │
│   WebRequest / Config / Setting / FileSystem│
│   ObjectPool / ReferencePool / Variable     │
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
- `Resource` 模块的 `ResourceManager` 实现层（Asset Bundle 体系）不适用于 Godot，适配层直接实现 `IResourceManager` 接口
- 同理，`Scene`、`Sound`、`UI`、`Entity` 的 Manager 实现也直接实现接口，不继承原有 Manager
- `BaseComponent` 使用 `HelperTypeEnums` 枚举下拉统一管理 Helper 实例化，避免在代码中硬编码类型
- `Utility/Log.cs` 提供统一日志入口，解决 `GameFramework.Log` 与 Godot 命名空间冲突问题

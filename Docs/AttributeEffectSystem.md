# 装备与技能效果系统说明文档

## 概述

本系统为类暗黑破坏神俯视角游戏提供**装备词条、技能效果、Buff/Debuff**的统一管理框架。
核心设计目标：**数据驱动、低耦合、高扩展**。

---

## 目录结构

```
GameMain/Scripts/
├── AttributeSystem/
│   ├── AttributeType.cs        # 所有可修改属性的枚举定义
│   ├── AttributeModifier.cs    # 修饰器（固定值/百分比）
│   ├── CharacterAttribute.cs   # 单个属性：基础值 + 修饰器堆栈 + 缓存计算
│   └── AttributeContainer.cs  # 角色属性集合，统一管理所有属性
├── Effect/
│   ├── IEffect.cs              # Effect 接口定义
│   ├── EffectContext.cs        # Effect 执行上下文（目标属性容器 + 来源）
│   ├── EffectBase.cs           # 抽象基类，提供默认空实现
│   ├── EffectContainer.cs      # 挂载在角色上，管理 Effect 生命周期
│   └── Effects/
│       ├── FlatStatEffect.cs       # 固定值加成：+N 属性
│       ├── PercentStatEffect.cs    # 百分比加成：+N% 属性
│       ├── DotEffect.cs            # 持续伤害（中毒/燃烧/流血）
│       ├── ProcEffect.cs           # 概率触发效果
│       └── ConditionalEffect.cs    # 条件激活效果
├── Equipment/
│   └── Equipment.cs            # 装备数据持有者，管理词条 Effect 列表
├── Skill/
│   └── Skill.cs                # 技能数据持有者，管理冷却、法力消耗、主动/被动效果
└── Action/                     # (规划中) 行为链系统
    ├── IAction.cs              # 行为接口
    ├── ActionContext.cs        # 行为执行上下文
    ├── ActionChain.cs          # 行为链：有序插入与执行
    └── Actions/
        ├── DealDamageAction.cs     # 造成伤害
        ├── SpawnProjectileAction.cs# 生成弹道实体
        └── ApplyEffectAction.cs    # 施加 Effect 到目标
```

---

## 核心设计理念

### 技能/Buff/装备 = 实体（Entity）

所有可挂载的游戏对象——技能、Buff、装备——都被视为**依附于角色的实体**：

- **存在时创建**：购买装备 → 创建装备实体，绑定 Effect；学习技能 → 创建技能实体
- **移除时撤销**：卸除装备 → 实体隐藏/销毁，撤回所有由其施加的修改
- **生命周期绑定**：Effect 的作用域严格跟随宿主实体，不会泄漏

```
角色
├── 装备实体（sword_001）── Effect: +120 攻击
├── 技能实体（whirlwind）── Effect: +10% 攻击速度（被动）
└── Buff 实体（armor_down）── Effect: -30 护甲，3 秒后自动移除并还原
```

### 行为链（Action Chain）= 依赖注入式流程编排

游戏中所有发生的事件都是**行为（Action）**：普通攻击、施法、被击、造成伤害……

行为的关键特性：

1. **可独立触发**：操控角色普通攻击 → 执行攻击行为
2. **可插入其他行为前后**：道具/Buff/装备可以向行为链中注入自己的逻辑
3. **无限嵌套叠加**：多个来源的注入按优先级有序执行，不限层数

这类似依赖注入中的**中间件管道（Middleware Pipeline）**或**职责链（Chain of Responsibility）**模式。

---

## 核心概念

### 1. 属性系统（AttributeSystem）

所有角色数值通过**修饰器堆栈**计算，而不是直接修改基础值：

```
最终值 = (基础值 + Σ固定修饰) × (1 + Σ百分比修饰)
```

**示例：**
- 角色基础攻击力 100
- 装备 A 提供 +50 攻击（Flat）
- 装备 B 提供 +20% 攻击（Percent）
- 最终攻击力 = (100 + 50) × (1 + 0.2) = **180**

采用脏标记（dirty flag）延迟计算，只有值被读取时才重新计算，避免每帧重算开销。

```csharp
var attributes = new AttributeContainer();
attributes.SetBase(AttributeType.MaxHealth, 500f);
attributes.SetBase(AttributeType.AttackDamage, 100f);

float hp = attributes.GetValue(AttributeType.MaxHealth); // 500
```

---

### 2. Effect 系统

每个装备词条、技能效果、Buff/Debuff 都实现 `IEffect` 接口：

```csharp
public interface IEffect
{
    string EffectId { get; }
    EffectType EffectType { get; }
    bool IsExpired { get; }            // Timed 类型到期后返回 true，容器自动移除

    void OnApply(EffectContext context);   // 挂载时执行
    void OnRemove(EffectContext context);  // 卸载时执行
    void OnTick(EffectContext context, float delta); // 每帧驱动
}
```

#### EffectType 枚举

| 类型 | 说明 | 典型场景 |
|------|------|---------|
| `Passive` | 持续生效，随来源（装备/技能）卸除 | 装备属性词条 |
| `Timed` | 有持续时间，自动移除 | Buff、Debuff、中毒 |
| `Triggered` | 外部事件驱动 | Proc 触发 |

---

### 3. EffectContainer

挂载在每个角色（Entity）上，是 Effect 的宿主：

```csharp
var container = new EffectContainer(attributes, this);

// 每帧驱动（在 _Process 或 ProcedureMain 中调用）
container.Tick((float)delta);
```

`Tick()` 内部会：
1. 驱动所有 Effect 的 `OnTick()`
2. 自动移除 `IsExpired == true` 的 Timed Effect

---

### 4. 行为链系统（Action Chain）— 规划中

#### 核心接口

```csharp
// 每一个游戏行为实现此接口
public interface IAction
{
    string ActionId { get; }
    int Priority { get; }  // 数值越小越先执行（插入到前面）

    void Execute(ActionContext context);
}

// 行为执行上下文：携带本次行为的所有数据
public class ActionContext
{
    public Entity Source { get; }    // 发起者
    public Entity Target { get; }    // 目标
    public float Value { get; set; } // 可变中间值（如伤害量）
    // ... 其他扩展数据
}
```

#### 行为链（ActionChain）

```csharp
// 按优先级有序排列的行为队列
public class ActionChain
{
    private readonly List<IAction> _actions = new();

    // 插入行为（自动按 Priority 排序）
    public void Insert(IAction action) { ... }

    // 顺序执行所有行为
    public void Execute(ActionContext context)
    {
        foreach (var action in _actions)
            action.Execute(context);
    }
}
```

#### 典型流程：穿透护甲的普通攻击

```
角色普通攻击
    └─ 创建子弹实体，子弹命中目标时构建行为链：
           ┌─────────────────────────────────────────┐
           │ Priority=0  ArmorReductionAction         │ ← 减护甲装备注入
           │              └─ 创建 ArmorDownBuff 实体  │
           │                  └─ Buff 对目标施加       │
           │                      FlatStatEffect(-30)  │
           │ Priority=10 DealDamageAction              │ ← 基础伤害
           │              └─ 读取目标当前护甲（已减）  │
           │                 计算并扣除 HP             │
           └─────────────────────────────────────────┘
           Buff 持续时间结束后：
               ArmorDownBuff 实体调用 OnRemove
               → FlatStatEffect(-30) 被撤销
               → 目标护甲恢复
```

#### 为什么类似依赖注入？

- **行为链** = 容器（Container）
- **DealDamageAction** = 核心服务（被依赖方）
- **ArmorReductionAction** = 中间件（注入方）
- **减护甲装备存在时注入，卸除时不再注入** = 依赖的生命周期管理

任何数量的装备/Buff/技能都可以向同一条行为链注入自己的逻辑，且彼此不需要感知对方的存在。

---

## 内置 Effect 类型

### FlatStatEffect — 固定值加成

```csharp
// +50 攻击力（来自装备）
var effect = new FlatStatEffect("sword_dmg", AttributeType.AttackDamage, 50f);
```

### PercentStatEffect — 百分比加成

```csharp
// +20% 移动速度（来自鞋子词条）
var effect = new PercentStatEffect("boots_ms", AttributeType.MoveSpeed, 0.20f);
```

### DotEffect — 持续伤害

```csharp
// 中毒：每秒造成 15 点伤害，持续 5 秒，每 1 秒触发一次
var poison = new DotEffect("poison_01", damagePerTick: 15f, tickInterval: 1f, duration: 5f);
container.AddEffect(poison);
// 5 秒后自动移除
```

### ProcEffect — 概率触发

```csharp
// 命中时 8% 概率触发冰冻 Debuff
var freezeDebuff = new DotEffect("freeze", 0f, 1f, 2f);
var proc = new ProcEffect("item_freeze_proc", procChance: 0.08f, innerEffect: freezeDebuff);

if (proc.TryProc(targetContext))
{
    // 冰冻已施加到目标
}
```

### ConditionalEffect — 条件激活

```csharp
// 血量低于 30% 时，攻击力 +50%（"绝境增幅"词条）
var berserker = new PercentStatEffect("berserk_inner", AttributeType.AttackDamage, 0.50f);
var conditional = new ConditionalEffect(
    "berserk",
    ctx => ctx.Attributes.GetValue(AttributeType.CurrentHealth) /
           ctx.Attributes.GetValue(AttributeType.MaxHealth) < 0.3f,
    berserker
);
container.AddEffect(conditional);
```

---

## 装备系统（Equipment）

装备是依附于角色的实体，购买时创建并绑定 Effect，卸除时撤销所有影响：

```csharp
var legendSword = new Equipment("sword_001", "灭世之剑", EquipmentSlot.Weapon, ItemRarity.Legendary);

legendSword.AddEffect(new FlatStatEffect("sword_dmg", AttributeType.AttackDamage, 120f));
legendSword.AddEffect(new PercentStatEffect("sword_crit", AttributeType.CritChance, 0.15f));
legendSword.AddEffect(new ProcEffect("sword_proc", 0.05f, new DotEffect("sword_fire", 30f, 1f, 3f)));

// 装备时：所有 Effect.OnApply 被调用
legendSword.Equip(playerEffectContainer);

// 卸装时：所有 Effect.OnRemove 被调用，属性还原
legendSword.Unequip(playerEffectContainer);
```

### 带行为注入的装备

装备除了持有 Effect，还可以向行为链注入动作（规划中）：

```csharp
// 减护甲装备：命中时向伤害行为链前插入减护甲动作
armorBreakerItem.RegisterActionInjector(
    triggerActionId: "deal_damage",        // 在造成伤害行为上注入
    injectedAction: new ArmorReductionAction(priority: 0, reduction: 30f, duration: 3f)
);
```

---

## 技能系统（Skill）

```csharp
var whirlwind = new Skill("skill_whirlwind", "旋风斩", manaCost: 20f, cooldown: 3f);

// 被动修饰：学习后持续生效
whirlwind.AddPassiveModifier(new PercentStatEffect("ww_atkspd", AttributeType.AttackSpeed, 0.10f));

// 学习/遗忘（被动加成跟随学习状态，与实体生命周期绑定）
whirlwind.Learn(playerEffectContainer);
whirlwind.Forget(playerEffectContainer);

// 释放技能
if (whirlwind.TryCast(playerContext))
{
    foreach (var effect in whirlwind.ActiveEffects)
        effect.OnApply(targetContext);
}

whirlwind.Tick((float)delta);
```

---

## 完整示例：减护甲弹道攻击流程

```
1. 玩家操控角色发起普通攻击
2. 创建 Bullet 实体，Bullet 飞行
3. Bullet 命中目标，开始构建行为链（ActionChain）

   行为链（执行顺序按 Priority 升序）：
   ┌─────────────────────────────────────────────────────┐
   │ [0]  ArmorReductionAction（减护甲装备注入）          │
   │       → 创建 ArmorDownBuff 实体挂载到目标            │
   │       → ArmorDownBuff.OnApply: 施加 FlatStatEffect  │
   │            (-30 护甲) 到目标 AttributeContainer      │
   │ [10] DealDamageAction（基础伤害）                    │
   │       → 读取目标当前护甲（此时已减 30）              │
   │       → 计算最终伤害并扣除目标 HP                    │
   └─────────────────────────────────────────────────────┘

4. ArmorDownBuff 持续计时（Timed Effect，3 秒）
5. 3 秒后 IsExpired = true，EffectContainer 自动触发
      ArmorDownBuff.OnRemove → 撤销 FlatStatEffect(-30)
      → 目标护甲恢复原值
```

每一层注入都彼此独立，可叠加任意数量，逻辑互不感知。

---

## 与框架集成

### 角色 Entity 集成模板

```csharp
public partial class CharacterEntity : Node
{
    public AttributeContainer Attributes { get; private set; }
    public EffectContainer Effects { get; private set; }

    public override void _Ready()
    {
        Attributes = new AttributeContainer();
        Effects = new EffectContainer(Attributes, this);
        InitBaseAttributes();
    }

    public override void _Process(double delta)
    {
        Effects.Tick((float)delta);
    }

    private void InitBaseAttributes()
    {
        Attributes.SetBase(AttributeType.MaxHealth, 500f);
        Attributes.SetBase(AttributeType.CurrentHealth, 500f);
        Attributes.SetBase(AttributeType.AttackDamage, 100f);
        Attributes.SetBase(AttributeType.MoveSpeed, 300f);
    }
}
```

### EventComponent 联动

```csharp
// 装备变化时通过事件广播，避免硬耦合
GameEntry.Event.Fire(this, EquipmentChangedEventArgs.Create(slot, oldItem, newItem));

// UI 监听事件刷新属性面板
GameEntry.Event.Subscribe(EquipmentChangedEventArgs.EventId, OnEquipmentChanged);
```

---

## 数据驱动扩展

后续可将 Effect 参数化，通过 `DataTableComponent` 从 CSV 读取，实现零代码添加新词条：

| 数据表 | 字段 |
|--------|------|
| `DT_Item.csv` | ItemId, Name, Slot, Rarity, EffectId1, EffectId2, EffectId3 |
| `DT_Effect.csv` | EffectId, EffectClass, TargetAttribute, FlatValue, PercentValue, Duration |
| `DT_Skill.csv` | SkillId, Name, ManaCost, Cooldown, PassiveEffectId, ActiveEffectId |
| `DT_Action.csv` | ActionId, ActionClass, Priority, TargetActionId, Params |

---

## 扩展指南

### 新增自定义 Effect

1. 继承 `EffectBase`
2. 重写 `OnApply` / `OnRemove` / `OnTick`（按需）
3. 在 `OnApply` 中向 `context.Attributes` 添加修饰器，`OnRemove` 中移除

### 新增行为注入（规划中）

1. 实现 `IAction` 接口，填写 `Priority`（数值越小越优先）
2. 在装备/Buff/技能的 `OnApply` 中向目标行为链注册，`OnRemove` 中注销
3. 无需修改任何现有 Action 代码

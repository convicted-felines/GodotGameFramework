using System.Collections.Generic;

namespace GameMain
{
    public enum EquipmentSlot
    {
        Weapon,
        OffHand,
        Helmet,
        Chest,
        Gloves,
        Boots,
        Ring1,
        Ring2,
        Amulet,
        Belt,
    }

    public enum ItemRarity
    {
        Normal,
        Magic,
        Rare,
        Legendary,
        Set,
    }

    /// <summary>
    /// 装备实体：购买时创建并绑定 Effect 与行为注入器，卸除时撤销所有影响。
    /// </summary>
    public sealed class Equipment
    {
        public string ItemId { get; }
        public string Name { get; }
        public EquipmentSlot Slot { get; }
        public ItemRarity Rarity { get; }

        private readonly List<IEffect> _passiveEffects = new();
        private readonly List<(ActionChain chain, IAction action)> _injections = new();

        public IReadOnlyList<IEffect> PassiveEffects => _passiveEffects;

        public Equipment(string itemId, string name, EquipmentSlot slot, ItemRarity rarity)
        {
            ItemId = itemId;
            Name = name;
            Slot = slot;
            Rarity = rarity;
        }

        public void AddEffect(IEffect effect) => _passiveEffects.Add(effect);

        /// <summary>
        /// 注册行为注入：装备时向指定行为链插入 action，卸装时自动移除。
        /// </summary>
        public void RegisterActionInjector(ActionChain chain, IAction action)
        {
            _injections.Add((chain, action));
        }

        /// <summary>装备：施加所有被动 Effect 并向行为链注入 Action。</summary>
        public void Equip(EffectContainer container)
        {
            foreach (var effect in _passiveEffects)
                container.AddEffect(effect);

            foreach (var (chain, action) in _injections)
                chain.Insert(action);
        }

        /// <summary>卸装：撤销所有被动 Effect 并从行为链移除注入的 Action。</summary>
        public void Unequip(EffectContainer container)
        {
            foreach (var effect in _passiveEffects)
                container.RemoveEffect(effect);

            foreach (var (chain, action) in _injections)
                chain.Remove(action);
        }
    }
}

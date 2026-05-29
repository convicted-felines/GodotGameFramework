namespace GameMain
{
    public enum AttributeType
    {
        // 生命与魔法
        MaxHealth,
        CurrentHealth,
        HealthRegen,
        MaxMana,
        CurrentMana,
        ManaRegen,

        // 攻击
        AttackDamage,
        AttackSpeed,
        CritChance,
        CritMultiplier,
        LifeSteal,
        Thorns,

        // 防御
        Armor,
        MagicResistance,
        DodgeChance,
        BlockChance,

        // 技能
        CastSpeed,
        CooldownReduction,
        SkillDamageBonus,
        AreaOfEffect,
        ProjectileCount,
        ProjectileSpeed,

        // 移动
        MoveSpeed,

        // 元素伤害加成
        FireDamageBonus,
        ColdDamageBonus,
        LightningDamageBonus,
        PoisonDamageBonus,

        // 元素抗性
        FireResistance,
        ColdResistance,
        LightningResistance,
        PoisonResistance,
    }
}

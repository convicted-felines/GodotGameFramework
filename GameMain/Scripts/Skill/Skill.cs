using System.Collections.Generic;

namespace GameMain
{
    public sealed class Skill
    {
        public string SkillId { get; }
        public string Name { get; }
        public float ManaCost { get; }
        public float Cooldown { get; }

        /// <summary>技能加点时挂载到角色的被动修饰效果（如提升属性）。</summary>
        private readonly List<IEffect> _passiveModifiers = new();

        /// <summary>释放技能时执行的主动效果（伤害、位移、召唤等）。</summary>
        private readonly List<IEffect> _activeEffects = new();

        public IReadOnlyList<IEffect> PassiveModifiers => _passiveModifiers;
        public IReadOnlyList<IEffect> ActiveEffects => _activeEffects;

        private float _cooldownTimer;
        public bool IsReady => _cooldownTimer <= 0f;

        public Skill(string skillId, string name, float manaCost, float cooldown)
        {
            SkillId = skillId;
            Name = name;
            ManaCost = manaCost;
            Cooldown = cooldown;
        }

        public void AddPassiveModifier(IEffect effect) => _passiveModifiers.Add(effect);
        public void AddActiveEffect(IEffect effect) => _activeEffects.Add(effect);

        /// <summary>学习此技能时，将被动修饰施加到角色 EffectContainer。</summary>
        public void Learn(EffectContainer container)
        {
            foreach (var effect in _passiveModifiers)
                container.AddEffect(effect);
        }

        /// <summary>遗忘此技能时，移除被动修饰。</summary>
        public void Forget(EffectContainer container)
        {
            foreach (var effect in _passiveModifiers)
                container.RemoveEffect(effect);
        }

        /// <summary>
        /// 尝试释放技能，返回 false 表示冷却中或法力不足。
        /// 实际伤害逻辑由调用方根据 ActiveEffects 列表执行。
        /// </summary>
        public bool TryCast(EffectContext casterContext)
        {
            if (!IsReady) return false;

            float mana = casterContext.Attributes.GetValue(AttributeType.CurrentMana);
            if (mana < ManaCost) return false;

            casterContext.Attributes.Get(AttributeType.CurrentMana).BaseValue -= ManaCost;
            _cooldownTimer = Cooldown;
            return true;
        }

        public void Tick(float delta)
        {
            if (_cooldownTimer > 0f)
                _cooldownTimer -= delta;
        }
    }
}

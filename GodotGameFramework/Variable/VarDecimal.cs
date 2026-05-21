//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;

namespace GodotGameFramework
{
    public sealed class VarDecimal : Variable<decimal>
    {
        public VarDecimal() { }

        public static implicit operator VarDecimal(decimal value)
        {
            VarDecimal varValue = ReferencePool.Acquire<VarDecimal>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator decimal(VarDecimal value)
        {
            return value.Value;
        }
    }
}

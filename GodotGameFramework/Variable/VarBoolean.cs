//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;

namespace GodotGameFramework
{
    public sealed class VarBoolean : Variable<bool>
    {
        public VarBoolean() { }

        public static implicit operator VarBoolean(bool value)
        {
            VarBoolean varValue = ReferencePool.Acquire<VarBoolean>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator bool(VarBoolean value)
        {
            return value.Value;
        }
    }
}

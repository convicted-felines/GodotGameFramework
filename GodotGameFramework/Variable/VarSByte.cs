//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;

namespace GodotGameFramework
{
    public sealed class VarSByte : Variable<sbyte>
    {
        public VarSByte() { }

        public static implicit operator VarSByte(sbyte value)
        {
            VarSByte varValue = ReferencePool.Acquire<VarSByte>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator sbyte(VarSByte value)
        {
            return value.Value;
        }
    }
}

//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;

namespace GodotGameFramework
{
    public sealed class VarUInt64 : Variable<ulong>
    {
        public VarUInt64() { }

        public static implicit operator VarUInt64(ulong value)
        {
            VarUInt64 varValue = ReferencePool.Acquire<VarUInt64>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator ulong(VarUInt64 value)
        {
            return value.Value;
        }
    }
}

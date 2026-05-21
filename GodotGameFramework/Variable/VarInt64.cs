//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;

namespace GodotGameFramework
{
    public sealed class VarInt64 : Variable<long>
    {
        public VarInt64() { }

        public static implicit operator VarInt64(long value)
        {
            VarInt64 varValue = ReferencePool.Acquire<VarInt64>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator long(VarInt64 value)
        {
            return value.Value;
        }
    }
}

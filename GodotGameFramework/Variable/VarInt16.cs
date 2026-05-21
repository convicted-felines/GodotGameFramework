//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;

namespace GodotGameFramework
{
    public sealed class VarInt16 : Variable<short>
    {
        public VarInt16() { }

        public static implicit operator VarInt16(short value)
        {
            VarInt16 varValue = ReferencePool.Acquire<VarInt16>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator short(VarInt16 value)
        {
            return value.Value;
        }
    }
}

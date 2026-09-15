//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;

namespace GodotGameFramework
{
    public sealed class VarInt32 : Variable<int>
    {
        public VarInt32() { }

        public static implicit operator VarInt32(int value)
        {
            VarInt32 varValue = ReferencePool.Acquire<VarInt32>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator int(VarInt32 value)
        {
            return value?.Value ?? 0;
        }
    }
}

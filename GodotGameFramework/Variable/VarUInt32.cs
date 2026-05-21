//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;

namespace GodotGameFramework
{
    public sealed class VarUInt32 : Variable<uint>
    {
        public VarUInt32() { }

        public static implicit operator VarUInt32(uint value)
        {
            VarUInt32 varValue = ReferencePool.Acquire<VarUInt32>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator uint(VarUInt32 value)
        {
            return value.Value;
        }
    }
}

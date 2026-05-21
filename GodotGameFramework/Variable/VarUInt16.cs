//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;

namespace GodotGameFramework
{
    public sealed class VarUInt16 : Variable<ushort>
    {
        public VarUInt16() { }

        public static implicit operator VarUInt16(ushort value)
        {
            VarUInt16 varValue = ReferencePool.Acquire<VarUInt16>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator ushort(VarUInt16 value)
        {
            return value.Value;
        }
    }
}

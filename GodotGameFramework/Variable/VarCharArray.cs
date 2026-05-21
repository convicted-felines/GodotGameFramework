//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;

namespace GodotGameFramework
{
    public sealed class VarCharArray : Variable<char[]>
    {
        public VarCharArray() { }

        public static implicit operator VarCharArray(char[] value)
        {
            VarCharArray varValue = ReferencePool.Acquire<VarCharArray>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator char[](VarCharArray value)
        {
            return value.Value;
        }
    }
}

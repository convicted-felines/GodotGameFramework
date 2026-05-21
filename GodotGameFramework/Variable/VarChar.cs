//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;

namespace GodotGameFramework
{
    public sealed class VarChar : Variable<char>
    {
        public VarChar() { }

        public static implicit operator VarChar(char value)
        {
            VarChar varValue = ReferencePool.Acquire<VarChar>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator char(VarChar value)
        {
            return value.Value;
        }
    }
}

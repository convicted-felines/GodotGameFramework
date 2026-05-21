//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;

namespace GodotGameFramework
{
    public sealed class VarObject : Variable<object>
    {
        public VarObject() { }

        public static implicit operator VarObject(object value)
        {
            VarObject varValue = ReferencePool.Acquire<VarObject>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator object(VarObject value)
        {
            return value.Value;
        }
    }
}

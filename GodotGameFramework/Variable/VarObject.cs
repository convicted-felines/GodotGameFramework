//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;

namespace GodotGameFramework
{
    public sealed class VarObject : Variable<object>
    {
        public VarObject() { }

        public static VarObject Create(object value)
        {
            VarObject varValue = ReferencePool.Acquire<VarObject>();
            varValue.Value = value;
            return varValue;
        }
    }
}

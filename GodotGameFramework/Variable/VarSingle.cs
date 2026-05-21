//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;

namespace GodotGameFramework
{
    public sealed class VarSingle : Variable<float>
    {
        public VarSingle() { }

        public static implicit operator VarSingle(float value)
        {
            VarSingle varValue = ReferencePool.Acquire<VarSingle>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator float(VarSingle value)
        {
            return value.Value;
        }
    }
}

//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;

namespace GodotGameFramework
{
    public sealed class VarDouble : Variable<double>
    {
        public VarDouble() { }

        public static implicit operator VarDouble(double value)
        {
            VarDouble varValue = ReferencePool.Acquire<VarDouble>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator double(VarDouble value)
        {
            return value.Value;
        }
    }
}

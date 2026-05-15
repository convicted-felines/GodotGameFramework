using System;
using System.IO;

namespace DataTableGenerator
{
    public sealed partial class DataTableProcessor
    {
        public abstract class GenericDataProcessor<T> : DataProcessor
        {
            public override Type Type => typeof(T);
            public override bool IsId => false;
            public override bool IsComment => false;

            public abstract T Parse(string value);
        }
    }
}

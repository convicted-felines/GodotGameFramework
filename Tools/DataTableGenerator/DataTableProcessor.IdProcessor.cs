using System;
using System.IO;

namespace DataTableGenerator
{
    public sealed partial class DataTableProcessor
    {
        private sealed class IdProcessor : DataProcessor
        {
            public override Type Type => typeof(int);
            public override bool IsId => true;
            public override bool IsComment => false;
            public override bool IsSystem => false;
            public override string LanguageKeyword => "int";

            public override string[] GetTypeStrings() => new[] { "id" };

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                binaryWriter.Write7BitEncodedInt(int.Parse(value));
            }
        }
    }
}

using System.IO;

namespace DataTableGenerator
{
    public sealed partial class DataTableProcessor
    {
        private sealed class UInt32Processor : GenericDataProcessor<uint>
        {
            public override bool IsSystem => true;
            public override string LanguageKeyword => "uint";

            public override string[] GetTypeStrings() => new[] { "uint", "uint32", "system.uint32" };

            public override uint Parse(string value) => uint.Parse(value);

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                // 7-bit encoding for variable-length uint
                binaryWriter.Write7BitEncodedInt((int)Parse(value));
            }
        }
    }
}

using System.IO;

namespace DataTableGenerator
{
    public sealed partial class DataTableProcessor
    {
        private sealed class UInt64Processor : GenericDataProcessor<ulong>
        {
            public override bool IsSystem => true;
            public override string LanguageKeyword => "ulong";

            public override string[] GetTypeStrings() => new[] { "ulong", "uint64", "system.uint64" };

            public override ulong Parse(string value) => ulong.Parse(value);

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                binaryWriter.Write7BitEncodedInt64((long)Parse(value));
            }
        }
    }
}

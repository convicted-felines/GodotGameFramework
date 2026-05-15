using System.IO;

namespace DataTableGenerator
{
    public sealed partial class DataTableProcessor
    {
        private sealed class Int64Processor : GenericDataProcessor<long>
        {
            public override bool IsSystem => true;
            public override string LanguageKeyword => "long";

            public override string[] GetTypeStrings() => new[] { "long", "int64", "system.int64" };

            public override long Parse(string value) => long.Parse(value);

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                binaryWriter.Write7BitEncodedInt64(Parse(value));
            }
        }
    }
}

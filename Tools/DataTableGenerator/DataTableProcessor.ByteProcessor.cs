using System.IO;

namespace DataTableGenerator
{
    public sealed partial class DataTableProcessor
    {
        private sealed class ByteProcessor : GenericDataProcessor<byte>
        {
            public override bool IsSystem => true;
            public override string LanguageKeyword => "byte";

            public override string[] GetTypeStrings() => new[] { "byte", "system.byte" };

            public override byte Parse(string value) => byte.Parse(value);

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}

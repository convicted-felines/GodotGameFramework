using System.IO;

namespace DataTableGenerator
{
    public sealed partial class DataTableProcessor
    {
        private sealed class UInt16Processor : GenericDataProcessor<ushort>
        {
            public override bool IsSystem => true;
            public override string LanguageKeyword => "ushort";

            public override string[] GetTypeStrings() => new[] { "ushort", "uint16", "system.uint16" };

            public override ushort Parse(string value) => ushort.Parse(value);

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}

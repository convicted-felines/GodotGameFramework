using System.IO;

namespace DataTableGenerator
{
    public sealed partial class DataTableProcessor
    {
        private sealed class StringProcessor : GenericDataProcessor<string>
        {
            public override bool IsSystem => true;
            public override string LanguageKeyword => "string";

            public override string[] GetTypeStrings() => new[] { "string", "system.string" };

            public override string Parse(string value) => value;

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}

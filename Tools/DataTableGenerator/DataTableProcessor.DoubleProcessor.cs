using System.IO;

namespace DataTableGenerator
{
    public sealed partial class DataTableProcessor
    {
        private sealed class DoubleProcessor : GenericDataProcessor<double>
        {
            public override bool IsSystem => true;
            public override string LanguageKeyword => "double";

            public override string[] GetTypeStrings() => new[] { "double", "system.double" };

            public override double Parse(string value) => double.Parse(value);

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}

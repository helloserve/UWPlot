namespace helloserve.com.UWPlot
{
    internal class DataExtents
    {
        public bool IsPrepared { get; set; }

        public double? ValueMin { get; set; }
        public string ValueMinString { get; set; }
        public int ValueMinLength => ValueMinString.Length;

        public double? ValueMax { get; set; }
        public string ValueMaxString { get; set; }
        public int ValueMaxLength => ValueMaxString.Length;

        public string LongestCategory { get; set; }

        public string LongestValueString => ValueMaxLength > ValueMinLength ? ValueMaxString : ValueMinString;

    }
}

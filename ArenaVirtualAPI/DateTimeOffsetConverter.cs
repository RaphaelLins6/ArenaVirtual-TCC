using System;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

public class DateTimeOffsetConverter : DefaultTypeConverter {
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData) {
        // Se o texto for nulo ou vazio, retorne null
        if (string.IsNullOrEmpty(text)) {
            return null;
        }

        if (long.TryParse(text, out long ticks)) {
            try {
                return new DateTimeOffset(ticks, TimeSpan.Zero).UtcDateTime;
            } catch (Exception) {
                return null;
            }
        }

        if (DateTime.TryParse(text, out var dateResult)) {
            return dateResult;
        }

        return base.ConvertFromString(text, row, memberMapData);
    }
}
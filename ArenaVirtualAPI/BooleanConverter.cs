using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System.Globalization;

public class CustomBooleanConverter : DefaultTypeConverter {
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData) {
        if (string.IsNullOrWhiteSpace(text)) {
            return false; // Ou null, se o tipo for bool?
        }

        if (bool.TryParse(text, out bool result)) {
            return result;
        }

        // Tente converter de "1" ou "0" se a conversão falhar
        if (text == "1") {
            return true;
        }
        if (text == "0") {
            return false;
        }

        return base.ConvertFromString(text, row, memberMapData);
    }
}

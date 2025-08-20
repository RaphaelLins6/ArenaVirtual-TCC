using ArenaVirtualAPI.Models;
using CsvHelper.Configuration;
using System.Globalization;

public sealed class TimeMap : ClassMap<Time> {
    public TimeMap() {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.Id).Ignore();
    }
}
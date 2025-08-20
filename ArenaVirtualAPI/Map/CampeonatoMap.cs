using ArenaVirtualAPI.Models;
using CsvHelper.Configuration;
using System.Globalization;

public sealed class CampeonatoMap : ClassMap<Campeonato> {
    public CampeonatoMap() {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.Id).Ignore();
    }
}
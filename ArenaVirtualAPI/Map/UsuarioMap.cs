using ArenaVirtualAPI.Models;
using CsvHelper.Configuration;
using System.Globalization;

public sealed class UsuarioMap : ClassMap<Usuario> {
    public UsuarioMap() {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.Id).Ignore();
    }
}
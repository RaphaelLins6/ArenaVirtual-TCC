using ArenaVirtualAPI.Models;
using CsvHelper;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Data;

public static class DataImporter {
    public static void ImportData(AppDbContext context, string filePath) {
        // Exclui todos os registros existentes antes da importação
        // Isso garante que você terá apenas os dados do seu arquivo CSV
        context.Usuarios.RemoveRange(context.Usuarios);
        context.SaveChanges();


        // Lê os dados do arquivo CSV
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture)) {
            // O CsvHelper irá mapear automaticamente os campos do CSV para as propriedades da sua classe Usuario
            var usuarios = csv.GetRecords<Usuario>().ToList();

            // Adiciona os usuários lidos do CSV ao banco de dados
            context.Usuarios.AddRange(usuarios);
            context.SaveChanges();
        }
    }
}
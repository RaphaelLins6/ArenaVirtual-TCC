using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.Models;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

public static class DataSeed {
    // Método principal para chamar todos os seeders
    public static void SeedAll(AppDbContext context) {
        SeedUsuarios(context);
        SeedTimes(context);
        SeedCampeonato(context);
    }

    private static void SeedUsuarios(AppDbContext context) {
        if (!context.Usuarios.Any()) {
            try {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                    MissingFieldFound = null,
                    IgnoreBlankLines = true,
                    HasHeaderRecord = true
                };

                string filePath = "C:\\Users\\rapha\\Documents\\TCC - Projeto\\ArenaVirtualAPI\\CSV\\Usuario.csv";
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, config)) {
                    csv.Context.TypeConverterCache.AddConverter<DateTime?>(new DateTimeOffsetConverter());
                    csv.Context.TypeConverterCache.AddConverter<DateTime>(new DateTimeOffsetConverter());
                    csv.Context.TypeConverterCache.AddConverter<bool>(new CustomBooleanConverter());
                    csv.Context.TypeConverterCache.AddConverter<bool?>(new CustomBooleanConverter());

                    csv.Context.RegisterClassMap<UsuarioMap>();

                    var records = csv.GetRecords<Usuario>().ToList();
                    context.Usuarios.AddRange(records);
                    context.SaveChanges();
                }
            } catch (Exception ex) {
                Console.WriteLine($"Erro ao importar 'Usuario.csv': {ex.Message}");
            }
        }
    }

    private static void SeedTimes(AppDbContext context) {
        if (!context.Times.Any()) {
            try {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                    MissingFieldFound = null,
                    IgnoreBlankLines = true,
                    HasHeaderRecord = true
                };

                string filePath = "C:\\Users\\rapha\\Documents\\TCC - Projeto\\ArenaVirtualAPI\\CSV\\Time.csv";
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, config)) {
                    csv.Context.TypeConverterCache.AddConverter<DateTime?>(new DateTimeOffsetConverter());
                    csv.Context.TypeConverterCache.AddConverter<DateTime>(new DateTimeOffsetConverter());
                    csv.Context.TypeConverterCache.AddConverter<bool>(new CustomBooleanConverter());
                    csv.Context.TypeConverterCache.AddConverter<bool?>(new CustomBooleanConverter());

                    csv.Context.RegisterClassMap<TimeMap>();

                    var records = csv.GetRecords<Time>().ToList();
                    context.Times.AddRange(records);
                    context.SaveChanges();
                }
            } catch (Exception ex) {
                Console.WriteLine($"Erro ao importar 'Time.csv': {ex.Message}");
            }
        }
    }

    private static void SeedCampeonato(AppDbContext context) {
        if (!context.Campeonatos.Any()) {
            try {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                    MissingFieldFound = null,
                    IgnoreBlankLines = true,
                    HasHeaderRecord = true
                };

                string filePath = "C:\\Users\\rapha\\Documents\\TCC - Projeto\\ArenaVirtualAPI\\CSV\\Campeonato.csv";
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, config)) {
                    csv.Context.TypeConverterCache.AddConverter<DateTime?>(new DateTimeOffsetConverter());
                    csv.Context.TypeConverterCache.AddConverter<DateTime>(new DateTimeOffsetConverter());
                    csv.Context.TypeConverterCache.AddConverter<bool>(new CustomBooleanConverter());
                    csv.Context.TypeConverterCache.AddConverter<bool?>(new CustomBooleanConverter());

                    csv.Context.RegisterClassMap<CampeonatoMap>();

                    var records = csv.GetRecords<Campeonato>().ToList();
                    context.Campeonatos.AddRange(records);
                    context.SaveChanges();
                }
            } catch (Exception ex) {
                Console.WriteLine($"Erro ao importar 'Campeonato.csv': {ex.Message}");
            }
        }
    }
}
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NeoMoto.Infrastructure;
using NeoMoto.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace NeoMoto.Api;

public static class DataSeeder
{
    private class Root
    {
        public List<Localizacao> localizacoes { get; set; } = new();
        public List<MotoItem> motos { get; set; } = new();
        public List<ManutencaoItem> manutencoes { get; set; } = new();
    }

    private class Localizacao
    {
        public int id_localizacao { get; set; }
        public string endereco { get; set; } = string.Empty;
        public string cep { get; set; } = string.Empty;
        public string cidade { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
    }

    private class MotoItem
    {
        public int id_moto { get; set; }
        public string placa { get; set; } = string.Empty;
        public string modelo { get; set; } = string.Empty;
        public int ano { get; set; }
        public string status { get; set; } = string.Empty;
        public int id_localizacao { get; set; }
    }

    private class ManutencaoItem
    {
        public int id_manutencao { get; set; }
        public string data { get; set; } = string.Empty;
        public string descricao { get; set; } = string.Empty;
        public decimal custo { get; set; }
        public int id_moto { get; set; }
    }

    public static async Task SeedAsync(NeoMotoDbContext db, string jsonPath, ILogger logger)
    {
        if (!File.Exists(jsonPath))
        {
            logger.LogWarning("Seed file not found: {Path}", jsonPath);
            return;
        }

        var hasData = await db.Filiais.AsNoTracking().AnyAsync();
        if (hasData)
        {
            logger.LogInformation("Database already has data. Skipping seeding.");
            return;
        }

        logger.LogInformation("Seeding database from {Path}", jsonPath);
        var text = await File.ReadAllTextAsync(jsonPath);
        var root = JsonSerializer.Deserialize<Root>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new Root();

        // Map localizacao id to Filial Id
        var locToFilialId = new Dictionary<int, Guid>();

        foreach (var loc in root.localizacoes)
        {
            var filial = new Filial
            {
                Id = Guid.NewGuid(),
                Nome = $"Filial {loc.cidade}",
                Endereco = loc.endereco,
                Cidade = loc.cidade,
                Uf = (loc.estado ?? "").Trim().ToUpperInvariant().PadRight(2).Substring(0, 2)
            };
            locToFilialId[loc.id_localizacao] = filial.Id;
            db.Filiais.Add(filial);
        }
        await db.SaveChangesAsync();

        // Map moto id to Guid
        var motoIdMap = new Dictionary<int, Guid>();
        foreach (var m in root.motos)
        {
            // skip if placa already exists
            if (await db.Motos.AsNoTracking().AnyAsync(x => x.Placa == m.placa))
                continue;

            var guid = Guid.NewGuid();
            var moto = new Moto
            {
                Id = guid,
                Placa = m.placa,
                Modelo = m.modelo,
                Ano = m.ano,
                FilialId = locToFilialId.TryGetValue(m.id_localizacao, out var fid) ? fid : locToFilialId.Values.First()
            };
            motoIdMap[m.id_moto] = guid;
            db.Motos.Add(moto);
        }
        await db.SaveChangesAsync();

        foreach (var man in root.manutencoes)
        {
            if (!motoIdMap.TryGetValue(man.id_moto, out var mid))
                continue;

            DateTime dt;
            if (!DateTime.TryParse(man.data, out dt))
                dt = DateTime.UtcNow;

            var manut = new Manutencao
            {
                Id = Guid.NewGuid(),
                MotoId = mid,
                Data = dt,
                Descricao = man.descricao,
                Custo = man.custo
            };
            db.Manutencoes.Add(manut);
        }
        await db.SaveChangesAsync();

        logger.LogInformation("Seeding completed.");
    }
}
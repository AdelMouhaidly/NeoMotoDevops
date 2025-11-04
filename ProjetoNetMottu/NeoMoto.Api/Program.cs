using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NeoMoto.Domain.Entities;
using NeoMoto.Infrastructure;
using Swashbuckle.AspNetCore.Filters;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NeoMoto.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "NeoMoto API",
        Version = "v1",
        Description = "API RESTful para gestão de frotas NeoMoto (motos, filiais, manutenções)",
    });
    options.ExampleFilters();
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<NeoMoto.Api.CreateFilialExample>();

var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
}

builder.Services.AddDbContext<NeoMotoDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NeoMotoDbContext>();
    db.Database.Migrate();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupSeeder");
    var seedPath = builder.Configuration.GetSection("Seed").GetValue<string>("RelationalJsonPath");
    if (!string.IsNullOrWhiteSpace(seedPath))
    {
        await DataSeeder.SeedAsync(db, seedPath, logger);
    }
}

object BuildPaginationLinks(HttpContext http, int pageNumber, int pageSize, int totalCount)
{
    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    var baseUrl = $"{http.Request.Scheme}://{http.Request.Host}{http.Request.Path}";
    string BuildUrl(int p) => QueryString.Create(new Dictionary<string, string?>
    {
        ["pageNumber"] = p.ToString(),
        ["pageSize"] = pageSize.ToString()
    }).ToUriComponent();

    return new
    {
        self = $"{baseUrl}{BuildUrl(pageNumber)}",
        first = pageNumber > 1 ? $"{baseUrl}{BuildUrl(1)}" : null,
        prev = pageNumber > 1 ? $"{baseUrl}{BuildUrl(pageNumber - 1)}" : null,
        next = pageNumber < totalPages ? $"{baseUrl}{BuildUrl(pageNumber + 1)}" : null,
        last = totalPages > 0 ? $"{baseUrl}{BuildUrl(totalPages)}" : null
    };
}

app.MapGet("/api/filiais", async (NeoMotoDbContext db, HttpContext http, int pageNumber = 1, int pageSize = 10) =>
{
    if (pageNumber < 1 || pageSize < 1 || pageSize > 100) return Results.BadRequest("Parâmetros de paginação inválidos.");
    var query = db.Filiais.AsNoTracking().OrderBy(f => f.Nome);
    var total = await query.CountAsync();
    var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    var itemsList = items.Select(f => new
    {
        f.Id,
        f.Nome,
        f.Endereco,
        f.Cidade,
        f.Uf,
        _links = new
        {
            self = $"/api/filiais/{f.Id}",
            motos = $"/api/filiais/{f.Id}/motos"
        }
    }).ToList();
    
    var result = new
    {
        items = itemsList,
        totalCount = total,
        _links = BuildPaginationLinks(http, pageNumber, pageSize, total)
    };
    
    var json = JsonSerializer.Serialize(result);
    return Results.Content(json, "application/json");
})
.WithName("GetFiliais")
.WithOpenApi();

app.MapGet("/api/filiais/{id:guid}", async Task<IResult> (NeoMotoDbContext db, Guid id) =>
{
    var filial = await db.Filiais.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
    if (filial == null) return Results.NotFound();
    var body = new
    {
        filial.Id,
        filial.Nome,
        filial.Endereco,
        filial.Cidade,
        filial.Uf,
        _links = new { self = $"/api/filiais/{filial.Id}", motos = $"/api/filiais/{filial.Id}/motos" }
    };
    return Results.Json(body);
})
.WithName("GetFilialById")
.WithOpenApi();

app.MapPost("/api/filiais", async Task<IResult> (NeoMotoDbContext db, Filial input) =>
{
    if (string.IsNullOrWhiteSpace(input.Nome)) return TypedResults.BadRequest("Nome é obrigatório.");
    db.Filiais.Add(input);
    await db.SaveChangesAsync();
    var body = new { input.Id, input.Nome, input.Endereco, input.Cidade, input.Uf, _links = new { self = $"/api/filiais/{input.Id}" } };
    return Results.Created($"/api/filiais/{input.Id}", body);
})
.WithName("CreateFilial")
.WithOpenApi(operation =>
{
    operation.Summary = "Cria uma nova filial";
    operation.Description = "Cria uma filial e retorna location no header.";
    return operation;
});

app.MapPut("/api/filiais/{id:guid}", async Task<Results<NoContent, NotFound, BadRequest<string>>> (NeoMotoDbContext db, Guid id, Filial input) =>
{
    if (id != input.Id) return TypedResults.BadRequest("Id do corpo difere do parâmetro.");
    var exists = await db.Filiais.AnyAsync(f => f.Id == id);
    if (!exists) return TypedResults.NotFound();
    db.Filiais.Update(input);
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
})
.WithName("UpdateFilial")
.WithOpenApi();

app.MapDelete("/api/filiais/{id:guid}", async Task<Results<NoContent, NotFound>> (NeoMotoDbContext db, Guid id) =>
{
    var entity = await db.Filiais.FindAsync(id);
    if (entity == null) return TypedResults.NotFound();
    db.Filiais.Remove(entity);
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
})
.WithName("DeleteFilial")
.WithOpenApi();

app.MapGet("/api/filiais/{id:guid}/motos", async (NeoMotoDbContext db, Guid id) =>
{
    var exists = await db.Filiais.AnyAsync(f => f.Id == id);
    if (!exists) return Results.NotFound();
    var motos = await db.Motos.AsNoTracking().Where(m => m.FilialId == id).ToListAsync();
    return Results.Json(motos.Select(m => new { m.Id, m.Placa, m.Modelo, m.Ano, _links = new { self = $"/api/motos/{m.Id}" } }));
})
.WithName("GetMotosByFilial")
.WithOpenApi();

app.MapGet("/api/motos", async (NeoMotoDbContext db, HttpContext http, int pageNumber = 1, int pageSize = 10) =>
{
    if (pageNumber < 1 || pageSize < 1 || pageSize > 100) return Results.BadRequest("Parâmetros de paginação inválidos.");
    var query = db.Motos.AsNoTracking().OrderBy(m => m.Placa);
    var total = await query.CountAsync();
    var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    var result = new
    {
        items = items.Select(m => new
        {
            m.Id,
            m.Placa,
            m.Modelo,
            m.Ano,
            m.FilialId,
            _links = new { self = $"/api/motos/{m.Id}", filial = $"/api/filiais/{m.FilialId}", manutencoes = $"/api/motos/{m.Id}/manutencoes" }
        }),
        totalCount = total,
        _links = BuildPaginationLinks(http, pageNumber, pageSize, total)
    };
    return Results.Json(result);
})
.WithName("GetMotos")
.WithOpenApi();

app.MapGet("/api/motos/{id:guid}", async Task<IResult> (NeoMotoDbContext db, Guid id) =>
{
    var moto = await db.Motos.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
    if (moto == null) return Results.NotFound();
    var body = new
    {
        moto.Id,
        moto.Placa,
        moto.Modelo,
        moto.Ano,
        moto.FilialId,
        _links = new { self = $"/api/motos/{moto.Id}", filial = $"/api/filiais/{moto.FilialId}", manutencoes = $"/api/motos/{moto.Id}/manutencoes" }
    };
    return Results.Json(body);
})
.WithName("GetMotoById")
.WithOpenApi();

app.MapPost("/api/motos", async Task<IResult> (NeoMotoDbContext db, Moto input) =>
{
    if (string.IsNullOrWhiteSpace(input.Placa) || string.IsNullOrWhiteSpace(input.Modelo)) return TypedResults.BadRequest("Placa e Modelo são obrigatórios.");
    var placaExists = await db.Motos.AnyAsync(m => m.Placa == input.Placa);
    if (placaExists) return TypedResults.BadRequest("Placa já cadastrada.");
    var filialExists = await db.Filiais.AnyAsync(f => f.Id == input.FilialId);
    if (!filialExists) return TypedResults.BadRequest("Filial inexistente.");
    db.Motos.Add(input);
    await db.SaveChangesAsync();
    var body = new { input.Id, input.Placa, input.Modelo, input.Ano, input.FilialId, _links = new { self = $"/api/motos/{input.Id}" } };
    return Results.Created($"/api/motos/{input.Id}", body);
})
.WithName("CreateMoto")
.WithOpenApi(operation =>
{
    operation.Summary = "Cria uma nova moto";
    operation.Description = "Cria uma moto vinculada a uma filial.";
    return operation;
});

app.MapPut("/api/motos/{id:guid}", async Task<Results<NoContent, NotFound, BadRequest<string>>> (NeoMotoDbContext db, Guid id, Moto input) =>
{
    if (id != input.Id) return TypedResults.BadRequest("Id do corpo difere do parâmetro.");
    var exists = await db.Motos.AsNoTracking().AnyAsync(m => m.Id == id);
    if (!exists) return TypedResults.NotFound();
    db.Motos.Update(input);
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
})
.WithName("UpdateMoto")
.WithOpenApi();

app.MapDelete("/api/motos/{id:guid}", async Task<Results<NoContent, NotFound>> (NeoMotoDbContext db, Guid id) =>
{
    var entity = await db.Motos.FindAsync(id);
    if (entity == null) return TypedResults.NotFound();
    db.Motos.Remove(entity);
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
})
.WithName("DeleteMoto")
.WithOpenApi();

app.MapGet("/api/motos/{id:guid}/manutencoes", async (NeoMotoDbContext db, Guid id, HttpContext http, int pageNumber = 1, int pageSize = 10) =>
{
    var exists = await db.Motos.AnyAsync(m => m.Id == id);
    if (!exists) return Results.NotFound();
    if (pageNumber < 1 || pageSize < 1 || pageSize > 100) return Results.BadRequest("Parâmetros de paginação inválidos.");
    var query = db.Manutencoes.AsNoTracking().Where(m => m.MotoId == id).OrderByDescending(m => m.Data);
    var total = await query.CountAsync();
    var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    var result = new
    {
        items = items.Select(m => new { m.Id, m.MotoId, m.Data, m.Descricao, m.Custo, _links = new { self = $"/api/manutencoes/{m.Id}", moto = $"/api/motos/{m.MotoId}" } }),
        totalCount = total,
        _links = BuildPaginationLinks(http, pageNumber, pageSize, total)
    };
    return Results.Json(result);
})
.WithName("GetManutencoesByMoto")
.WithOpenApi();

app.MapGet("/api/manutencoes", async (NeoMotoDbContext db, HttpContext http, int pageNumber = 1, int pageSize = 10) =>
{
    if (pageNumber < 1 || pageSize < 1 || pageSize > 100) return Results.BadRequest("Parâmetros de paginação inválidos.");
    var query = db.Manutencoes.AsNoTracking().OrderByDescending(m => m.Data);
    var total = await query.CountAsync();
    var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    var result = new
    {
        items = items.Select(m => new { m.Id, m.MotoId, m.Data, m.Descricao, m.Custo, _links = new { self = $"/api/manutencoes/{m.Id}", moto = $"/api/motos/{m.MotoId}" } }),
        totalCount = total,
        _links = BuildPaginationLinks(http, pageNumber, pageSize, total)
    };
    return Results.Json(result);
})
.WithName("GetManutencoes")
.WithOpenApi();

app.MapGet("/api/manutencoes/{id:guid}", async Task<IResult> (NeoMotoDbContext db, Guid id) =>
{
    var m = await db.Manutencoes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    if (m == null) return Results.NotFound();
    var body = new { m.Id, m.MotoId, m.Data, m.Descricao, m.Custo, _links = new { self = $"/api/manutencoes/{m.Id}", moto = $"/api/motos/{m.MotoId}" } };
    return Results.Json(body);
})
.WithName("GetManutencaoById")
.WithOpenApi();

app.MapPost("/api/manutencoes", async Task<IResult> (NeoMotoDbContext db, Manutencao input) =>
{
    var motoExists = await db.Motos.AnyAsync(m => m.Id == input.MotoId);
    if (!motoExists) return TypedResults.BadRequest("Moto inexistente.");
    if (string.IsNullOrWhiteSpace(input.Descricao)) return TypedResults.BadRequest("Descrição é obrigatória.");
    db.Manutencoes.Add(input);
    await db.SaveChangesAsync();
    var body = new { input.Id, input.MotoId, input.Data, input.Descricao, input.Custo, _links = new { self = $"/api/manutencoes/{input.Id}" } };
    return Results.Created($"/api/manutencoes/{input.Id}", body);
})
.WithName("CreateManutencao")
.WithOpenApi(operation =>
{
    operation.Summary = "Registra uma manutenção";
    operation.Description = "Cria uma manutenção para uma moto existente.";
    return operation;
});

app.MapPut("/api/manutencoes/{id:guid}", async Task<Results<NoContent, NotFound, BadRequest<string>>> (NeoMotoDbContext db, Guid id, Manutencao input) =>
{
    if (id != input.Id) return TypedResults.BadRequest("Id do corpo difere do parâmetro.");
    var exists = await db.Manutencoes.AsNoTracking().AnyAsync(m => m.Id == id);
    if (!exists) return TypedResults.NotFound();
    db.Manutencoes.Update(input);
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
})
.WithName("UpdateManutencao")
.WithOpenApi();

app.MapDelete("/api/manutencoes/{id:guid}", async Task<Results<NoContent, NotFound>> (NeoMotoDbContext db, Guid id) =>
{
    var entity = await db.Manutencoes.FindAsync(id);
    if (entity == null) return TypedResults.NotFound();
    db.Manutencoes.Remove(entity);
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
})
.WithName("DeleteManutencao")
.WithOpenApi();

app.Run();

public partial class Program { }

using Microsoft.EntityFrameworkCore;
using NeoMoto.Domain.Entities;

namespace NeoMoto.Infrastructure;

public class NeoMotoDbContext : DbContext
{
	public NeoMotoDbContext(DbContextOptions<NeoMotoDbContext> options) : base(options)
	{
	}

	public DbSet<Moto> Motos => Set<Moto>();
	public DbSet<Filial> Filiais => Set<Filial>();
	public DbSet<Manutencao> Manutencoes => Set<Manutencao>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Filial>()
			.HasMany(f => f.Motos)
			.WithOne(m => m.Filial!)
			.HasForeignKey(m => m.FilialId)
			.OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<Moto>()
			.HasIndex(m => m.Placa)
			.IsUnique();

		modelBuilder.Entity<Manutencao>()
			.HasOne(m => m.Moto!)
			.WithMany(mo => mo.Manutencoes)
			.HasForeignKey(m => m.MotoId)
			.OnDelete(DeleteBehavior.Cascade);

		var filialA = new Filial { Id = Guid.NewGuid(), Nome = "Filial Centro", Endereco = "Rua A, 100", Cidade = "São Paulo", Uf = "SP" };
		var filialB = new Filial { Id = Guid.NewGuid(), Nome = "Filial Zona Sul", Endereco = "Av. B, 200", Cidade = "São Paulo", Uf = "SP" };

		var moto1 = new Moto { Id = Guid.NewGuid(), Placa = "ABC1D23", Modelo = "Honda CG 160", Ano = 2022, FilialId = filialA.Id };
		var moto2 = new Moto { Id = Guid.NewGuid(), Placa = "EFG4H56", Modelo = "Yamaha Factor 150", Ano = 2021, FilialId = filialA.Id };
		var moto3 = new Moto { Id = Guid.NewGuid(), Placa = "IJK7L89", Modelo = "Honda Biz 125", Ano = 2020, FilialId = filialB.Id };

		var manut1 = new Manutencao { Id = Guid.NewGuid(), MotoId = moto1.Id, Data = DateTime.UtcNow.AddDays(-10), Descricao = "Troca de óleo", Custo = 120m };
		var manut2 = new Manutencao { Id = Guid.NewGuid(), MotoId = moto1.Id, Data = DateTime.UtcNow.AddDays(-5), Descricao = "Ajuste de corrente", Custo = 80m };
		var manut3 = new Manutencao { Id = Guid.NewGuid(), MotoId = moto3.Id, Data = DateTime.UtcNow.AddDays(-2), Descricao = "Pastilha de freio", Custo = 150m };

		modelBuilder.Entity<Filial>().HasData(filialA, filialB);
		modelBuilder.Entity<Moto>().HasData(moto1, moto2, moto3);
		modelBuilder.Entity<Manutencao>().HasData(manut1, manut2, manut3);
	}
}


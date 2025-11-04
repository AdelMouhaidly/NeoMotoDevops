using Swashbuckle.AspNetCore.Filters;
using NeoMoto.Domain.Entities;

namespace NeoMoto.Api;

public class CreateFilialExample : IExamplesProvider<Filial>
{
	public Filial GetExamples()
	{
		return new Filial
		{
			Nome = "Filial Centro",
			Endereco = "Rua A, 100",
			Cidade = "São Paulo",
			Uf = "SP"
		};
	}
}

public class CreateMotoExample : IExamplesProvider<Moto>
{
	public Moto GetExamples()
	{
		return new Moto
		{
			Placa = "ABC1D23",
			Modelo = "Honda CG 160",
			Ano = 2022,
			FilialId = Guid.Parse("00000000-0000-0000-0000-000000000001")
		};
	}
}

public class CreateManutencaoExample : IExamplesProvider<Manutencao>
{
	public Manutencao GetExamples()
	{
		return new Manutencao
		{
			MotoId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
			Data = DateTime.UtcNow,
			Descricao = "Troca de óleo",
			Custo = 120m
		};
	}
}

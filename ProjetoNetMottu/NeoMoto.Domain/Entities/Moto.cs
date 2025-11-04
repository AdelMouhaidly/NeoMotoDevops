using System;
using System.ComponentModel.DataAnnotations;

namespace NeoMoto.Domain.Entities;

public class Moto
{
	public Guid Id { get; set; } = Guid.NewGuid();

	[Required]
	[StringLength(20)]
	public string Placa { get; set; } = string.Empty;

	[Required]
	[StringLength(50)]
	public string Modelo { get; set; } = string.Empty;

	[Range(2000, 2100)]
	public int Ano { get; set; }

	[Required]
	public Guid FilialId { get; set; }

	public Filial? Filial { get; set; }

	public ICollection<Manutencao> Manutencoes { get; set; } = new List<Manutencao>();
}


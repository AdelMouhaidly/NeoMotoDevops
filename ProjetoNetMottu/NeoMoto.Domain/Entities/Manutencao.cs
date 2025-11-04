using System;
using System.ComponentModel.DataAnnotations;

namespace NeoMoto.Domain.Entities;

public class Manutencao
{
	public Guid Id { get; set; } = Guid.NewGuid();

	[Required]
	public Guid MotoId { get; set; }

	public Moto? Moto { get; set; }

	[Required]
	public DateTime Data { get; set; } = DateTime.UtcNow;

	[Required]
	[StringLength(200)]
	public string Descricao { get; set; } = string.Empty;

	[Range(0, double.MaxValue)]
	public decimal Custo { get; set; }
}


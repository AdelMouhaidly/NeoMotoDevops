using System;
using System.ComponentModel.DataAnnotations;

namespace NeoMoto.Domain.Entities;

public class Filial
{
	public Guid Id { get; set; } = Guid.NewGuid();

	[Required]
	[StringLength(120)]
	public string Nome { get; set; } = string.Empty;

	[Required]
	[StringLength(200)]
	public string Endereco { get; set; } = string.Empty;

	[Required]
	[StringLength(50)]
	public string Cidade { get; set; } = string.Empty;

	[StringLength(2)]
	public string Uf { get; set; } = "SP";

	public ICollection<Moto> Motos { get; set; } = new List<Moto>();
}


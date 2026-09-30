namespace Ciee.Curriculos.Api.Models;

public class Candidato
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? CargoInteresse { get; set; }
    public string? ResumoProfissional { get; set; }
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public bool TeveOrigemPdf { get; set; } = false;
}

namespace Ciee.Curriculos.Api.DTOs;

public record CriarCandidatoDto(
    string NomeCompleto,
    string Email,
    string? Telefone,
    string? CargoInteresse,
    string? ResumoProfissional,
    bool TeveOrigemPdf = false
);

public record CandidatoResponseDto(
    Guid Id,
    string NomeCompleto,
    string Email,
    string? Telefone,
    string? CargoInteresse,
    string? ResumoProfissional,
    DateTime DataCadastro,
    bool TeveOrigemPdf
);

public record ExtracaoPdfResponseDto(
    string? NomeCompleto,
    string? Email,
    string? Telefone,
    string? CargoInteresse,
    string? ResumoProfissional,
    string TextoBruto,
    bool Sucesso,
    string Mensagem
);

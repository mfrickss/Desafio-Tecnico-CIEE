using Ciee.Curriculos.Api.Data;
using Ciee.Curriculos.Api.DTOs;
using Ciee.Curriculos.Api.Models;
using Ciee.Curriculos.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ciee.Curriculos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CandidatosController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPdfExtractionService _pdfExtractionService;
    private readonly IValidator<CriarCandidatoDto> _validator;
    private readonly ILogger<CandidatosController> _logger;

    public CandidatosController(
        AppDbContext context,
        IPdfExtractionService pdfExtractionService,
        IValidator<CriarCandidatoDto> validator,
        ILogger<CandidatosController> logger)
    {
        _context = context;
        _pdfExtractionService = pdfExtractionService;
        _validator = validator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CandidatoResponseDto>>> Listar([FromQuery] string? busca)
    {
        var query = _context.Candidatos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToLower();
            query = query.Where(c =>
                c.NomeCompleto.ToLower().Contains(termo) ||
                c.Email.ToLower().Contains(termo) ||
                (c.CargoInteresse != null && c.CargoInteresse.ToLower().Contains(termo)));
        }

        var candidatos = await query
            .OrderByDescending(c => c.DataCadastro)
            .Select(c => new CandidatoResponseDto(
                c.Id,
                c.NomeCompleto,
                c.Email,
                c.Telefone,
                c.CargoInteresse,
                c.ResumoProfissional,
                c.DataCadastro,
                c.TeveOrigemPdf))
            .ToListAsync();

        return Ok(candidatos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CandidatoResponseDto>> ObterPorId(Guid id)
    {
        var c = await _context.Candidatos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (c == null)
        {
            return NotFound(new { mensagem = "Candidato nao encontrado." });
        }

        return Ok(new CandidatoResponseDto(
            c.Id,
            c.NomeCompleto,
            c.Email,
            c.Telefone,
            c.CargoInteresse,
            c.ResumoProfissional,
            c.DataCadastro,
            c.TeveOrigemPdf));
    }

    [HttpPost]
    public async Task<ActionResult<CandidatoResponseDto>> Criar([FromBody] CriarCandidatoDto dto)
    {
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                mensagem = "Dados invalidos para cadastro do candidato.",
                erros = validationResult.Errors.Select(e => new { campo = e.PropertyName, erro = e.ErrorMessage })
            });
        }

        var candidato = new Candidato
        {
            NomeCompleto = dto.NomeCompleto.Trim(),
            Email = dto.Email.Trim().ToLower(),
            Telefone = dto.Telefone?.Trim(),
            CargoInteresse = dto.CargoInteresse?.Trim(),
            ResumoProfissional = dto.ResumoProfissional?.Trim(),
            TeveOrigemPdf = dto.TeveOrigemPdf,
            DataCadastro = DateTime.UtcNow
        };

        _context.Candidatos.Add(candidato);
        await _context.SaveChangesAsync();

        var responseDto = new CandidatoResponseDto(
            candidato.Id,
            candidato.NomeCompleto,
            candidato.Email,
            candidato.Telefone,
            candidato.CargoInteresse,
            candidato.ResumoProfissional,
            candidato.DataCadastro,
            candidato.TeveOrigemPdf);

        return CreatedAtAction(nameof(ObterPorId), new { id = candidato.Id }, responseDto);
    }

    [HttpPost("extrair-pdf")]
    [Consumes("multipart/form-data")]
    public IActionResult ExtrairPdf(IFormFile? arquivo)
    {
        if (arquivo == null || arquivo.Length == 0)
        {
            return BadRequest(new { mensagem = "Nenhum arquivo enviado. Selecione um arquivo PDF." });
        }

        const long tamanhoMaximoBytes = 5 * 1024 * 1024; // 5 MB
        if (arquivo.Length > tamanhoMaximoBytes)
        {
            return BadRequest(new { mensagem = "O arquivo excede o limite maximo permitido de 5 MB." });
        }

        var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
        if (extensao != ".pdf" || (arquivo.ContentType != "application/pdf" && !string.IsNullOrEmpty(arquivo.ContentType) && arquivo.ContentType != "application/octet-stream"))
        {
            return BadRequest(new { mensagem = "Formato de arquivo invalido. Apenas documentos PDF sao aceitos." });
        }

        try
        {
            using var stream = arquivo.OpenReadStream();
            var resultado = _pdfExtractionService.ExtrairDados(stream);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar arquivo PDF de curriculo.");
            return StatusCode(500, new { mensagem = "Ocorreu um erro interno ao processar o arquivo PDF." });
        }
    }
}

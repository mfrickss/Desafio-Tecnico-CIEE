using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ciee.Curriculos.Api.Common;
using Ciee.Curriculos.Api.Data;
using Ciee.Curriculos.Api.DTOs;
using Ciee.Curriculos.Api.Models;
using Ciee.Curriculos.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ciee.Curriculos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CandidatosController : ControllerBase
{
    private static readonly byte[] PdfMagicBytes = Encoding.ASCII.GetBytes("%PDF-");

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
    [ProducesResponseType(typeof(IEnumerable<CandidatoResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CandidatoResponseDto>>> Listar([FromQuery] string? busca)
    {
        var query = _context.Candidatos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            query = query.Where(c =>
                c.NomeCompleto.Contains(termo) ||
                c.Email.Contains(termo) ||
                (c.CargoInteresse != null && c.CargoInteresse.Contains(termo)));
        }

        var candidatos = await query
            .OrderByDescending(c => c.DataCadastro)
            .Take(100)
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
    [ProducesResponseType(typeof(CandidatoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CandidatoResponseDto>> ObterPorId(Guid id)
    {
        var c = await _context.Candidatos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (c == null)
        {
            return Problem(
                detail: $"Nenhum candidato localizado com o identificador '{id}'.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Candidato não encontrado");
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
    [ProducesResponseType(typeof(CandidatoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CandidatoResponseDto>> Criar([FromBody] CriarCandidatoDto dto)
    {
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errosFormatados = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            var problem = new ValidationProblemDetails(errosFormatados)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Dados inválidos para cadastro do candidato.",
                Detail = "Um ou mais campos contêm erros de validação.",
                Instance = HttpContext.Request.Path
            };

            problem.Extensions["mensagem"] = "Dados inválidos para cadastro do candidato.";
            problem.Extensions["erros"] = validationResult.Errors.Select(e => new { campo = e.PropertyName, erro = e.ErrorMessage }).ToList();

            return BadRequest(problem);
        }

        var candidato = new Candidato
        {
            NomeCompleto = dto.NomeCompleto.Trim(),
            Email = dto.Email.Trim().ToLower(),
            Telefone = TelefoneHelper.FormatarTelefoneBrasil(dto.Telefone),
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
    [ProducesResponseType(typeof(ExtracaoPdfResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult ExtrairPdf(IFormFile? arquivo)
    {
        if (arquivo == null || arquivo.Length == 0)
        {
            return CriarProblemBadRequest(
                "Nenhum arquivo enviado. Selecione um arquivo PDF.",
                "O corpo da requisição não possui um arquivo anexado.");
        }

        const long tamanhoMaximoBytes = 5 * 1024 * 1024; // 5 MB
        if (arquivo.Length > tamanhoMaximoBytes)
        {
            return CriarProblemBadRequest(
                "O arquivo excede o limite máximo permitido de 5 MB.",
                $"Tamanho recebido: {arquivo.Length} bytes. Máximo permitido: {tamanhoMaximoBytes} bytes.");
        }

        var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
        if (extensao != ".pdf" || (arquivo.ContentType != "application/pdf" && !string.IsNullOrEmpty(arquivo.ContentType) && arquivo.ContentType != "application/octet-stream"))
        {
            return CriarProblemBadRequest(
                "Formato de arquivo inválido. Apenas documentos PDF são aceitos.",
                "A extensão ou o MIME type informado não é suportado.");
        }

        try
        {
            using var stream = arquivo.OpenReadStream();
            if (!ValidarMagicBytesPdf(stream))
            {
                return CriarProblemBadRequest(
                    "O arquivo enviado não é um PDF válido ou está corrompido.",
                    "O cabeçalho do arquivo não contém a assinatura binária esperada de um documento PDF (%PDF-).");
            }

            stream.Position = 0;
            var resultado = _pdfExtractionService.ExtrairDados(stream);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar arquivo PDF de currículo.");
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Erro interno no processamento",
                Detail = "Ocorreu um erro interno ao processar o arquivo PDF.",
                Instance = HttpContext.Request.Path
            };
            problem.Extensions["mensagem"] = "Ocorreu um erro interno ao processar o arquivo PDF.";

            return StatusCode(StatusCodes.Status500InternalServerError, problem);
        }
    }

    private static bool ValidarMagicBytesPdf(Stream stream)
    {
        if (!stream.CanRead || stream.Length < PdfMagicBytes.Length)
        {
            return false;
        }

        var buffer = new byte[PdfMagicBytes.Length];
        var bytesLidos = stream.Read(buffer, 0, buffer.Length);

        return bytesLidos == PdfMagicBytes.Length && buffer.SequenceEqual(PdfMagicBytes);
    }

    private ObjectResult CriarProblemBadRequest(string mensagem, string detail) =>
        Problem(
            detail: detail,
            statusCode: StatusCodes.Status400BadRequest,
            title: mensagem);
}

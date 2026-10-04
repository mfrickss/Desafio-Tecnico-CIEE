using System.IO;
using Ciee.Curriculos.Api.DTOs;

namespace Ciee.Curriculos.Api.Services;

public interface IPdfExtractionService
{
    ExtracaoPdfResponseDto ExtrairDados(Stream pdfStream);
}

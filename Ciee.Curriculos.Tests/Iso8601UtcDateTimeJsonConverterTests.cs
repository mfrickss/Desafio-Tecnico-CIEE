using System;
using System.Text.Json;
using Ciee.Curriculos.Api.Common.Converters;
using Ciee.Curriculos.Api.DTOs;
using Xunit;

namespace Ciee.Curriculos.Tests;

public class Iso8601UtcDateTimeJsonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public Iso8601UtcDateTimeJsonConverterTests()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new Iso8601UtcDateTimeJsonConverter());
    }

    [Fact]
    public void Serializar_DataUtc_DeveConterSufixoZ()
    {
        var data = new DateTime(2026, 10, 4, 15, 30, 45, 123, DateTimeKind.Utc);
        var json = JsonSerializer.Serialize(data, _options);

        Assert.Equal("\"2026-10-04T15:30:45.123Z\"", json);
    }

    [Fact]
    public void Serializar_CandidatoResponseDto_DeveSerializarDataCadastroEmIso8601Utc()
    {
        var dto = new CandidatoResponseDto(
            Id: Guid.NewGuid(),
            NomeCompleto: "João Silva",
            Email: "joao.silva@exemplo.com",
            Telefone: "(11) 98765-4321",
            CargoInteresse: "Desenvolvedor .NET",
            ResumoProfissional: "Resumo",
            DataCadastro: new DateTime(2026, 10, 4, 12, 0, 0, 0, DateTimeKind.Utc),
            TeveOrigemPdf: false
        );

        var json = JsonSerializer.Serialize(dto, _options);

        Assert.Contains("\"dataCadastro\":\"2026-10-04T12:00:00.000Z\"", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Deserializar_DataIso8601Utc_DeveConverterParaKindUtc()
    {
        var json = "\"2026-10-04T15:30:45.123Z\"";
        var data = JsonSerializer.Deserialize<DateTime>(json, _options);

        Assert.Equal(DateTimeKind.Utc, data.Kind);
        Assert.Equal(2026, data.Year);
        Assert.Equal(10, data.Month);
        Assert.Equal(4, data.Day);
    }
}

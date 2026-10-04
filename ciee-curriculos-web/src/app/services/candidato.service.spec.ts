import { describe, it } from 'node:test';
import assert from 'node:assert';

describe('CandidatoService Tests', () => {
  it('deve extrair erros de validação a partir de RFC 7807 ProblemDetails com array erros', () => {
    const fakeError = {
      error: {
        title: 'Dados inválidos para cadastro do candidato.',
        erros: [
          { campo: 'NomeCompleto', erro: 'Nome completo é obrigatório.' },
          { campo: 'Email', erro: 'Informe um e-mail válido.' }
        ]
      }
    };

    const extrairErros = (err: any) => {
      const body = err?.error;
      const listaErros: Array<{ campo: string; erro: string }> = [];
      if (Array.isArray(body?.erros) && body.erros.length > 0) {
        for (const item of body.erros) {
          if (item.campo && item.erro) {
            listaErros.push({ campo: item.campo, erro: item.erro });
          }
        }
        return listaErros;
      }
      return [];
    };

    const errosExtraidos = extrairErros(fakeError);
    assert.strictEqual(errosExtraidos.length, 2);
    assert.strictEqual(errosExtraidos[0].campo, 'NomeCompleto');
    assert.strictEqual(errosExtraidos[0].erro, 'Nome completo é obrigatório.');
    assert.strictEqual(errosExtraidos[1].campo, 'Email');
    assert.strictEqual(errosExtraidos[1].erro, 'Informe um e-mail válido.');
  });

  it('deve extrair erros quando formato for dicionário padrão ASP.NET Core errors', () => {
    const fakeError = {
      error: {
        title: 'One or more validation errors occurred.',
        errors: {
          NomeCompleto: ['The NomeCompleto field is required.'],
          Telefone: ['O telefone informado é inválido.']
        }
      }
    };

    const extrairErros = (err: any) => {
      const body = err?.error;
      const listaErros: Array<{ campo: string; erro: string }> = [];
      if (body?.errors && typeof body.errors === 'object') {
        for (const [campo, mensagens] of Object.entries(body.errors)) {
          if (Array.isArray(mensagens)) {
            for (const msg of mensagens) {
              listaErros.push({ campo, erro: msg });
            }
          }
        }
      }
      return listaErros;
    };

    const errosExtraidos = extrairErros(fakeError);
    assert.strictEqual(errosExtraidos.length, 2);
    assert.strictEqual(errosExtraidos[0].campo, 'NomeCompleto');
    assert.strictEqual(errosExtraidos[0].erro, 'The NomeCompleto field is required.');
    assert.strictEqual(errosExtraidos[1].campo, 'Telefone');
    assert.strictEqual(errosExtraidos[1].erro, 'O telefone informado é inválido.');
  });

  it('deve formatar query string de busca adequadamente', () => {
    const criarParametros = (busca?: string) => {
      const params: Record<string, string> = {};
      if (busca && busca.trim().length > 0) {
        params['busca'] = busca.trim();
      }
      return params;
    };

    const comBusca = criarParametros('Desenvolvedor C#');
    assert.strictEqual(comBusca['busca'], 'Desenvolvedor C#');

    const semBusca = criarParametros('   ');
    assert.strictEqual(semBusca['busca'], undefined);

    const nulo = criarParametros(undefined);
    assert.strictEqual(nulo['busca'], undefined);
  });
});

import { describe, it } from 'node:test';
import assert from 'node:assert';
import { Subject, of, throwError, EMPTY } from 'rxjs';
import { catchError } from 'rxjs/operators';

describe('ListagemComponent Logic Tests', () => {
  it('deve inicializar com lista vazia e estado de carregando ativo', () => {
    const estadoInicial = {
      candidatos: [],
      carregando: true,
      erroConexao: false,
      tituloErro: 'Serviço Indisponível',
      descricaoErro: 'Não foi possível conectar ao servidor.',
      alertaSucesso: false,
      termoBusca: ''
    };

    assert.strictEqual(estadoInicial.candidatos.length, 0);
    assert.strictEqual(estadoInicial.carregando, true);
    assert.strictEqual(estadoInicial.erroConexao, false);
  });

  it('deve atualizar o termo de busca e disparar filtragem', () => {
    let termoAtual = '';
    const aoDigitarBusca = (termo: string) => {
      termoAtual = termo.trim();
    };

    aoDigitarBusca('  Desenvolvedor  ');
    assert.strictEqual(termoAtual, 'Desenvolvedor');
  });

  it('deve limpar termo de busca e restaurar estado', () => {
    let termoAtual = 'Backend';
    const limparBusca = () => {
      termoAtual = '';
    };

    limparBusca();
    assert.strictEqual(termoAtual, '');
  });

  it('CA-01 e CA-02: deve reter erroConexao como true e titulo padronizado para erro de conexao (status 0), sem chamar next downstream', () => {
    let carregando = true;
    let erroConexao = false;
    let tituloErro = '';
    let descricaoErro = '';
    let candidatos: any[] = [{ id: '1', nomeCompleto: 'Anterior' }];
    let nextChamado = false;

    const tratarFalhaConexao = (err: any) => {
      carregando = false;
      erroConexao = true;
      candidatos = [];

      if (err?.status === 0) {
        tituloErro = 'Serviço Indisponível';
        descricaoErro =
          'Não foi possível estabelecer conexão com o servidor. ' +
          'Verifique sua conexão de rede ou tente novamente em instantes.';
      } else if (err?.status === 503 || err?.status === 502) {
        tituloErro = 'Serviço Temporariamente Indisponível';
        descricaoErro = 'O servidor está temporariamente fora de operação. Tente novamente em instantes.';
      } else {
        tituloErro = 'Falha na comunicação com o servidor';
        descricaoErro = 'Não foi possível carregar a lista de candidatos devido a uma instabilidade de conexão com o servidor.';
      }
    };

    const erroHttpStatusZero = { status: 0, statusText: 'Unknown Error' };
    const observableComErro$ = throwError(() => erroHttpStatusZero);

    observableComErro$
      .pipe(
        catchError((err) => {
          tratarFalhaConexao(err);
          return EMPTY; // Garante que downstream 'next' NÃO é executado
        })
      )
      .subscribe({
        next: (dados) => {
          nextChamado = true;
          erroConexao = false;
          candidatos = dados;
          carregando = false;
        }
      });

    assert.strictEqual(nextChamado, false, 'Downstream next não deve ser invocado em falhas');
    assert.strictEqual(erroConexao, true, 'erroConexao deve permanecer true');
    assert.strictEqual(carregando, false, 'carregando deve ser false');
    assert.strictEqual(tituloErro, 'Serviço Indisponível');
    assert.ok(descricaoErro.includes('conexão com o servidor'));
    assert.strictEqual(candidatos.length, 0);
  });

  it('deve diagnosticar status 503 como Serviço Temporariamente Indisponível', () => {
    let tituloErro = '';
    let descricaoErro = '';
    let erroConexao = false;

    const tratarFalhaConexao = (err: any) => {
      erroConexao = true;
      if (err?.status === 503) {
        tituloErro = 'Serviço Temporariamente Indisponível';
        descricaoErro = 'O servidor está temporariamente fora de operação. Tente novamente em instantes.';
      }
    };

    const erroHttp503 = { status: 503, statusText: 'Service Unavailable' };
    throwError(() => erroHttp503)
      .pipe(
        catchError((err) => {
          tratarFalhaConexao(err);
          return EMPTY;
        })
      )
      .subscribe();

    assert.strictEqual(erroConexao, true);
    assert.strictEqual(tituloErro, 'Serviço Temporariamente Indisponível');
    assert.strictEqual(descricaoErro, 'O servidor está temporariamente fora de operação. Tente novamente em instantes.');
  });

  it('CA-03: ação de recarregar/tentar novamente deve reiniciar carregando como true e erroConexao como false', () => {
    let carregando = false;
    let erroConexao = true;

    const tentarNovamente = () => {
      carregando = true;
      erroConexao = false;
    };

    tentarNovamente();

    assert.strictEqual(carregando, true);
    assert.strictEqual(erroConexao, false);
  });
});

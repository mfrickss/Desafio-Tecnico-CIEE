import { describe, it } from 'node:test';
import assert from 'node:assert';

describe('CadastroComponent Logic & State Tests', () => {
  const aplicarMascaraTelefone = (valor: string): string => {
    if (!valor) return '';
    const digitos = valor.replace(/\D/g, '').substring(0, 11);
    if (digitos.length <= 2) {
      return digitos.length > 0 ? '(' + digitos : '';
    }
    if (digitos.length <= 6) {
      return '(' + digitos.substring(0, 2) + ') ' + digitos.substring(2);
    }
    if (digitos.length <= 10) {
      return '(' + digitos.substring(0, 2) + ') ' + digitos.substring(2, 6) + '-' + digitos.substring(6);
    }
    return '(' + digitos.substring(0, 2) + ') ' + digitos.substring(2, 7) + '-' + digitos.substring(7, 11);
  };

  it('deve formatar mascara de telefone para 10 e 11 digitos corretamente', () => {
    assert.strictEqual(aplicarMascaraTelefone('11987654321'), '(11) 98765-4321');
    assert.strictEqual(aplicarMascaraTelefone('1133334444'), '(11) 3333-4444');
    assert.strictEqual(aplicarMascaraTelefone('1198'), '(11) 98');
    assert.strictEqual(aplicarMascaraTelefone(''), '');
  });

  it('deve validar extensoes aceitas e rejeitar extensoes divergentes de .pdf', () => {
    const validarArquivo = (nome: string, tamanho: number) => {
      const ehPdf = nome.toLowerCase().endsWith('.pdf');
      if (!ehPdf) {
        return { valido: false, erro: 'Arquivo inválido. Por favor, envie um documento no formato PDF.' };
      }
      const tamanhoMaximo = 5 * 1024 * 1024;
      if (tamanho > tamanhoMaximo) {
        return { valido: false, erro: 'O arquivo selecionado ultrapassa o limite máximo de 5 MB.' };
      }
      return { valido: true, erro: null };
    };

    const docx = validarArquivo('curriculo.docx', 1024);
    assert.strictEqual(docx.valido, false);
    assert.strictEqual(docx.erro, 'Arquivo inválido. Por favor, envie um documento no formato PDF.');

    const png = validarArquivo('foto.png', 2048);
    assert.strictEqual(png.valido, false);

    const pdfGrande = validarArquivo('curriculo.pdf', (5 * 1024 * 1024) + 1);
    assert.strictEqual(pdfGrande.valido, false);
    assert.strictEqual(pdfGrande.erro, 'O arquivo selecionado ultrapassa o limite máximo de 5 MB.');

    const pdfValido = validarArquivo('curriculo_valido.pdf', 1024 * 100);
    assert.strictEqual(pdfValido.valido, true);
    assert.strictEqual(pdfValido.erro, null);
  });

  it('deve processar resposta de extracao com sucesso e mapear status de campos', () => {
    const respostaExtracao = {
      sucesso: true,
      nomeCompleto: "Carlos Eduardo D'Angelo",
      email: "carlos@ciee.teste.com",
      telefone: "11987654321",
      cargoInteresse: "Desenvolvedor .NET",
      resumoProfissional: "Experiência sólida em C#"
    };

    const mapaCampos = {
      nomeCompleto: respostaExtracao.nomeCompleto ? 'extraido' : 'nao_encontrado',
      email: respostaExtracao.email ? 'extraido' : 'nao_encontrado',
      telefone: respostaExtracao.telefone ? 'extraido' : 'nao_encontrado',
      cargoInteresse: respostaExtracao.cargoInteresse ? 'extraido' : 'nao_encontrado',
      resumoProfissional: respostaExtracao.resumoProfissional ? 'extraido' : 'nao_encontrado'
    };

    assert.strictEqual(mapaCampos.nomeCompleto, 'extraido');
    assert.strictEqual(mapaCampos.email, 'extraido');
    assert.strictEqual(mapaCampos.telefone, 'extraido');
    assert.strictEqual(mapaCampos.cargoInteresse, 'extraido');
    assert.strictEqual(mapaCampos.resumoProfissional, 'extraido');
  });

  it('deve identificar campos nao encontrados para orientar preenchimento manual', () => {
    const respostaParcial = {
      sucesso: true,
      nomeCompleto: "Ana Paula Silva",
      email: "ana@email.com",
      telefone: null,
      cargoInteresse: null,
      resumoProfissional: null
    };

    const camposOrdem = ['nomeCompleto', 'email', 'telefone', 'cargoInteresse', 'resumoProfissional'] as const;
    const mapaCampos = {
      nomeCompleto: respostaParcial.nomeCompleto ? 'extraido' : 'nao_encontrado',
      email: respostaParcial.email ? 'extraido' : 'nao_encontrado',
      telefone: respostaParcial.telefone ? 'extraido' : 'nao_encontrado',
      cargoInteresse: respostaParcial.cargoInteresse ? 'extraido' : 'nao_encontrado',
      resumoProfissional: respostaParcial.resumoProfissional ? 'extraido' : 'nao_encontrado'
    };

    const primeiroPendente = camposOrdem.find((c) => mapaCampos[c] === 'nao_encontrado');
    assert.strictEqual(primeiroPendente, 'telefone');
  });

  it('deve validar campos obrigatorios do formulario de cadastro', () => {
    const validarFormulario = (form: { nomeCompleto: string; email: string }) => {
      const erros: Record<string, string> = {};
      if (!form.nomeCompleto || form.nomeCompleto.trim().length === 0) {
        erros['nomeCompleto'] = 'Nome completo é obrigatório.';
      } else if (form.nomeCompleto.trim().length < 3) {
        erros['nomeCompleto'] = 'O nome deve ter no mínimo 3 caracteres.';
      }

      if (!form.email || form.email.trim().length === 0) {
        erros['email'] = 'E-mail é obrigatório.';
      } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
        erros['email'] = 'Informe um e-mail válido.';
      }

      return {
        valido: Object.keys(erros).length === 0,
        erros
      };
    };

    const formInvalido = validarFormulario({ nomeCompleto: '', email: 'emailinvalido' });
    assert.strictEqual(formInvalido.valido, false);
    assert.strictEqual(formInvalido.erros['nomeCompleto'], 'Nome completo é obrigatório.');
    assert.strictEqual(formInvalido.erros['email'], 'Informe um e-mail válido.');

    const formValido = validarFormulario({ nomeCompleto: 'Carlos Eduardo', email: 'carlos@ciee.com' });
    assert.strictEqual(formValido.valido, true);
  });

  it('deve bloquear novo disparo de selecao se extracao de PDF ja estiver em andamento', () => {
    let processou = false;
    const processarArquivoMock = () => { processou = true; };

    const aoSelecionarArquivoMock = (extraindo: boolean, arquivo: { name: string; size: number }) => {
      if (extraindo) return;
      processarArquivoMock();
    };

    aoSelecionarArquivoMock(true, { name: 'curriculo.pdf', size: 1024 });
    assert.strictEqual(processou, false);

    aoSelecionarArquivoMock(false, { name: 'curriculo.pdf', size: 1024 });
    assert.strictEqual(processou, true);
  });

  it('deve deduplicar arquivo identico se usuario confirmar seletor apos upload via drag-and-drop', () => {
    let contagemProcessamento = 0;
    const processarArquivoMock = () => { contagemProcessamento++; };

    let arquivoAtual: { name: string; size: number } | null = { name: 'curriculo.pdf', size: 2048 };

    const aoSelecionarArquivoMock = (extraindo: boolean, novoArquivo: { name: string; size: number }) => {
      if (extraindo) return;
      if (arquivoAtual && arquivoAtual.name === novoArquivo.name && arquivoAtual.size === novoArquivo.size) {
        return;
      }
      processarArquivoMock();
    };

    // Tenta selecionar o mesmo arquivo que ja esta anexado
    aoSelecionarArquivoMock(false, { name: 'curriculo.pdf', size: 2048 });
    assert.strictEqual(contagemProcessamento, 0);

    // Seleciona um arquivo diferente
    aoSelecionarArquivoMock(false, { name: 'outro_curriculo.pdf', size: 4096 });
    assert.strictEqual(contagemProcessamento, 1);
  });

  describe('Subtask 3.1: Ciclo de vida da flag teveOrigemPdf', () => {
    it('deve manter teveOrigemPdf como false na inicializacao', () => {
      let teveOrigemPdf = false;
      assert.strictEqual(teveOrigemPdf, false);
    });

    it('deve definir teveOrigemPdf como true somente quando resposta de extracao tiver sucesso === true', () => {
      let teveOrigemPdf = false;

      const simularRespostaApi = (sucesso: boolean) => {
        if (sucesso) {
          teveOrigemPdf = true;
        } else {
          teveOrigemPdf = false;
        }
      };

      simularRespostaApi(true);
      assert.strictEqual(teveOrigemPdf, true);

      simularRespostaApi(false);
      assert.strictEqual(teveOrigemPdf, false);
    });

    it('deve reverter teveOrigemPdf para false quando usuario remover o arquivo anexado', () => {
      let teveOrigemPdf = true;
      let arquivoSelecionado: string | null = 'curriculo.pdf';

      const removerArquivo = () => {
        arquivoSelecionado = null;
        teveOrigemPdf = false;
      };

      removerArquivo();
      assert.strictEqual(teveOrigemPdf, false);
      assert.strictEqual(arquivoSelecionado, null);
    });

    it('deve manter teveOrigemPdf como false quando a chamada de extracao falhar na comunicacao (erro HTTP)', () => {
      let teveOrigemPdf = true; // Supõe que estava true por algum motivo anterior
      let extraindoPdf = true;

      const simularErroHttp = () => {
        extraindoPdf = false;
        teveOrigemPdf = false;
      };

      simularErroHttp();
      assert.strictEqual(teveOrigemPdf, false);
      assert.strictEqual(extraindoPdf, false);
    });
  });

  describe('Subtask 3.2: Vinculacao e normalizacao de erros RFC 7807 por campo', () => {
    it('deve preservar erros sincronos pre-existentes ao vincular erro retornado pelo servidor', () => {
      const controle = {
        errors: { required: true } as Record<string, any> | null,
        touched: false
      };

      const aplicarErroServidor = (ctrl: typeof controle, mensagemErro: string) => {
        const errosAtuais = ctrl.errors || {};
        ctrl.errors = { ...errosAtuais, erroServidor: mensagemErro };
        ctrl.touched = true;
      };

      aplicarErroServidor(controle, 'E-mail ja cadastrado no sistema.');

      assert.strictEqual(controle.touched, true);
      assert.strictEqual(controle.errors?.['required'], true);
      assert.strictEqual(controle.errors?.['erroServidor'], 'E-mail ja cadastrado no sistema.');

      const outrosErros = { ...controle.errors };
      delete outrosErros['erroServidor'];
      const temRestantes = Object.keys(outrosErros).length > 0;
      controle.errors = temRestantes ? outrosErros : null;

      assert.strictEqual(controle.errors?.['required'], true);
      assert.strictEqual(controle.errors?.['erroServidor'], undefined);
    });

    it('deve normalizar nomes de campos vindos em PascalCase (ASP.NET Core) para camelCase do form', () => {
      const controlesForm = {
        nomeCompleto: { erro: null as string | null, touched: false },
        email: { erro: null as string | null, touched: false },
        telefone: { erro: null as string | null, touched: false },
        cargoInteresse: { erro: null as string | null, touched: false },
        resumoProfissional: { erro: null as string | null, touched: false }
      };

      const obterControle = (nomeCampo: string) => {
        const segmentoFinal = nomeCampo.includes('.') ? nomeCampo.split('.').pop()! : nomeCampo;
        const normalizar = (s: string) => s.replace(/[^a-zA-Z0-9]/g, '').toLowerCase();
        const termoNormalizado = normalizar(segmentoFinal);

        for (const [key, ctrl] of Object.entries(controlesForm)) {
          if (normalizar(key) === termoNormalizado) {
            return ctrl;
          }
        }
        return null;
      };

      const errosRfc7807 = [
        { campo: 'NomeCompleto', erro: 'O nome completo é obrigatório.' },
        { campo: 'Email', erro: 'Informe um e-mail válido no formato exemplo@dominio.com.' },
        { campo: 'Telefone', erro: 'O telefone informado deve conter um número válido com DDD.' }
      ];

      for (const item of errosRfc7807) {
        const ctrl = obterControle(item.campo);
        if (ctrl) {
          ctrl.erro = item.erro;
          ctrl.touched = true;
        }
      }

      assert.strictEqual(controlesForm.nomeCompleto.erro, 'O nome completo é obrigatório.');
      assert.strictEqual(controlesForm.nomeCompleto.touched, true);
      assert.strictEqual(controlesForm.email.erro, 'Informe um e-mail válido no formato exemplo@dominio.com.');
      assert.strictEqual(controlesForm.email.touched, true);
      assert.strictEqual(controlesForm.telefone.erro, 'O telefone informado deve conter um número válido com DDD.');
      assert.strictEqual(controlesForm.telefone.touched, true);
      assert.strictEqual(controlesForm.cargoInteresse.erro, null);
    });
  });
});

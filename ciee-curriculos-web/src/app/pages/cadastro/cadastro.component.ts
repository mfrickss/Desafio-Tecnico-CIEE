import { Component, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl } from '@angular/forms';
import { Router } from '@angular/router';
import { CandidatoService } from '../../services/candidato.service';
import { ExtracaoPdfResponse } from '../../models/candidato.model';
import { AppIconComponent } from '../../shared/ui/icon.component';
import { CieeButtonDirective } from '../../shared/ui/button.directive';
import { CieeBadgeDirective } from '../../shared/ui/badge.directive';
import { CieeInputDirective } from '../../shared/ui/input.directive';
import { 
  CieeCardComponent, 
  CieeCardHeaderDirective, 
  CieeCardTitleDirective, 
  CieeCardDescriptionDirective, 
  CieeCardContentDirective, 
  CieeCardFooterDirective 
} from '../../shared/ui/card.components';

export type StatusExtracaoCampo = 'extraido' | 'nao_encontrado' | 'manual';

export interface MapaCamposExtraidos {
  nomeCompleto: StatusExtracaoCampo;
  email: StatusExtracaoCampo;
  telefone: StatusExtracaoCampo;
  cargoInteresse: StatusExtracaoCampo;
  resumoProfissional: StatusExtracaoCampo;
}

@Component({
  selector: 'app-cadastro',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    AppIconComponent,
    CieeButtonDirective,
    CieeBadgeDirective,
    CieeInputDirective,
    CieeCardComponent,
    CieeCardHeaderDirective,
    CieeCardTitleDirective,
    CieeCardDescriptionDirective,
    CieeCardContentDirective,
    CieeCardFooterDirective,
  ],
  templateUrl: './cadastro.component.html'
})
export class CadastroComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly candidatoService = inject(CandidatoService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  arquivoSelecionado = signal<File | null>(null);
  estaArrastando = signal<boolean>(false);
  extraindoPdf = signal<boolean>(false);
  salvando = signal<boolean>(false);
  teveOrigemPdf = signal<boolean>(false);

  mensagemPdf = signal<string | null>(null);
  tipoMensagemPdf = signal<'sucesso' | 'aviso'>('sucesso');
  mensagemErro = signal<string | null>(null);

  private dragCounter = 0;
  private bloqueioCliqueDrop = false;

  // Ouvintes nativos globais para impedir que o navegador abra o PDF ao ser arrastado na tela
  private readonly windowDragOverHandler = (e: DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
  };

  private readonly windowDropHandler = (e: DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
  };

  statusCampos = signal<MapaCamposExtraidos>({
    nomeCompleto: 'manual',
    email: 'manual',
    telefone: 'manual',
    cargoInteresse: 'manual',
    resumoProfissional: 'manual'
  });

  form: FormGroup = this.fb.group({
    nomeCompleto: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    telefone: [''],
    cargoInteresse: [''],
    resumoProfissional: ['']
  });

  ngOnInit() {
    if (typeof window !== 'undefined') {
      window.addEventListener('dragover', this.windowDragOverHandler, false);
      window.addEventListener('drop', this.windowDropHandler, false);
      this.destroyRef.onDestroy(() => {
        window.removeEventListener('dragover', this.windowDragOverHandler);
        window.removeEventListener('drop', this.windowDropHandler);
      });
    }

    Object.keys(this.form.controls).forEach((nomeControle) => {
      const controle = this.form.get(nomeControle);
      if (controle) {
        controle.valueChanges
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe(() => {
            if (controle.errors && controle.errors['erroServidor']) {
              const outrosErros = { ...controle.errors };
              delete outrosErros['erroServidor'];
              const temErrosRestantes = Object.keys(outrosErros).length > 0;
              controle.setErrors(temErrosRestantes ? outrosErros : null);
            }
          });
      }
    });
  }

  campoInvalido(campo: string): boolean {
    const control = this.obterControleNormalizado(campo);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }

  obterMensagemErroCampo(campo: string): string | null {
    const control = this.obterControleNormalizado(campo);
    if (!control || !control.errors || !(control.dirty || control.touched)) {
      return null;
    }

    if (control.errors['erroServidor']) {
      return control.errors['erroServidor'];
    }

    if (campo === 'nomeCompleto') {
      if (control.errors['required']) return 'Nome completo é obrigatório.';
      if (control.errors['minlength']) return 'O nome deve ter no mínimo 3 caracteres.';
    }

    if (campo === 'email') {
      if (control.errors['required']) return 'E-mail é obrigatório.';
      if (control.errors['email']) return 'Informe um e-mail válido.';
    }

    return 'Campo inválido.';
  }

  aplicarMascaraTelefone(valor: string): string {
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
  }

  onTelefoneInput(event?: Event) {
    const input = event ? (event.target as HTMLInputElement) : null;
    if (input) {
      const valorFormatado = this.aplicarMascaraTelefone(input.value);
      this.form.get('telefone')?.setValue(valorFormatado, { emitEvent: false });
      input.value = valorFormatado;
    }
  }

  pararPropagacao(event?: Event) {
    event?.stopPropagation();
  }

  /**
   * CQ-01: Disparo estrito de upload exclusivamente via teclas Enter e Espaço.
   * Tab, Shift, Ctrl, Alt, Escape ou setas navegam normalmente sem abrir o arquivo.
   */
      acionarSelecaoArquivoManual() {
    const input = document.getElementById('input-curriculo-pdf') as HTMLInputElement;
    input?.click();
  }

  acionarUploadPorTeclado(event?: KeyboardEvent) {
    if (!event) return;
    const tecla = event.key;
    if (tecla === 'Enter' || tecla === ' ' || tecla === 'Spacebar') {
      event.preventDefault();
      event.stopPropagation();
      const input = document.getElementById('input-curriculo-pdf') as HTMLInputElement;
      input?.click();
    }
  }

  /**
   * CQ-04: Drag-and-Drop imune a flicker e com bloqueio total do comportamento padrão do browser.
   */
  aoEntrarArrasto(event?: DragEvent) {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    this.dragCounter++;
    if (this.dragCounter > 0) {
      this.estaArrastando.set(true);
    }
  }

  aoArrastarSobre(event?: DragEvent) {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
      if (event.dataTransfer) {
        event.dataTransfer.dropEffect = 'copy';
      }
    }
    this.estaArrastando.set(true);
  }

  aoSairArrasto(event?: DragEvent) {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    this.dragCounter--;
    if (this.dragCounter <= 0) {
      this.dragCounter = 0;
      this.estaArrastando.set(false);
    }
  }

  aoSoltarArquivo(event?: DragEvent) {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
        this.dragCounter = 0;
    this.estaArrastando.set(false);
    this.bloqueioCliqueDrop = true;
    setTimeout(() => {
      this.bloqueioCliqueDrop = false;
    }, 300);

    let arq: File | null = null;
    if (event && event.dataTransfer) {
      if (event.dataTransfer.files && event.dataTransfer.files.length > 0) {
        arq = event.dataTransfer.files[0];
      } else if (event.dataTransfer.items && event.dataTransfer.items.length > 0) {
        for (let i = 0; i < event.dataTransfer.items.length; i++) {
          const item = event.dataTransfer.items[i];
          if (item.kind === 'file') {
            const f = item.getAsFile();
            if (f) {
              arq = f;
              break;
            }
          }
        }
      }
    }
    if (arq) {
      this.processarArquivo(arq);
      const input = document.getElementById('input-curriculo-pdf') as HTMLInputElement | null;
      if (input) {
        input.value = '';
      }
    }
  }

  aoSelecionarArquivo(event?: Event) {
    if (this.extraindoPdf()) return;
    const input = event ? (event.target as HTMLInputElement) : (document.getElementById('input-curriculo-pdf') as HTMLInputElement);
    if (input && input.files && input.files.length > 0) {
      const arquivo = input.files[0];
      const arqAtual = this.arquivoSelecionado();
      if (arqAtual && arqAtual.name === arquivo.name && arqAtual.size === arquivo.size) {
        input.value = '';
        return;
      }
      this.processarArquivo(arquivo);
      input.value = '';
    }
  }

  processarArquivo(arquivo: File) {
    this.mensagemPdf.set(null);
    this.mensagemErro.set(null);
    this.teveOrigemPdf.set(false);

    const extensaoPdf = arquivo.name.toLowerCase().endsWith('.pdf');
    if (!extensaoPdf) {
      this.mensagemPdf.set('Arquivo inválido. Por favor, envie um documento no formato PDF.');
      this.tipoMensagemPdf.set('aviso');
      return;
    }

    const tamanhoMaximo = 5 * 1024 * 1024;
    if (arquivo.size > tamanhoMaximo) {
      this.mensagemPdf.set('O arquivo selecionado ultrapassa o limite máximo de 5 MB.');
      this.tipoMensagemPdf.set('aviso');
      return;
    }

    this.arquivoSelecionado.set(arquivo);
    this.extraindoPdf.set(true);

    this.candidatoService.extrairPdf(arquivo).subscribe({
      next: (res: ExtracaoPdfResponse) => {
        this.extraindoPdf.set(false);
        if (res.sucesso) {
          this.teveOrigemPdf.set(true);

          if (res.nomeCompleto) this.form.patchValue({ nomeCompleto: res.nomeCompleto });
          if (res.email) this.form.patchValue({ email: res.email });
          if (res.telefone) {
            const telFormatado = this.aplicarMascaraTelefone(res.telefone);
            this.form.patchValue({ telefone: telFormatado });
          }
          if (res.cargoInteresse) this.form.patchValue({ cargoInteresse: res.cargoInteresse });
          if (res.resumoProfissional) this.form.patchValue({ resumoProfissional: res.resumoProfissional });

          const mapa: MapaCamposExtraidos = {
            nomeCompleto: res.nomeCompleto ? 'extraido' : 'nao_encontrado',
            email: res.email ? 'extraido' : 'nao_encontrado',
            telefone: res.telefone ? 'extraido' : 'nao_encontrado',
            cargoInteresse: res.cargoInteresse ? 'extraido' : 'nao_encontrado',
            resumoProfissional: res.resumoProfissional ? 'extraido' : 'nao_encontrado'
          };
          this.statusCampos.set(mapa);

          this.mensagemPdf.set('Dados extraídos com sucesso. Complete os dados faltantes destacados abaixo.');
          this.tipoMensagemPdf.set('sucesso');

          setTimeout(() => {
            const camposOrdem: (keyof MapaCamposExtraidos)[] = [
              'nomeCompleto', 
              'email', 
              'telefone', 
              'cargoInteresse', 
              'resumoProfissional'
            ];
            const pendente = camposOrdem.find((c) => mapa[c] === 'nao_encontrado');
            if (pendente) {
              const seletor = '[formControlName="' + pendente + '"]';
              const elemento = document.querySelector(seletor) as HTMLElement;
              if (elemento) {
                const posicaoY = elemento.getBoundingClientRect().top + window.scrollY - 100;
                window.scrollTo({
                  top: Math.max(0, posicaoY),
                  behavior: 'smooth'
                });

                setTimeout(() => {
                  elemento.focus({ preventScroll: true });
                }, 450);
              }
            }
          }, 150);
        } else {
          this.teveOrigemPdf.set(false);
          this.statusCampos.set({
            nomeCompleto: 'nao_encontrado',
            email: 'nao_encontrado',
            telefone: 'nao_encontrado',
            cargoInteresse: 'nao_encontrado',
            resumoProfissional: 'nao_encontrado'
          });
          this.mensagemPdf.set(res.mensagem || 'Não foi possível extrair dados automaticamente deste PDF.');
          this.tipoMensagemPdf.set('aviso');
        }
      },
      error: (err) => {
        this.extraindoPdf.set(false);
        this.teveOrigemPdf.set(false);
        const msg = err.error?.mensagem || 'Falha na comunicação com o servidor ao ler o arquivo PDF. O cadastro manual continua disponível.';
        this.mensagemPdf.set(msg);
        this.tipoMensagemPdf.set('aviso');
      }
    });
  }

  removerArquivo() {
    this.arquivoSelecionado.set(null);
    this.teveOrigemPdf.set(false);
    this.mensagemPdf.set(null);
    this.statusCampos.set({
      nomeCompleto: 'manual',
      email: 'manual',
      telefone: 'manual',
      cargoInteresse: 'manual',
      resumoProfissional: 'manual'
    });
  }

  obterControleNormalizado(nomeCampo: string): AbstractControl | null {
    if (!nomeCampo) return null;

    if (this.form.contains(nomeCampo)) {
      return this.form.get(nomeCampo);
    }

    const segmentoFinal = nomeCampo.includes('.') ? nomeCampo.split('.').pop()! : nomeCampo;
    const normalizar = (s: string) => s.replace(/[^a-zA-Z0-9]/g, '').toLowerCase();
    const termoNormalizado = normalizar(segmentoFinal);

    for (const key of Object.keys(this.form.controls)) {
      if (normalizar(key) === termoNormalizado) {
        return this.form.get(key);
      }
    }

    return null;
  }

  private mapearErrosBackendNoFormulario(error: unknown) {
    const listaErros = this.candidatoService.extrairErrosValidacao(error);
    if (listaErros.length === 0) {
      return;
    }

    for (const item of listaErros) {
      const controle = this.obterControleNormalizado(item.campo);
      if (controle) {
        const errosAtuais = controle.errors || {};
        controle.setErrors({ ...errosAtuais, erroServidor: item.erro });
        controle.markAsTouched();
      }
    }
  }

  salvar() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.mensagemErro.set('Por favor, preencha corretamente todos os campos obrigatórios destacados.');
      return;
    }

    this.salvando.set(true);
    this.mensagemErro.set(null);

    const telefoneFormatado = this.aplicarMascaraTelefone(this.form.value.telefone || '');

    const dados = {
      ...this.form.value,
      telefone: telefoneFormatado || null,
      teveOrigemPdf: this.teveOrigemPdf()
    };

    this.candidatoService.criar(dados).subscribe({
      next: () => {
        this.salvando.set(false);
        this.router.navigate(['/'], { queryParams: { salvo: 'sucesso' } });
      },
      error: (err) => {
        this.salvando.set(false);
        this.mapearErrosBackendNoFormulario(err);
        this.mensagemErro.set(
          err.error?.mensagem || 
          err.error?.detail || 
          'Ocorreu um erro ao salvar o candidato. Verifique os dados e tente novamente.'
        );
      }
    });
  }
}

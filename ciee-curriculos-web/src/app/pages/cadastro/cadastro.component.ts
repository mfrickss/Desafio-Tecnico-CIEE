import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CandidatoService } from '../../services/candidato.service';
import { ExtracaoPdfResponse } from '../../models/candidato.model';
import { HlmButtonDirective } from '../../shared/ui/button.directive';
import { HlmBadgeDirective } from '../../shared/ui/badge.directive';
import { HlmInputDirective } from '../../shared/ui/input.directive';
import { 
  HlmCardComponent, 
  HlmCardHeaderDirective, 
  HlmCardTitleDirective, 
  HlmCardDescriptionDirective, 
  HlmCardContentDirective, 
  HlmCardFooterDirective 
} from '../../shared/ui/card.components';
import { GsapFadeInDirective } from '../../shared/directives/gsap-animate.directive';

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
    HlmButtonDirective,
    HlmBadgeDirective,
    HlmInputDirective,
    HlmCardComponent,
    HlmCardHeaderDirective,
    HlmCardTitleDirective,
    HlmCardDescriptionDirective,
    HlmCardContentDirective,
    HlmCardFooterDirective,
    GsapFadeInDirective
  ],
  templateUrl: './cadastro.component.html'
})
export class CadastroComponent {
  private readonly fb = inject(FormBuilder);
  private readonly candidatoService = inject(CandidatoService);
  private readonly router = inject(Router);

  arquivoSelecionado = signal<File | null>(null);
  estaArrastando = signal<boolean>(false);
  extraindoPdf = signal<boolean>(false);
  salvando = signal<boolean>(false);

  mensagemPdf = signal<string | null>(null);
  tipoMensagemPdf = signal<'sucesso' | 'aviso'>('sucesso');
  mensagemErro = signal<string | null>(null);

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

  campoInvalido(campo: string): boolean {
    const control = this.form.get(campo);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }

  pararPropagacao(event?: Event) {
    event?.stopPropagation();
  }

  aoArrastarSobre(event?: DragEvent) {
    event?.preventDefault();
    event?.stopPropagation();
    this.estaArrastando.set(true);
  }

  aoSairArrasto(event?: DragEvent) {
    event?.preventDefault();
    event?.stopPropagation();
    this.estaArrastando.set(false);
  }

  aoSoltarArquivo(event?: DragEvent) {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
      this.estaArrastando.set(false);
      if (event.dataTransfer && event.dataTransfer.files && event.dataTransfer.files.length > 0) {
        this.processarArquivo(event.dataTransfer.files[0]);
      }
    }
  }

  aoSelecionarArquivo(event?: Event) {
    const input = event ? (event.target as HTMLInputElement) : (document.querySelector('input[type="file"]') as HTMLInputElement);
    if (input && input.files && input.files.length > 0) {
      const arquivo = input.files[0];
      this.processarArquivo(arquivo);
      input.value = '';
    }
  }

  processarArquivo(arquivo: File) {
    this.mensagemPdf.set(null);
    this.mensagemErro.set(null);

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
          if (res.nomeCompleto) this.form.patchValue({ nomeCompleto: res.nomeCompleto });
          if (res.email) this.form.patchValue({ email: res.email });
          if (res.telefone) this.form.patchValue({ telefone: res.telefone });
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

          // Rolagem verdadeiramente suave até o 1º campo não encontrado
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
                // Cálculo de posição com desconto da barra sticky (80px de margem)
                const posicaoY = elemento.getBoundingClientRect().top + window.scrollY - 100;
                window.scrollTo({
                  top: Math.max(0, posicaoY),
                  behavior: 'smooth'
                });

                // Foco após a conclusão da rolagem para não interromper a animação suave
                setTimeout(() => {
                  elemento.focus({ preventScroll: true });
                }, 450);
              }
            }
          }, 150);
        } else {
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
        const msg = err.error?.mensagem || 'Falha na comunicação com o servidor ao ler o arquivo PDF. O cadastro manual continua disponível.';
        this.mensagemPdf.set(msg);
        this.tipoMensagemPdf.set('aviso');
      }
    });
  }

  removerArquivo() {
    this.arquivoSelecionado.set(null);
    this.mensagemPdf.set(null);
    this.statusCampos.set({
      nomeCompleto: 'manual',
      email: 'manual',
      telefone: 'manual',
      cargoInteresse: 'manual',
      resumoProfissional: 'manual'
    });
  }

  salvar() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.mensagemErro.set('Por favor, preencha corretamente todos os campos obrigatórios destacados.');
      return;
    }

    this.salvando.set(true);
    this.mensagemErro.set(null);

    const dados = {
      ...this.form.value,
      teveOrigemPdf: !!this.arquivoSelecionado()
    };

    this.candidatoService.criar(dados).subscribe({
      next: () => {
        this.salvando.set(false);
        this.router.navigate(['/'], { queryParams: { salvo: 'sucesso' } });
      },
      error: (err) => {
        this.salvando.set(false);
        this.mensagemErro.set(err.error?.mensagem || 'Ocorreu um erro ao salvar o candidato. Verifique os dados e tente novamente.');
      }
    });
  }
}

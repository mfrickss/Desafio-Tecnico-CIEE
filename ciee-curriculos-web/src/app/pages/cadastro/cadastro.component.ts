import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CandidatoService } from '../../services/candidato.service';

@Component({
  selector: 'app-cadastro',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
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

  aoArrastarSobre(event?: DragEvent) {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    this.estaArrastando.set(true);
  }

  aoSairArrasto(event?: DragEvent) {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    this.estaArrastando.set(false);
  }

  aoSoltarArquivo(event?: DragEvent) {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
      this.estaArrastando.set(false);
      if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
        this.processarArquivo(event.dataTransfer.files[0]);
      }
    }
  }

  aoSelecionarArquivo(event?: Event) {
    const input = event?.target as HTMLInputElement;
    if (input?.files && input.files.length > 0) {
      const arquivo = input.files[0];
      this.processarArquivo(arquivo);
      input.value = ''; // Reseta para permitir selecionar o mesmo arquivo novamente
    }
  }

  processarArquivo(arquivo: File) {
    this.mensagemPdf.set(null);
    this.mensagemErro.set(null);

    console.log('[Upload PDF] Arquivo selecionado:', arquivo.name, arquivo.size, 'bytes', arquivo.type);

    const extensaoPdf = arquivo.name.toLowerCase().endsWith('.pdf');
    if (!extensaoPdf) {
      this.mensagemPdf.set('Arquivo inválido. Por favor, envie um documento no formato PDF.');
      this.tipoMensagemPdf.set('aviso');
      return;
    }

    const tamanhoMaximo = 5 * 1024 * 1024; // 5 MB
    if (arquivo.size > tamanhoMaximo) {
      this.mensagemPdf.set('O arquivo selecionado ultrapassa o limite máximo de 5 MB.');
      this.tipoMensagemPdf.set('aviso');
      return;
    }

    this.arquivoSelecionado.set(arquivo);
    this.extraindoPdf.set(true);

    console.log('[Upload PDF] Enviando FormData para POST /api/candidatos/extrair-pdf...');

    this.candidatoService.extrairPdf(arquivo).subscribe({
      next: (res) => {
        console.log('[Upload PDF] Resposta do backend recebida:', res);
        this.extraindoPdf.set(false);
        if (res.sucesso) {
          if (res.nomeCompleto) this.form.patchValue({ nomeCompleto: res.nomeCompleto });
          if (res.email) this.form.patchValue({ email: res.email });
          if (res.telefone) this.form.patchValue({ telefone: res.telefone });
          if (res.cargoInteresse) this.form.patchValue({ cargoInteresse: res.cargoInteresse });
          if (res.resumoProfissional) this.form.patchValue({ resumoProfissional: res.resumoProfissional });

          this.mensagemPdf.set('Dados extraídos do PDF e preenchidos no formulário. Você pode revisá-los ou complementá-los antes de salvar.');
          this.tipoMensagemPdf.set('sucesso');
        } else {
          this.mensagemPdf.set(res.mensagem || 'Não foi possível extrair dados automaticamente deste PDF. Você pode preencher o formulário manualmente.');
          this.tipoMensagemPdf.set('aviso');
        }
      },
      error: (err) => {
        console.error('[Upload PDF] Erro na requisição:', err);
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

    console.log('[Cadastro] Enviando payload:', dados);

    this.candidatoService.criar(dados).subscribe({
      next: (resp) => {
        console.log('[Cadastro] Candidato salvo com sucesso:', resp);
        this.salvando.set(false);
        this.router.navigate(['/'], { queryParams: { salvo: 'sucesso' } });
      },
      error: (err) => {
        console.error('[Cadastro] Erro ao salvar candidato:', err);
        this.salvando.set(false);
        this.mensagemErro.set(err.error?.mensagem || 'Ocorreu um erro ao salvar o candidato. Verifique os dados e tente novamente.');
      }
    });
  }
}

import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CandidatoService } from '../../services/candidato.service';
import { Candidato } from '../../models/candidato.model';

@Component({
  selector: 'app-listagem',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './listagem.component.html'
})
export class ListagemComponent implements OnInit {
  private readonly candidatoService = inject(CandidatoService);
  private readonly route = inject(ActivatedRoute);

  candidatos = signal<Candidato[]>([]);
  carregando = signal<boolean>(true);
  termoBusca = '';
  alertaSucesso = signal<boolean>(false);

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      if (params['salvo'] === 'sucesso') {
        this.alertaSucesso.set(true);
      }
    });

    this.carregarCandidatos();
  }

  carregarCandidatos() {
    this.carregando.set(true);
    this.candidatoService.listar(this.termoBusca).subscribe({
      next: (dados) => {
        this.candidatos.set(dados);
        this.carregando.set(false);
      },
      error: (err) => {
        console.error('Erro ao listar candidatos:', err);
        this.carregando.set(false);
      }
    });
  }

  fecharAlerta() {
    this.alertaSucesso.set(false);
  }

  buscar() {
    this.carregarCandidatos();
  }

  limparBusca() {
    this.termoBusca = '';
    this.carregarCandidatos();
  }
}

import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { CandidatoService } from '../../services/candidato.service';
import { Candidato } from '../../models/candidato.model';
import { AppIconComponent } from '../../shared/ui/icon.component';
import { CieeButtonDirective } from '../../shared/ui/button.directive';
import { CieeBadgeDirective } from '../../shared/ui/badge.directive';
import { 
  CieeCardComponent, 
  CieeCardHeaderDirective, 
  CieeCardTitleDirective, 
  CieeCardDescriptionDirective, 
  CieeCardContentDirective 
} from '../../shared/ui/card.components';

@Component({
  selector: 'app-detalhes',
  standalone: true,
  imports: [
    CommonModule, 
    RouterLink,
    AppIconComponent,
    CieeButtonDirective,
    CieeBadgeDirective,
    CieeCardComponent,
    CieeCardHeaderDirective,
    CieeCardTitleDirective,
    CieeCardDescriptionDirective,
    CieeCardContentDirective,
  ],
  templateUrl: './detalhes.component.html'
})
export class DetalhesComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly candidatoService = inject(CandidatoService);

  candidato = signal<Candidato | null>(null);
  carregando = signal<boolean>(true);
  erroNaoEncontrado = signal<boolean>(false);
  erroConexao = signal<boolean>(false);
  idCandidato = signal<string | null>(null);

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    this.idCandidato.set(id);
    this.carregar();
  }

  carregar() {
    const id = this.idCandidato();
    if (!id) {
      this.erroNaoEncontrado.set(true);
      this.carregando.set(false);
      return;
    }

    this.carregando.set(true);
    this.erroNaoEncontrado.set(false);
    this.erroConexao.set(false);

    this.candidatoService.obterPorId(id).subscribe({
      next: (dados) => {
        this.candidato.set(dados);
        this.carregando.set(false);
      },
      error: (err: unknown) => {
        this.carregando.set(false);
        if (err instanceof HttpErrorResponse && err.status === 404) {
          this.erroNaoEncontrado.set(true);
        } else {
          this.erroConexao.set(true);
        }
      }
    });
  }
}

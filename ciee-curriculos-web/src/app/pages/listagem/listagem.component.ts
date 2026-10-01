import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { CandidatoService } from '../../services/candidato.service';
import { Candidato } from '../../models/candidato.model';
import { HlmButtonDirective } from '../../shared/ui/button.directive';
import { HlmBadgeDirective } from '../../shared/ui/badge.directive';
import { HlmInputDirective } from '../../shared/ui/input.directive';
import { 
  HlmTableComponent, 
  HlmTableHeaderDirective, 
  HlmTableBodyDirective, 
  HlmTableRowDirective, 
  HlmTableHeadDirective, 
  HlmTableCellDirective 
} from '../../shared/ui/table.components';
import { GsapFadeInDirective } from '../../shared/directives/gsap-animate.directive';

@Component({
  selector: 'app-listagem',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    RouterLink,
    HlmButtonDirective,
    HlmBadgeDirective,
    HlmInputDirective,
    HlmTableComponent,
    HlmTableHeaderDirective,
    HlmTableBodyDirective,
    HlmTableRowDirective,
    HlmTableHeadDirective,
    HlmTableCellDirective,
    GsapFadeInDirective
  ],
  templateUrl: './listagem.component.html'
})
export class ListagemComponent implements OnInit {
  private readonly candidatoService = inject(CandidatoService);
  private readonly route = inject(ActivatedRoute);

  candidatos = signal<Candidato[]>([]);
  carregando = signal<boolean>(true);
  alertaSucesso = signal<boolean>(false);
  termoBusca: string = '';

  ngOnInit() {
    this.carregar();
    this.route.queryParams.subscribe(params => {
      if (params['salvo'] === 'sucesso') {
        this.alertaSucesso.set(true);
      }
    });
  }

  carregar() {
    this.carregando.set(true);
    this.candidatoService.listar(this.termoBusca).subscribe({
      next: (dados) => {
        this.candidatos.set(dados);
        this.carregando.set(false);
      },
      error: () => {
        this.carregando.set(false);
      }
    });
  }

  buscar() {
    this.carregar();
  }

  limparBusca() {
    this.termoBusca = '';
    this.carregar();
  }

  fecharAlerta() {
    this.alertaSucesso.set(false);
  }
}

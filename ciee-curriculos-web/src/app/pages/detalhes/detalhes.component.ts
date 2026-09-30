import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CandidatoService } from '../../services/candidato.service';
import { Candidato } from '../../models/candidato.model';

@Component({
  selector: 'app-detalhes',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './detalhes.component.html'
})
export class DetalhesComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly candidatoService = inject(CandidatoService);

  candidato = signal<Candidato | null>(null);
  carregando = signal<boolean>(true);
  erro = signal<boolean>(false);

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.erro.set(true);
      this.carregando.set(false);
      return;
    }

    this.candidatoService.obterPorId(id).subscribe({
      next: (dados) => {
        this.candidato.set(dados);
        this.carregando.set(false);
      },
      error: () => {
        this.erro.set(true);
        this.carregando.set(false);
      }
    });
  }
}

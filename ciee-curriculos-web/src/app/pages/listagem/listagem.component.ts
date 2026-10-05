import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Subject, Subscription, EMPTY } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, catchError } from 'rxjs/operators';
import { CandidatoService } from '../../services/candidato.service';
import { Candidato } from '../../models/candidato.model';
import { AppIconComponent } from '../../shared/ui/icon.component';
import { CieeButtonDirective } from '../../shared/ui/button.directive';
import { CieeBadgeDirective } from '../../shared/ui/badge.directive';
import { CieeInputDirective } from '../../shared/ui/input.directive';
import { 
  CieeTableComponent, 
  CieeTableHeaderDirective, 
  CieeTableBodyDirective, 
  CieeTableRowDirective, 
  CieeTableHeadDirective, 
  CieeTableCellDirective 
} from '../../shared/ui/table.components';

@Component({
  selector: 'app-listagem',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    RouterLink,
    AppIconComponent,
    CieeButtonDirective,
    CieeBadgeDirective,
    CieeInputDirective,
    CieeTableComponent,
    CieeTableHeaderDirective,
    CieeTableBodyDirective,
    CieeTableRowDirective,
    CieeTableHeadDirective,
    CieeTableCellDirective,
  ],
  templateUrl: './listagem.component.html'
})
export class ListagemComponent implements OnInit, OnDestroy {
  private readonly candidatoService = inject(CandidatoService);
  private readonly route = inject(ActivatedRoute);

  candidatos = signal<Candidato[]>([]);
  carregando = signal<boolean>(true);
  erroConexao = signal<boolean>(false);
  tituloErro = signal<string>('Serviço Indisponível');
  descricaoErro = signal<string>('Não foi possível conectar ao servidor.');
  alertaSucesso = signal<boolean>(false);
  termoBusca: string = '';

  private readonly buscaSubject = new Subject<string>();
  private buscaSub?: Subscription;

  ngOnInit() {
    this.carregar();

    this.buscaSub = this.buscaSubject
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((termo) => {
          this.carregando.set(true);
          return this.candidatoService.listar(termo).pipe(
            catchError((err: unknown) => {
              this.tratarFalhaConexao(err);
              return EMPTY;
            })
          );
        })
      )
      .subscribe({
        next: (dados) => {
          this.erroConexao.set(false);
          this.candidatos.set(dados);
          this.carregando.set(false);
        }
      });

    this.route.queryParams.subscribe(params => {
      if (params['salvo'] === 'sucesso') {
        this.alertaSucesso.set(true);
      }
    });
  }

  ngOnDestroy() {
    this.buscaSub?.unsubscribe();
  }

  carregar() {
    this.carregando.set(true);
    this.erroConexao.set(false);
    this.candidatoService.listar(this.termoBusca).pipe(
      catchError((err: unknown) => {
        this.tratarFalhaConexao(err);
        return EMPTY;
      })
    ).subscribe({
      next: (dados) => {
        this.erroConexao.set(false);
        this.candidatos.set(dados);
        this.carregando.set(false);
      }
    });
  }

  tratarFalhaConexao(err: unknown) {
    this.carregando.set(false);
    this.erroConexao.set(true);
    this.candidatos.set([]);

    if (err instanceof HttpErrorResponse && err.status === 0) {
      this.tituloErro.set('Serviço Indisponível');
      this.descricaoErro.set(
        'Não foi possível estabelecer conexão com o servidor. ' +
        'Verifique sua conexão de rede ou tente novamente em instantes.'
      );
    } else if (err instanceof HttpErrorResponse && (err.status === 503 || err.status === 502 || err.status === 504)) {
      this.tituloErro.set('Serviço Temporariamente Indisponível');
      this.descricaoErro.set('O servidor está temporariamente fora de operação. Tente novamente em instantes.');
    } else {
      this.tituloErro.set('Falha na comunicação com o servidor');
      this.descricaoErro.set('Não foi possível carregar a lista de candidatos devido a uma instabilidade de conexão com o servidor.');
    }
  }

  aoDigitarBusca(termo?: string) {
    const valor = termo ?? this.termoBusca ?? '';
    this.termoBusca = valor;
    this.buscaSubject.next(valor);
  }

  limparBusca() {
    this.termoBusca = '';
    this.buscaSubject.next('');
    this.carregar();
  }

  fecharAlerta() {
    this.alertaSucesso.set(false);
  }
}

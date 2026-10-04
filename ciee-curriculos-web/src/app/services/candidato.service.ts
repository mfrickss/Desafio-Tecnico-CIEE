import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Candidato, CriarCandidatoDto, ExtracaoPdfResponse } from '../models/candidato.model';
import { ErroValidacaoCampo, ApiErroValidacaoResponse } from '../models/api-error.model';

@Injectable({
  providedIn: 'root'
})
export class CandidatoService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5004/api/candidatos';

  listar(busca?: string): Observable<Candidato[]> {
    const params: { [key: string]: string } = {};
    if (busca && busca.trim().length > 0) {
      params['busca'] = busca.trim();
    }
    return this.http.get<Candidato[]>(this.apiUrl, { params });
  }

  obterPorId(id: string): Observable<Candidato> {
    return this.http.get<Candidato>(this.apiUrl + '/' + id);
  }

  criar(dto: CriarCandidatoDto): Observable<Candidato> {
    return this.http.post<Candidato>(this.apiUrl, dto);
  }

  extrairPdf(arquivo: File): Observable<ExtracaoPdfResponse> {
    const formData = new FormData();
    formData.append('arquivo', arquivo);
    return this.http.post<ExtracaoPdfResponse>(this.apiUrl + '/extrair-pdf', formData);
  }

  extrairErrosValidacao(error: unknown): ErroValidacaoCampo[] {
    if (!(error instanceof HttpErrorResponse) || !error.error) {
      return [];
    }

    const body = error.error as ApiErroValidacaoResponse;
    const listaErros: ErroValidacaoCampo[] = [];

    if (Array.isArray(body.erros) && body.erros.length > 0) {
      for (const item of body.erros) {
        if (item.campo && item.erro) {
          listaErros.push({ campo: item.campo, erro: item.erro });
        }
      }
      return listaErros;
    }

    if (body.errors && typeof body.errors === 'object') {
      for (const [campo, mensagens] of Object.entries(body.errors)) {
        if (Array.isArray(mensagens)) {
          for (const msg of mensagens) {
            listaErros.push({ campo, erro: msg });
          }
        }
      }
    }

    return listaErros;
  }
}
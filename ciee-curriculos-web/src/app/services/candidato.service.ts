import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Candidato, CriarCandidatoDto, ExtracaoPdfResponse } from '../models/candidato.model';

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
}

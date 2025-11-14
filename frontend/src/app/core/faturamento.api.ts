import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface NotaItem {
  id: number;
  produtoId: number;
  quantidade: number;
  preco: number;
}

export interface Nota {
  id: number;
  numero: number;
  status: string;
  itens: NotaItem[];
}

export interface CriarNotaItemDto {
  produtoId: number;
  quantidade: number;
  preco: number;
}

export interface CriarNotaDto {
  itens: CriarNotaItemDto[];
}

export interface AtualizarNotaDto {
  itens: CriarNotaItemDto[];
}

@Injectable({ providedIn: 'root' })
export class FaturamentoApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBase}/api/faturamento/notas`;

  listarNotas(q?: string): Observable<Nota[]> {
    const params: any = {};
    if (q && q.trim() !== '') {
      params.q = q.trim();
    }

    return this.http
      .get<{ notas: Nota[] }>(this.baseUrl, { params })
      .pipe(map(r => r.notas));
  }

  imprimirNota(id: number): Observable<void> {
    const url = `${this.baseUrl}/${id}/impressao`;
    return this.http.post<void>(url, {});
  }


  obterNota(id: number): Observable<Nota> {
    return this.http.get<Nota>(`${this.baseUrl}/${id}`);
  }

  criarNota(dto: CriarNotaDto): Observable<{ id: number; status: string }> {
    return this.http.post<{ id: number; status: string }>(this.baseUrl, dto);
  }

  atualizarNota(id: number, dto: AtualizarNotaDto): Observable<{ id: number; status: string }> {
    return this.http.put<{ id: number; status: string }>(
      `${this.baseUrl}/${id}`,
      dto
    );
  }

  deletarNota(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
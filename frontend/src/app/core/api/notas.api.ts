import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface NotaItemCreateDto {
  produtoId: number;
  quantidade: number;
  preco: number;
}

export interface NotaCreateDto {
  itens: NotaItemCreateDto[];
}

export interface NotaItemReadDto {
  id: number;
  produtoId: number;
  quantidade: number;
  preco: number;
}

export interface NotaItemDto {
  produtoId: number;
  quantidade: number;
  preco: number;
}

export interface NotaDto {
  id: number;
  numero: string | null;
  status: string;
  itens: NotaItemDto[];
}

export interface NotaReadDto {
  id: number;
  numero?: number | null;
  status: string;
  itens?: NotaItemReadDto[];
}

export interface Page<T> {
  items: T[];
  total: number;
  page?: number;
  size?: number;
}

@Injectable({ providedIn: 'root' })
export class NotasApi {
  private http = inject(HttpClient);
  private base = environment.apiBase;

  list(q = '', page = 1, size = 20, status?: string): Observable<Page<NotaReadDto>> {
    let params = new HttpParams().set('page', page).set('size', size);
    if (q) params = params.set('q', q);
    if (status) params = params.set('status', status);
    return this.http.get<Page<NotaReadDto>>(`${this.base}/api/faturamento/notas`, { params });
  }

  getById(id: number): Observable<NotaReadDto> {
    return this.http.get<NotaReadDto>(`${this.base}/api/faturamento/notas/${id}`);
  }

  create(dto: NotaCreateDto): Observable<NotaReadDto> {
    return this.http.post<NotaReadDto>(`${this.base}/api/faturamento/notas`, dto);
  }

  update(id: number, dto: { itens: Array<{ id?: number; produtoId: number; quantidade: number; preco: number }> }) {
    return this.http.put<NotaReadDto>(`${this.base}/api/faturamento/notas/${id}`, dto);
  }

  imprimir(id: number, idemKey?: string) {
    let headers = new HttpHeaders();
    if (idemKey) headers = headers.set('Idempotency-Key', idemKey);
    return this.http.post(`${this.base}/api/faturamento/notas/${id}/impressao`, {}, { headers });
  }
}

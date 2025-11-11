import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface ProdutoDto {
  id: number;
  codigo: string;
  descricao: string;
  saldo: number;
}

export interface Page<T> {
  items: T[];
  total: number;
}

@Injectable({ providedIn: 'root' })
export class ProdutosApi {
  private http = inject(HttpClient);
  private base = environment.apiBase;

  list(q = '', page = 1, size = 10): Observable<Page<ProdutoDto>> {
    let params = new HttpParams()
      .set('page', page)
      .set('size', size);
    if (q) params = params.set('q', q);

    return this.http.get<Page<ProdutoDto>>(`${this.base}/api/estoque/produtos`, { params });
  }

  create(dto: { codigo: string; descricao: string; saldo: number }) {
    return this.http.post<ProdutoDto>(`${this.base}/api/estoque/produtos`, dto);
  }
}

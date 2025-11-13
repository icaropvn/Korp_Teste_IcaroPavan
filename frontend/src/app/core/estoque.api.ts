import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';

export interface Produto {
  id: number;
  codigo: string;
  descricao: string;
  saldo: number;
}

export interface CriarProdutoDto {
  descricao: string;
  saldo: number;
}

@Injectable({ providedIn: 'root' })  
export class EstoqueApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5000/api/estoque/produtos';

  listarProdutos(): Observable<Produto[]> {
    return this.http.get<{produtos: Produto[]}>(this.baseUrl).pipe(
      map(res => res.produtos)
    );
  }

  criarProduto(dto: CriarProdutoDto): Observable<Produto> {
    return this.http.post<Produto>(this.baseUrl, dto);
  }

  obterProduto(id: number): Observable<Produto> {
    return this.http.get<Produto>(`${this.baseUrl}/${id}`);
  }

  atualizarProduto(id: number, dto: Partial<CriarProdutoDto>): Observable<Produto> {
    return this.http.put<Produto>(`${this.baseUrl}/${id}`, dto);
  }

  deletarProduto(id: number): Observable<{message: string}> {
    return this.http.delete<{ message: string }>(`${this.baseUrl}/${id}`);
  }
}
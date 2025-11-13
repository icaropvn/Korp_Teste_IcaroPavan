// products-page.component.ts
import { Component } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatTableDataSource } from '@angular/material/table';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { ProdutoAddDialogComponent } from '../../components/produtos-add-dialog/produtos-add-dialog.component';
import { EstoqueApi, Produto, CriarProdutoDto } from '../../../core/estoque.api';
import { ProdutoEditDialogComponent } from '../../components/produtos-edit-dialog/produtos-edit-dialog.component';

@Component({
  selector: 'app-products-page',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatTableModule,
    MatDialogModule
  ],
  templateUrl: './produtos-page.component.html',
  styleUrls: ['./produtos-page.component.css']
})

export class ProdutosPageComponent {
  cols = ['codigo', 'descricao', 'saldo'];
  dataSource = new MatTableDataSource<Produto>([]);

  constructor(
    private dialog: MatDialog,
    private estoqueApi: EstoqueApi
  ) {}

  ngOnInit(): void {
    this.carregarProdutos();
  }

  get hasFilter() {
    return !!this.dataSource.filter;
  }

  applyFilter(event: Event) {
    const value = (event.target as HTMLInputElement).value ?? '';
    this.dataSource.filter = value.trim().toLowerCase();
    this.dataSource.filterPredicate = (data, filter) =>
      data.codigo.toLowerCase().includes(filter) ||
      data.descricao.toLowerCase().includes(filter) ||
      String(data.saldo).includes(filter);
  }

  clearFilter() {
    this.dataSource.filter = '';
  }

  private carregarProdutos(): void {
    this.estoqueApi.listarProdutos().subscribe({
      next: (produtos: Produto[]) => {
        console.log(produtos);
        this.dataSource.data = produtos;
      },
      error: (err: unknown) => {
        console.error('Erro ao carregar produtos', err);
        this.dataSource.data = [];
      }
    });
  }

  novoProduto() {
    const ref = this.dialog.open(ProdutoAddDialogComponent, {
      width: '420px'
    });

    ref.afterClosed().subscribe((produto: Produto | null) => {
      if (!produto) return;

      this.carregarProdutos();
    });
  }

  editarProduto(produto: Produto) {
    const ref = this.dialog.open(ProdutoEditDialogComponent, {
      width: '420px',
      disableClose: true,
      data: produto
    });

    ref.afterClosed().subscribe((result) => {
      if (!result) return;

      this.carregarProdutos();
    });
  }
}
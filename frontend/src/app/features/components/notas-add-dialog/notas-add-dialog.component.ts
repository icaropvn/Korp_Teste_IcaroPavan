import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { EstoqueApi, Produto } from '../../../core/estoque.api';
import { FaturamentoApi, CriarNotaDto, CriarNotaItemDto } from '../../../core/faturamento.api';

interface ItemNotaForm {
  produtoId: number;
  produtoDescricao: string;
  quantidade: number;
  preco: number;
}

@Component({
  selector: 'app-notas-add-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatTableModule,
    MatIconModule,
    ReactiveFormsModule,
    MatSnackBarModule
  ],
  templateUrl: './notas-add-dialog.component.html',
  styleUrl: './notas-add-dialog.component.css'
})
export class NotasAddDialogComponent implements OnInit {
  itemForm: FormGroup;
  loadingProdutos = false;
  criandoNota = false;

  produtos: Produto[] = [];
  displayedColumns: string[] = ['produto', 'quantidade', 'preco', 'acoes'];
  itens: ItemNotaForm[] = [];
  dataSource = new MatTableDataSource<ItemNotaForm>([]);

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<NotasAddDialogComponent>,
    private estoqueApi: EstoqueApi,
    private faturamentoApi: FaturamentoApi,
    private snack: MatSnackBar
  ) {
    this.itemForm = this.fb.group({
      produtoId: [null, [Validators.required]],
      quantidade: [1, [Validators.required, Validators.min(1)]],
      preco: [0, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.carregarProdutos();
  }

  carregarProdutos(): void {
    this.loadingProdutos = true;

    this.estoqueApi.listarProdutos().subscribe({
      next: produtos => {
        this.produtos = produtos.filter(p => p.saldo > 0);
        this.loadingProdutos = false;
      },
      error: err => {
        console.error('Erro ao carregar produtos do estoque', err);
        this.loadingProdutos = false;
        this.snack.open(
          'Erro ao carregar produtos do estoque.',
          'Fechar',
          { duration: 4000 }
        );
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close(null);
  }

  adicionarItem(): void {
    if (this.itemForm.invalid) {
      this.itemForm.markAllAsTouched();
      return;
    }

    const produtoId = this.itemForm.value.produtoId as number;
    const quantidade = this.itemForm.value.quantidade as number;
    const preco = this.itemForm.value.preco as number;

    const produto = this.produtos.find(p => p.id === produtoId);
    if (!produto) {
      this.snack.open('Produto inválido.', 'Fechar', { duration: 3000 });
      return;
    }

    const item: ItemNotaForm = {
      produtoId,
      produtoDescricao: produto.descricao,
      quantidade,
      preco
    };

    this.itens = [...this.itens, item];
    this.dataSource.data = this.itens;

    this.itemForm.patchValue({
      quantidade: 1,
      preco: 0
    });
  }

  removerItem(index: number): void {
    this.itens = this.itens.filter((_, i) => i !== index);
    this.dataSource.data = this.itens;
  }

  criarNota(): void {
    if (this.itens.length === 0) {
      this.snack.open(
        'Adicione pelo menos um item à nota.',
        'Fechar',
        { duration: 4000 }
      );
      return;
    }

    const dto: CriarNotaDto = {
      itens: this.itens.map<CriarNotaItemDto>(i => ({
        produtoId: i.produtoId,
        quantidade: i.quantidade,
        preco: i.preco
      }))
    };

    this.criandoNota = true;

    this.faturamentoApi.criarNota(dto).subscribe({
      next: resposta => {
        this.criandoNota = false;
        this.snack.open('Nota fiscal criada com sucesso!', 'OK', {
          duration: 3000
        });
        this.dialogRef.close(resposta);
      },
      error: err => {
        console.error('Erro ao criar nota', err);
        this.criandoNota = false;

        const msg =
          err?.error?.message ||
          err?.error?.title ||
          'Erro ao criar nota fiscal.';
        this.snack.open(msg, 'Fechar', { duration: 5000 });
      }
    });
  }
}
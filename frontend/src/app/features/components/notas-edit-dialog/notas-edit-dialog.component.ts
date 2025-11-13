import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { Nota, AtualizarNotaDto, CriarNotaItemDto, FaturamentoApi } from '../../../core/faturamento.api';
import { EstoqueApi, Produto } from '../../../core/estoque.api';

interface ItemNotaForm {
  produtoId: number;
  produtoDescricao: string;
  quantidade: number;
  preco: number;
}

@Component({
  selector: 'app-notas-edit-dialog',
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
  templateUrl: './notas-edit-dialog.component.html',
  styleUrl: './notas-edit-dialog.component.css'
})
export class NotasEditDialogComponent implements OnInit {
  itemForm: FormGroup;
  produtos: Produto[] = [];
  loadingProdutos = false;

  itens: ItemNotaForm[] = [];
  dataSource = new MatTableDataSource<ItemNotaForm>([]);
  displayedColumns: string[] = ['produto', 'quantidade', 'preco', 'acoes'];

  salvando = false;
  excluindo = false;

  get notaAberta(): boolean {
    return this.data.status?.toLowerCase() === 'aberta';
  }

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<NotasEditDialogComponent>,
    private snack: MatSnackBar,
    private faturamentoApi: FaturamentoApi,
    private estoqueApi: EstoqueApi,
    @Inject(MAT_DIALOG_DATA) public data: Nota
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
        this.produtos = produtos;
        this.loadingProdutos = false;
        this.preencherItensDaNota();
      },
      error: err => {
        console.error('Erro ao carregar produtos do estoque', err);
        this.loadingProdutos = false;
        this.snack.open(
          'Erro ao carregar produtos do estoque.',
          'Fechar',
          { duration: 4000 }
        );
        this.preencherItensDaNota();
      }
    });
  }

  preencherItensDaNota(): void {
    if (!this.data.itens) {
      this.itens = [];
      this.dataSource.data = this.itens;
      return;
    }

    this.itens = this.data.itens.map(i => {
      const prod = this.produtos.find(p => p.id === i.produtoId);
      return {
        produtoId: i.produtoId,
        produtoDescricao: prod?.descricao ?? `Produto #${i.produtoId}`,
        quantidade: i.quantidade,
        preco: i.preco
      };
    });

    this.dataSource.data = this.itens;
  }

  onCancel(): void {
    this.dialogRef.close(null);
  }

  adicionarItem(): void {
    if (!this.notaAberta) return;

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
    if (!this.notaAberta) return;

    this.itens = this.itens.filter((_, i) => i !== index);
    this.dataSource.data = this.itens;
  }

  onSave(): void {
    if (!this.notaAberta) return;

    if (this.itens.length === 0) {
      this.snack.open(
        'A nota deve possuir ao menos um item.',
        'Fechar',
        { duration: 4000 }
      );
      return;
    }

    const dto: AtualizarNotaDto = {
      itens: this.itens.map<CriarNotaItemDto>(i => ({
        produtoId: i.produtoId,
        quantidade: i.quantidade,
        preco: i.preco
      }))
    };

    this.salvando = true;

    this.faturamentoApi.atualizarNota(this.data.id, dto).subscribe({
      next: res => {
        this.salvando = false;
        this.snack.open('Nota fiscal atualizada com sucesso!', 'OK', {
          duration: 3000
        });
        this.dialogRef.close('updated');
      },
      error: err => {
        console.error('Erro ao atualizar nota', err);
        this.salvando = false;

        const msg =
          err?.error?.message ||
          err?.error?.title ||
          'Erro ao atualizar nota fiscal.';
        this.snack.open(msg, 'Fechar', { duration: 5000 });
      }
    });
  }

  onDelete(): void {
    if (!this.notaAberta) return;

    this.excluindo = true;

    this.faturamentoApi.deletarNota(this.data.id).subscribe({
      next: () => {
        this.excluindo = false;
        this.snack.open('Nota fiscal excluída com sucesso!', 'OK', {
          duration: 3000
        });
        this.dialogRef.close('deleted');
      },
      error: err => {
        console.error('Erro ao excluir nota', err);
        this.excluindo = false;

        const msg =
          err?.error?.message ||
          err?.error?.title ||
          'Erro ao excluir nota fiscal.';
        this.snack.open(msg, 'Fechar', { duration: 5000 });
      }
    });
  }
}
import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EstoqueApi, Produto, CriarProdutoDto } from '../../../core/estoque.api';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-produto-edit-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    ReactiveFormsModule,
    MatSnackBarModule
  ],
  templateUrl: './produtos-edit-dialog.component.html',
  styleUrl: './produtos-edit-dialog.component.css'
})

export class ProdutoEditDialogComponent {
  form: FormGroup;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<ProdutoEditDialogComponent>,
    private estoqueApi: EstoqueApi,
    private snack: MatSnackBar,
    @Inject(MAT_DIALOG_DATA) public data: Produto
  ) {
    this.form = this.fb.group({
      descricao: [data.descricao, [Validators.required]],
      saldo: [data.saldo, [Validators.required, Validators.min(0)]]
    });
  }

  onCancel() {
    this.dialogRef.close(null);
  }

  onSave() {
    if (this.form.invalid || this.loading) return;
    this.loading = true;

    const dto = this.form.value;

    this.estoqueApi.atualizarProduto(this.data.id, dto).subscribe({
      next: (produtoAtualizado) => {
        this.loading = false;

        this.snack.open(
          `Produto atualizado com sucesso!`,
          'OK',
          { duration: 3000 }
        );

        this.dialogRef.close(produtoAtualizado);
      },
      error: (err) => {
        this.loading = false;

        const msg = err?.error?.message || 'Erro ao atualizar o produto.';
        this.snack.open(msg, 'Fechar', { duration: 4000 });
      }
    });
  }

  onDelete() {
    if (this.loading) return;
    this.loading = true;

    this.estoqueApi.deletarProduto(this.data.id).subscribe({
      next: (res) => {
        this.loading = false;

        const msg = res?.message || 'Produto removido com sucesso.';
        this.snack.open(msg, 'OK', { duration: 3000 });

        this.dialogRef.close('deleted');
      },
      error: (err) => {
        this.loading = false;

        const msg = err?.error?.message || 'Erro ao remover o produto.';
        this.snack.open(msg, 'Fechar', { duration: 4000 });
      }
    });
  }
}
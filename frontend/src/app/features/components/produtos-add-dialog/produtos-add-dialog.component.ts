import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms';
import { EstoqueApi, Produto } from '../../../core/estoque.api';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-produto-create-dialog',
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
  templateUrl: './produtos-add-dialog.component.html',
  styleUrl: './produtos-add-dialog.component.css'
})
export class ProdutoAddDialogComponent {
  loading = false;
  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<ProdutoAddDialogComponent>,
    private estoqueApi: EstoqueApi,
    private snack: MatSnackBar
  ) {
    this.form = this.fb.group({
      descricao: ['', [Validators.required]],
      saldo: [0, [Validators.required, Validators.min(0)]],
    });
  }

  onCancel() {
    this.dialogRef.close(null);
  }

  onSubmit() {
    if (this.form.invalid || this.loading) return;

    this.loading = true;

    const dto = this.form.value;

    this.estoqueApi.criarProduto(dto).subscribe({
      next: (produto) => {
        this.loading = false;

        this.snack.open(
          `Produto "${produto.descricao}" criado com sucesso!`,
          'OK',
          { duration: 3000 }
        );

        this.dialogRef.close(produto);
      },
      error: (err) => {
        this.loading = false;

        const msg = err?.error?.message || 'Erro ao criar o produto.';
        this.snack.open(msg, 'Fechar', { duration: 4000 });
      }
    });
  }
}


import { Component, inject } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { NotasApi } from '../../../../core/api/notas.api';
import { CommonModule } from '@angular/common';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { NotaCreateDto } from '../../../../core/api/notas.api';

@Component({
  selector: 'app-nota-dialog',
  templateUrl: './nota-dialog.component.html',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ]
})

export class NotaDialogComponent {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<NotaDialogComponent>);
  private notasApi = inject(NotasApi);
  salvando = false;

  form: FormGroup = this.fb.group({
    itens: this.fb.array([this.novoItem()])
  });

  get itens(): FormArray {
    return this.form.get('itens') as FormArray<FormGroup>;
  }

  get itensControls(): FormGroup[] {
    return this.itens.controls as FormGroup[];
  }

  addItem(): void {
    this.itens.push(this.novoItem());
  }

  removeItem(i: number): void {
    if (this.itens.length > 1) {
      this.itens.removeAt(i);
    }
  }

  cancelar(): void {
    this.dialogRef.close(false);
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const dto = {
      itens: this.itens.controls.map((g: any) => ({
        produtoId: Number(g.get('produtoId')?.value),
        quantidade: Number(g.get('quantidade')?.value),
        preco: Number(g.get('preco')?.value ?? 0)
      }))
    };

    this.notasApi.create(dto).subscribe({
      next: () => this.dialogRef.close(true),
      error: () => {
        this.dialogRef.close(false);
      }
    });
  }

  private novoItem(): FormGroup {
    return this.fb.group({
      produtoId: [null, Validators.required],
      quantidade: [1, [Validators.required, Validators.min(1)]],
      preco: [0, [Validators.required, Validators.min(0)]],
    });
  }
}
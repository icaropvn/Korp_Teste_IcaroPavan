import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

export interface NotaItemCreateDto {
  produtoId: number;
  quantidade: number;
  preco: number;
}

export interface NotaCreateDto {
  itens: NotaItemCreateDto[];
}

@Component({
  standalone: true,
  selector: 'app-nota-dialog',
  templateUrl: './nota-dialog.component.html',
  styleUrls: ['./nota-dialog.component.css'],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule
  ],
})
export class NotaDialogComponent {
  private fb = inject(FormBuilder);
  private dialogRef = inject<MatDialogRef<NotaDialogComponent, NotaCreateDto | null>>(MatDialogRef);

  form = this.fb.group({
    itens: this.fb.array<FormGroup>([
      this.novoItemGroup()
    ])
  });

  get itens(): FormArray<FormGroup> {
    return this.form.get('itens') as FormArray<FormGroup>;
  }

  novoItemGroup(): FormGroup {
    return this.fb.group({
      produtoId: new FormControl<number | null>(null, { nonNullable: false, validators: [Validators.required, Validators.min(1)] }),
      quantidade: new FormControl<number>(1, { nonNullable: true, validators: [Validators.required, Validators.min(1)] }),
      preco: new FormControl<number>(0, { nonNullable: true, validators: [Validators.required, Validators.min(0)] }),
    });
  }

  addItem(): void {
    this.itens.push(this.novoItemGroup());
  }

  removeItem(index: number): void {
    if (this.itens.length > 1) this.itens.removeAt(index);
  }

  cancelar(): void {
    this.dialogRef.close(null);
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const dto: NotaCreateDto = {
      itens: this.itens.controls.map(g => ({
        produtoId: Number(g.get('produtoId')?.value),
        quantidade: Number(g.get('quantidade')?.value),
        preco: Number(g.get('preco')?.value),
      }))
    };
    this.dialogRef.close(dto);
  }
}

export type { NotaCreateDto as _NotaCreateDto };
import { Component, inject } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

interface ProdutoCreateDto {
  descricao: string;
  saldo: number;
}

@Component({
  standalone: true,
  selector: 'app-produto-dialog',
  templateUrl: './produto-dialog.component.html',
  styleUrls: ['./produto-dialog.component.css'],
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
  ],
})
export class ProdutoDialogComponent {
  private fb = inject(FormBuilder);
  private dialogRef = inject<MatDialogRef<ProdutoDialogComponent, ProdutoCreateDto | null>>(MatDialogRef);

  form = this.fb.group({
    descricao: ['', Validators.required],
    saldo: [0, [Validators.required, Validators.min(0)]],
  });

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const dto: ProdutoCreateDto = {
      descricao: this.form.value.descricao!,
      saldo: this.form.value.saldo ?? 0,
    };

    this.dialogRef.close(dto);
  }

  cancelar(): void {
    this.dialogRef.close(null);
  }
}

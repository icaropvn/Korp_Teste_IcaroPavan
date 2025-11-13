import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { FaturamentoApi, Nota } from '../../../core/faturamento.api';
import { NotasAddDialogComponent } from '../../components/notas-add-dialog/notas-add-dialog.component';
import { NotasEditDialogComponent } from '../../components/notas-edit-dialog/notas-edit-dialog.component';

@Component({
  selector: 'app-notas-page',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatTableModule,
    MatIconModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  templateUrl: './notas-page.component.html',
  styleUrl: './notas-page.component.css'
})
export class NotasPageComponent implements OnInit {
  cols: string[] = ['numero', 'status', 'acoes'];
  dataSource = new MatTableDataSource<Nota>([]);
  hasFilter = false;
  loading = false;
  imprimindoId: number | null = null;

  constructor(
    private faturamentoApi: FaturamentoApi,
    private dialog: MatDialog,
    private snack: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.carregarNotas();
  }

  carregarNotas(q?: string): void {
    this.loading = true;

    this.faturamentoApi.listarNotas(q).subscribe({
      next: notas => {
        this.dataSource.data = notas;
        this.loading = false;
      },
      error: err => {
        console.error('Erro ao carregar notas', err);
        this.dataSource.data = [];
        this.loading = false;
        this.snack.open('Erro ao carregar notas fiscais.', 'Fechar', {
          duration: 4000
        });
      }
    });
  }

  applyFilter(event: KeyboardEvent): void {
    const value = (event.target as HTMLInputElement).value ?? '';
    const trimmed = value.trim();
    this.hasFilter = trimmed.length > 0;
    this.carregarNotas(trimmed || undefined);
  }

  clearFilter(): void {
    this.hasFilter = false;
    this.carregarNotas();
  }

  novaNota(): void {
    const ref = this.dialog.open(NotasAddDialogComponent, {
      width: '900px',
      maxWidth: '95vw',
      disableClose: true
    });

    ref.afterClosed().subscribe(result => {
      if (!result) return;
      this.carregarNotas();
    });
  }

  editarNota(nota: Nota): void {
    const ref = this.dialog.open(NotasEditDialogComponent, {
      width: '900px',
      maxWidth: '95vw',
      disableClose: true,
      data: nota
    });

    ref.afterClosed().subscribe(result => {
      if (!result) return;
      this.carregarNotas();
    });
  }

  imprimirNota(nota: Nota, event: MouseEvent): void {
    event.stopPropagation();

    if (nota.status?.toLowerCase() === 'fechada') return;

    this.imprimindoId = nota.id;

    this.faturamentoApi.imprimirNota(nota.id).subscribe({
      next: () => {
        this.imprimindoId = null;

        this.snack.open(
          `Impressão da nota ${nota.numero} iniciada.`,
          'OK',
          { duration: 3000 }
        );

        this.carregarNotas();
      },
      error: err => {
        console.error('Erro ao imprimir nota', err);
        this.imprimindoId = null;

        const msg =
          err?.error?.message ?? 'Erro ao enviar nota para impressão.';
        this.snack.open(msg, 'Fechar', { duration: 4000 });
      }
    });
  }

  isFechada(nota: Nota): boolean {
    return nota.status?.toLowerCase() === 'fechada';
  }
}
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { NotasApi, NotaReadDto, Page } from '../../../../core/api/notas.api';
import { NotaDialogComponent } from '../../components/nota-dialog/nota-dialog.component';

@Component({
  selector: 'app-notas-list',
  standalone: true,
  templateUrl: './notas-list.component.html',
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    NotaDialogComponent
  ]
})
export class NotasListComponent {
  private api = inject(NotasApi);
  private dialog = inject(MatDialog);

  q = '';
  page = 1;
  pageSize = 10;
  total = 0;

  private _data: NotaReadDto[] = [];
  get data(): NotaReadDto[] { return this._data; }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.list(this.q, this.page, this.pageSize).subscribe({
      next: (res: Page<NotaReadDto>) => {
        this._data = res.items ?? [];
        this.total = res.total ?? 0;
      },
      error: () => {
        this._data = [];
        this.total = 0;
      }
    });
  }

  abrirCriar(): void {
    const ref = this.dialog.open(NotaDialogComponent, {
      width: '720px',
      disableClose: true
    });

    ref.afterClosed().subscribe((created: boolean) => {
      if (created) this.load();
    });
  }

  pesquisar(): void {
    this.page = 1;
    this.load();
  }

  irParaPagina(p: number): void {
    if (p < 1) return;
    const maxPages = Math.max(1, Math.ceil(this.total / this.pageSize));
    this.page = Math.min(p, maxPages);
    this.load();
  }
}
import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ProdutosApi, ProdutoDto } from '../../../../core/api/produtos.api';
import { debounceTime, distinctUntilChanged, startWith, switchMap, tap, catchError, of, Subject, merge } from 'rxjs';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatButtonModule } from '@angular/material/button';
import { ProdutoDialogComponent } from '../../components/produto-dialog/produto-dialog.component';

@Component({
  selector: 'app-produtos-list',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatTableModule, MatPaginatorModule,
    MatFormFieldModule, MatInputModule, MatProgressSpinnerModule,
    MatButtonModule, MatDialogModule, MatSnackBarModule,
    ProdutoDialogComponent
  ],
  templateUrl: './produtos-list.component.html',
  styleUrls: ['./produtos-list.component.css']
})
export class ProdutosListComponent implements OnInit {
  private api = inject(ProdutosApi);
  private dialog = inject(MatDialog);
  private snack = inject(MatSnackBar);

  displayedColumns = ['codigo', 'descricao', 'saldo'];
  data: ProdutoDto[] = [];
  total = 0;
  pageSize = 10;
  pageIndex = 0;

  search = new FormControl<string>('', { nonNullable: true });
  loading = signal(false);

  private reload$ = new Subject<void>();

  ngOnInit(): void {
    merge(
      this.search.valueChanges.pipe(
        startWith(this.search.value),
        debounceTime(300),
        distinctUntilChanged(),
        tap(() => { this.pageIndex = 0; })
      ),
      this.reload$
    ).pipe(
      switchMap(() => {
        this.loading.set(true);
        return this.api.list(this.search.value ?? '', this.pageIndex + 1, this.pageSize)
          .pipe(
            tap(res => {
              this.data = res.items;
              this.total = res.total;
              this.loading.set(false);
            }),
            catchError(err => {
              this.loading.set(false);
              console.error(err);
              this.data = [];
              this.total = 0;
              return of();
            })
          );
      })
    ).subscribe();
  }

  onPageChange(e: PageEvent) {
    this.pageIndex = e.pageIndex;
    this.pageSize = e.pageSize;
    this.reload$.next();
  }

  novoProduto() {
    const ref = this.dialog.open(ProdutoDialogComponent, { disableClose: true });

    ref.afterClosed().subscribe(dto => {
      if (!dto) return;

      this.loading.set(true);

      this.api.create(dto).subscribe({
        next: _ => {
          this.snack.open('Produto criado com sucesso!', 'OK', { duration: 2500 });
          
          this.pageIndex = 0;
          this.reload$.next();
        },
        error: err => {
          this.loading.set(false);
          const msg = err?.status === 409
            ? 'Código já existente.'
            : 'Erro ao criar produto.';
          this.snack.open(msg, 'OK', { duration: 3000 });
        }
      });
    });
  }
}
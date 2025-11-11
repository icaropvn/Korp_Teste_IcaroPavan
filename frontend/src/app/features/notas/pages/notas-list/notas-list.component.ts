import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { HttpClient, HttpParams } from '@angular/common/http';
import { debounceTime, distinctUntilChanged, switchMap, catchError, of } from 'rxjs';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { NotaDialogComponent, NotaCreateDto } from '../../components/nota-dialog/nota-dialog.component';

export interface NotaItemReadDto {
  id: number;
  produtoId: number;
  quantidade: number;
  preco: number;
}

export interface NotaReadDto {
  id: number;
  numero?: number | null;
  status: string;
  itens?: NotaItemReadDto[];
}

export interface PagedNotasResponse {
  items: NotaReadDto[];
  total: number;
  page?: number;
  size?: number;
}

class ApiNotasService {
  private http = inject(HttpClient);
  private baseUrl = '/api/faturamento/notas';

  list(q = '', page = 1, size = 20) {
    let params = new HttpParams().set('page', page).set('size', size);
    if (q?.trim()) params = params.set('q', q.trim());
    return this.http.get<PagedNotasResponse>(this.baseUrl, { params });
  }

  getById(id: number) {
    return this.http.get<NotaReadDto>(`${this.baseUrl}/${id}`);
  }

  create(dto: NotaCreateDto) {
    return this.http.post<NotaReadDto>(this.baseUrl, dto);
  }
}

@Component({
  standalone: true,
  selector: 'app-notas-list',
  templateUrl: './notas-list.component.html',
  styleUrls: ['./notas-list.component.css'],
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatSnackBarModule],
  providers: [ApiNotasService],
})
export class NotasListComponent implements OnInit {
  search = new FormControl<string>('', { nonNullable: true });
  pageIndex = 0;
  pageSize = 20;
  total = 0;

  loading = signal(false);
  data = signal<NotaReadDto[]>([]);

  totalPages = computed(() =>
    this.pageSize > 0 ? Math.max(1, Math.ceil(this.total / this.pageSize)) : 1
  );

  private api = inject(ApiNotasService);
  private dialog = inject(MatDialog);
  private snack = inject(MatSnackBar);

  ngOnInit(): void {
    this.load();

    this.search.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap(q => {
          this.pageIndex = 0;
          this.loading.set(true);
          return this.api.list(q ?? '', this.pageIndex + 1, this.pageSize)
            .pipe(catchError(() => of({ items: [], total: 0 } as PagedNotasResponse)));
        })
      )
      .subscribe(res => {
        this.data.set(res.items ?? []);
        this.total = res.total ?? 0;
        this.loading.set(false);
      });
  }

  load(): void {
    this.loading.set(true);
    const q = this.search.value ?? '';
    this.api.list(q, this.pageIndex + 1, this.pageSize)
      .pipe(catchError(() => of({ items: [], total: 0 } as PagedNotasResponse)))
      .subscribe(res => {
        this.data.set(res.items ?? []);
        this.total = res.total ?? 0;
        this.loading.set(false);
      });
  }

  goToPage(index: number): void {
    if (index < 0) index = 0;
    if (index > this.totalPages() - 1) index = this.totalPages() - 1;
    if (index === this.pageIndex) return;
    this.pageIndex = index;
    this.load();
  }

  nextPage(): void {
    this.goToPage(this.pageIndex + 1);
  }

  prevPage(): void {
    this.goToPage(this.pageIndex - 1);
  }

  refreshCurrentPage(): void {
    const lastPageIndex = Math.max(0, this.totalPages() - 1);
    if (this.pageIndex > lastPageIndex) this.pageIndex = lastPageIndex;
    this.load();
  }

  novaNota(): void {
    const ref = this.dialog.open(NotaDialogComponent, { disableClose: true });
    ref.afterClosed().subscribe((dto?: NotaCreateDto | null) => {
      if (!dto) return;
      this.loading.set(true);
      this.api.create(dto).subscribe({
        next: _ => {
          this.snack.open('Nota criada com sucesso!', 'OK', { duration: 2500 });
          this.refreshCurrentPage();
        },
        error: _ => {
          this.loading.set(false);
          this.snack.open('Erro ao criar nota.', 'OK', { duration: 3000 });
        }
      });
    });
  }
}
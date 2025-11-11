import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'produtos', pathMatch: 'full' },

  {
    path: 'produtos',
    loadComponent: () =>
      import('./features/produtos/pages/produtos-list/produtos-list.component')
        .then(m => m.ProdutosListComponent)
  },

  {
    path: 'notas',
    loadComponent: () =>
      import('./features/notas/pages/notas-list/notas-list.component')
        .then(m => m.NotasListComponent)
  },

  { path: '**', redirectTo: 'produtos' }
];


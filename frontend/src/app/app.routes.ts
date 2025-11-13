import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'produtos', pathMatch: 'full' },

  {
    path: 'produtos',
    loadComponent: () =>
      import('./features/pages/produtos-page/produtos-page.component')
        .then(m => m.ProdutosPageComponent)
  },

  {
    path: 'notas',
    loadComponent: () =>
      import('./features/pages/notas-page/notas-page.component')
        .then(m => m.NotasPageComponent)
  },

  { path: '**', redirectTo: 'produtos' }
];


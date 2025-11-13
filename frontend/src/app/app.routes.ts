import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'produtos', pathMatch: 'full' },

  {
    path: 'produtos',
    loadComponent: () =>
      import('./features/pages/produtos-page/produtos-page.component')
        .then(m => m.ProdutosPageComponent)
  },

  // {
  //   path: 'notas',
  //   loadComponent: () =>
  //     import('./features/notas/pages/notas-list/notas-list.component')
  //       .then(m => m.NotasListComponent)
  // },

  { path: '**', redirectTo: 'produtos' }
];


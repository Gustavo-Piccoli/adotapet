import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./features/pets/pet-list.page').then((m) => m.PetListPage),
  },
  { path: '**', redirectTo: '' },
];

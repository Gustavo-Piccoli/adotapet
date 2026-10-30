import { Routes } from '@angular/router';
import { guestGuard } from './core/auth/auth.guards';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./features/pets/pet-list.page').then((m) => m.PetListPage),
  },
  {
    path: 'pets/:id',
    loadComponent: () => import('./features/pets/pet-details.page').then((m) => m.PetDetailsPage),
  },
  {
    path: 'entrar',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login.page').then((m) => m.LoginPage),
  },
  {
    path: 'cadastro/ong',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/register-shelter.page').then((m) => m.RegisterShelterPage),
  },
  {
    path: 'cadastro/adotante',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/register-adopter.page').then((m) => m.RegisterAdopterPage),
  },
  { path: '**', redirectTo: '' },
];

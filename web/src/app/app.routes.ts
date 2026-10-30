import { Routes } from '@angular/router';
import { adopterGuard, guestGuard, shelterGuard } from './core/auth/auth.guards';

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
  {
    path: 'meus-pedidos',
    canActivate: [adopterGuard],
    loadComponent: () => import('./features/adopter/my-requests.page').then((m) => m.MyRequestsPage),
  },
  {
    path: 'ong/pets',
    canActivate: [shelterGuard],
    loadComponent: () =>
      import('./features/shelter/shelter-pets.page').then((m) => m.ShelterPetsPage),
  },
  {
    path: 'ong/pets/novo',
    canActivate: [shelterGuard],
    loadComponent: () => import('./features/shelter/pet-form.page').then((m) => m.PetFormPage),
  },
  {
    path: 'ong/pets/:id/editar',
    canActivate: [shelterGuard],
    loadComponent: () => import('./features/shelter/pet-form.page').then((m) => m.PetFormPage),
  },
  {
    path: 'ong/pedidos',
    canActivate: [shelterGuard],
    loadComponent: () =>
      import('./features/shelter/shelter-requests.page').then((m) => m.ShelterRequestsPage),
  },
  { path: '**', redirectTo: '' },
];

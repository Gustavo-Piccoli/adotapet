import { Routes } from '@angular/router';
import { adopterGuard, guestGuard, shelterGuard } from './core/auth/auth.guards';

export const routes: Routes = [
  {
    path: '',
    title: 'Adote um amigo — AdotPet',
    pathMatch: 'full',
    loadComponent: () => import('./features/pets/pet-list.page').then((m) => m.PetListPage),
  },
  {
    path: 'pets/:id',
    title: 'Pet — AdotPet',
    data: { announcesItself: true },
    loadComponent: () => import('./features/pets/pet-details.page').then((m) => m.PetDetailsPage),
  },
  {
    path: 'entrar',
    title: 'Entrar — AdotPet',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login.page').then((m) => m.LoginPage),
  },
  {
    path: 'cadastro/ong',
    title: 'Cadastro de ONG — AdotPet',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/register-shelter.page').then((m) => m.RegisterShelterPage),
  },
  {
    path: 'cadastro/adotante',
    title: 'Cadastro de adotante — AdotPet',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/register-adopter.page').then((m) => m.RegisterAdopterPage),
  },
  {
    path: 'meus-pedidos',
    title: 'Meus pedidos — AdotPet',
    canActivate: [adopterGuard],
    loadComponent: () => import('./features/adopter/my-requests.page').then((m) => m.MyRequestsPage),
  },
  {
    path: 'ong/pets',
    title: 'Meus pets — AdotPet',
    canActivate: [shelterGuard],
    loadComponent: () =>
      import('./features/shelter/shelter-pets.page').then((m) => m.ShelterPetsPage),
  },
  {
    path: 'ong/pets/novo',
    title: 'Cadastrar pet — AdotPet',
    canActivate: [shelterGuard],
    loadComponent: () => import('./features/shelter/pet-form.page').then((m) => m.PetFormPage),
  },
  {
    path: 'ong/pets/:id/editar',
    title: 'Editar pet — AdotPet',
    canActivate: [shelterGuard],
    loadComponent: () => import('./features/shelter/pet-form.page').then((m) => m.PetFormPage),
  },
  {
    path: 'ong/pedidos',
    title: 'Pedidos de adoção — AdotPet',
    canActivate: [shelterGuard],
    loadComponent: () =>
      import('./features/shelter/shelter-requests.page').then((m) => m.ShelterRequestsPage),
  },
  {
    path: '**',
    title: 'Página não encontrada — AdotPet',
    loadComponent: () => import('./features/not-found.page').then((m) => m.NotFoundPage),
  },
];

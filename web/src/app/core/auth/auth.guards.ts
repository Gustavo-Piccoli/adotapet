import { inject } from '@angular/core';
import { CanActivateFn, Router, RouterStateSnapshot } from '@angular/router';
import { AuthService } from './auth.service';
import { Role } from './auth.models';

function check(role: Role | null, state: RouterStateSnapshot) {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (!auth.isLoggedIn()) {
    return router.createUrlTree(['/entrar'], { queryParams: { returnUrl: state.url } });
  }
  if (role && auth.role() !== role) {
    return router.createUrlTree(['/']);
  }
  return true;
}

export const authGuard: CanActivateFn = (_route, state) => check(null, state);
export const shelterGuard: CanActivateFn = (_route, state) => check('Shelter', state);
export const adopterGuard: CanActivateFn = (_route, state) => check('Adopter', state);

import { Role } from './auth.models';

export function roleHome(role: Role | null): string {
  return role === 'Shelter' ? '/ong/pets' : '/';
}

/** Accepts only same-app absolute paths, to avoid open redirects. */
export function safeReturnUrl(raw: string | null | undefined): string | null {
  return raw && raw.startsWith('/') && !raw.startsWith('//') && !raw.includes('\\') ? raw : null;
}

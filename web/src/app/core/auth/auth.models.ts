export type Role = 'Shelter' | 'Adopter';

export interface AuthResult {
  accessToken: string;
  expiresAt: string;
  role: Role;
  profileId: string;
  name: string;
  email: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterShelterDto {
  email: string;
  password: string;
  name: string;
  cnpj: string;
  city: string;
  state: string;
  phone: string;
  description: string;
}

export interface RegisterAdopterDto {
  email: string;
  password: string;
  name: string;
  phone: string;
  city: string;
  state: string;
}

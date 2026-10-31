export type Species = 'Dog' | 'Cat' | 'Other';
export type Sex = 'Male' | 'Female';
export type Size = 'Small' | 'Medium' | 'Large';
export type AgeGroup = 'Puppy' | 'Adult' | 'Senior';
export type PetStatus = 'Available' | 'Adopted';
export type AdoptionRequestStatus = 'Pending' | 'Approved' | 'Rejected';

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface PetSummary {
  id: string;
  name: string;
  species: Species;
  sex: Sex;
  size: Size;
  ageGroup: AgeGroup;
  photoUrl: string | null;
  city: string;
  state: string;
  shelterName: string;
}

export interface ShelterInfo {
  id: string;
  name: string;
  city: string;
  state: string;
  phone: string;
  email: string;
  description: string;
}

export interface PetDetails {
  id: string;
  name: string;
  species: Species;
  sex: Sex;
  size: Size;
  ageGroup: AgeGroup;
  isNeutered: boolean;
  isVaccinated: boolean;
  description: string;
  photoUrl: string | null;
  status: PetStatus;
  shelter: ShelterInfo;
}

export interface SearchPetsParams {
  species?: Species;
  sex?: Sex;
  size?: Size;
  ageGroup?: AgeGroup;
  state?: string;
  city?: string;
  page?: number;
  pageSize?: number;
}

export interface PetInput {
  name: string;
  species: Species;
  sex: Sex;
  size: Size;
  ageGroup: AgeGroup;
  isNeutered: boolean;
  isVaccinated: boolean;
  description: string;
}

export interface ShelterPet {
  id: string;
  name: string;
  species: Species;
  sex: Sex;
  size: Size;
  ageGroup: AgeGroup;
  isNeutered: boolean;
  isVaccinated: boolean;
  description: string;
  photoUrl: string | null;
  status: PetStatus;
  createdAt: string;
}

export interface AdopterRequest {
  id: string;
  petId: string;
  petName: string;
  petPhotoUrl: string | null;
  shelterName: string;
  message: string;
  status: AdoptionRequestStatus;
  createdAt: string;
  decidedAt: string | null;
}

export interface ShelterRequest {
  id: string;
  petId: string;
  petName: string;
  adopterName: string;
  adopterEmail: string;
  adopterPhone: string;
  adopterCity: string;
  adopterState: string;
  message: string;
  status: AdoptionRequestStatus;
  createdAt: string;
  decidedAt: string | null;
}

export interface CreatedId {
  id: string;
}

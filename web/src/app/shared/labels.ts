import {
  AdoptionRequestStatus,
  AgeGroup,
  PetStatus,
  Sex,
  Size,
  Species,
} from '../core/api/api.models';

export const SPECIES_LABELS: Record<Species, string> = {
  Dog: 'Cachorro',
  Cat: 'Gato',
  Other: 'Outro',
};
export const SEX_LABELS: Record<Sex, string> = { Male: 'Macho', Female: 'Fêmea' };
export const SIZE_LABELS: Record<Size, string> = {
  Small: 'Pequeno',
  Medium: 'Médio',
  Large: 'Grande',
};
export const AGE_GROUP_LABELS: Record<AgeGroup, string> = {
  Puppy: 'Filhote',
  Adult: 'Adulto',
  Senior: 'Idoso',
};
export const PET_STATUS_LABELS: Record<PetStatus, string> = {
  Available: 'Disponível',
  Adopted: 'Adotado',
};
export const REQUEST_STATUS_LABELS: Record<AdoptionRequestStatus, string> = {
  Pending: 'Pendente',
  Approved: 'Aprovado',
  Rejected: 'Recusado',
};

export const UFS = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG', 'PA',
  'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO',
] as const;

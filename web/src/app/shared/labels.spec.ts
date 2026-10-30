import {
  AGE_GROUP_LABELS,
  PET_STATUS_LABELS,
  REQUEST_STATUS_LABELS,
  SEX_LABELS,
  SIZE_LABELS,
  SPECIES_LABELS,
  UFS,
} from './labels';

describe('labels', () => {
  it('has pt-BR labels for every enum value', () => {
    expect(SPECIES_LABELS).toEqual({ Dog: 'Cachorro', Cat: 'Gato', Other: 'Outro' });
    expect(SEX_LABELS).toEqual({ Male: 'Macho', Female: 'Fêmea' });
    expect(SIZE_LABELS).toEqual({ Small: 'Pequeno', Medium: 'Médio', Large: 'Grande' });
    expect(AGE_GROUP_LABELS).toEqual({ Puppy: 'Filhote', Adult: 'Adulto', Senior: 'Idoso' });
    expect(PET_STATUS_LABELS).toEqual({ Available: 'Disponível', Adopted: 'Adotado' });
    expect(REQUEST_STATUS_LABELS).toEqual({
      Pending: 'Pendente',
      Approved: 'Aprovado',
      Rejected: 'Recusado',
    });
  });

  it('lists the 27 UFs without duplicates', () => {
    expect(UFS.length).toBe(27);
    expect(new Set(UFS).size).toBe(27);
    expect(UFS).toContain('RS');
    expect(UFS).toContain('DF');
  });
});

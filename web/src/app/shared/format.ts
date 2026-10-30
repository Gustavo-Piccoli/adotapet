const dateFormat = new Intl.DateTimeFormat('pt-BR', {
  dateStyle: 'short',
  timeZone: 'America/Sao_Paulo',
});

export function formatDate(iso: string): string {
  return dateFormat.format(new Date(iso));
}

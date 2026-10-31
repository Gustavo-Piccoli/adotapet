import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CreatedId, PagedResult, PetDetails, PetSummary, SearchPetsParams } from './api.models';

@Injectable({ providedIn: 'root' })
export class PetsApi {
  private readonly http = inject(HttpClient);

  search(filters: SearchPetsParams = {}): Observable<PagedResult<PetSummary>> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filters)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return this.http.get<PagedResult<PetSummary>>('/api/pets', { params });
  }

  get(id: string): Observable<PetDetails> {
    return this.http.get<PetDetails>(`/api/pets/${id}`);
  }

  createAdoptionRequest(petId: string, message: string): Observable<CreatedId> {
    return this.http.post<CreatedId>(`/api/pets/${petId}/adoption-requests`, { message });
  }
}

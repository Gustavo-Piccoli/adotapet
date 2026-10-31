import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AdoptionRequestStatus,
  CreatedId,
  PetInput,
  ShelterPet,
  ShelterRequest,
} from './api.models';

@Injectable({ providedIn: 'root' })
export class ShelterApi {
  private readonly http = inject(HttpClient);

  listPets(): Observable<ShelterPet[]> {
    return this.http.get<ShelterPet[]>('/api/shelter/pets');
  }

  getPet(id: string): Observable<ShelterPet> {
    return this.http.get<ShelterPet>(`/api/shelter/pets/${id}`);
  }

  createPet(input: PetInput): Observable<CreatedId> {
    return this.http.post<CreatedId>('/api/shelter/pets', input);
  }

  updatePet(id: string, input: PetInput): Observable<void> {
    return this.http.put<void>(`/api/shelter/pets/${id}`, input);
  }

  uploadPhoto(id: string, file: File): Observable<void> {
    const form = new FormData();
    form.append('file', file);
    return this.http.put<void>(`/api/shelter/pets/${id}/photo`, form);
  }

  listRequests(status?: AdoptionRequestStatus): Observable<ShelterRequest[]> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    return this.http.get<ShelterRequest[]>('/api/shelter/adoption-requests', { params });
  }

  approveRequest(id: string): Observable<void> {
    return this.http.post<void>(`/api/shelter/adoption-requests/${id}/approve`, null);
  }

  rejectRequest(id: string): Observable<void> {
    return this.http.post<void>(`/api/shelter/adoption-requests/${id}/reject`, null);
  }
}

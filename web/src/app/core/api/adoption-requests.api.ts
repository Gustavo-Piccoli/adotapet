import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AdopterRequest } from './api.models';

@Injectable({ providedIn: 'root' })
export class AdoptionRequestsApi {
  private readonly http = inject(HttpClient);

  listMine(): Observable<AdopterRequest[]> {
    return this.http.get<AdopterRequest[]>('/api/adopter/adoption-requests');
  }
}

import { Injectable } from '@angular/core';
import { DataService } from './data.service';
import { Observable, of } from 'rxjs';

import { Moment } from 'moment'
import * as moment from 'moment';
import { HttpParams } from '@angular/common/http';

const constLocal = {
  labels: "labels/names"
};

@Injectable({
  providedIn: 'root'
})
export class LabelsService {

  constructor(private dataService: DataService) {
  }

  public getLabels(): Observable<string[]> {
    return this.dataService.get<string[]>(constLocal.labels, null, []);
  }
}

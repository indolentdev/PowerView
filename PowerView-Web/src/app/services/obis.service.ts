import { Injectable } from '@angular/core';
import { DataService } from './data.service';
import { Observable, of } from 'rxjs';
import { ObisCodeSet } from '../model/obisCodeSet';

import { Moment } from 'moment'
import * as moment from 'moment';
import { HttpParams } from '@angular/common/http';

const constLocal = {
  obisCodes: "obis/codes"
};

@Injectable({
  providedIn: 'root'
})
export class ObisService {

  constructor(private dataService: DataService) {
  }

  public getObisCodes(label: string): Observable<ObisCodeSet> {
    var params = new HttpParams().set("label", label);
    return this.dataService.get<ObisCodeSet>(constLocal.obisCodes, params, new ObisCodeSet);
  }
}

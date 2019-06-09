import { Injectable } from '@angular/core';
import { DataService } from './data.service';
import { Observable, of } from 'rxjs';
import { ProfilePage } from '../model/profilePage';

import { Moment } from 'moment'
import * as moment from 'moment';
import { HttpParams } from '@angular/common/http';

const constLocal = {
  profile: "profile/"
};

@Injectable({
  providedIn: 'root'
})
export class ProfileService {

  constructor(private dataService: DataService) { 
  }

  public getProfilePage(profilePeriod: string, page: string, start: Moment): Observable<ProfilePage> {
    var params = new HttpParams().set("page", page).set("start", start.toISOString());
    return this.dataService.get<ProfilePage>(constLocal.profile + profilePeriod, params, new ProfilePage);
  }

}

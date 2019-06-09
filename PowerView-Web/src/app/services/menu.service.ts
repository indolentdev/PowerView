import { Injectable, Output, EventEmitter } from '@angular/core';
import { DataService } from './data.service';
import { Observable, of } from 'rxjs';
import { ProfilePageNameSet } from '../model/profilePageNameSet';

import { HttpParams } from '@angular/common/http';

const constLocal = {
  settingsProfilePages: "settings/profilegraphs/pages"
};

@Injectable({
  providedIn: 'root'
})
export class MenuService {
  private profileGraphPagesChanged: EventEmitter<any> = new EventEmitter();

  constructor(private dataService: DataService) { 
  }

  public getProfilePageNames(period: string): Observable<ProfilePageNameSet> {
    var params = new HttpParams().set("period", period);
    return this.dataService.get<ProfilePageNameSet>(constLocal.settingsProfilePages, params, new ProfilePageNameSet);
  }

  public signalProfileGraphPagesChanged() {
    this.profileGraphPagesChanged.emit();
  }

  public getProfileGraphPageChanges(): Observable<any> {
    return this.profileGraphPagesChanged;
  }

}

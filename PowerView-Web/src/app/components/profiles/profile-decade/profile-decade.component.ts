import { Component, OnInit, Injectable } from '@angular/core';
import { NGXLogger } from 'ngx-logger';

import { DateAdapter } from '@angular/material/core';
import { MomentDateAdapter } from '@angular/material-moment-adapter';

import { Moment } from 'moment'
import moment from 'moment';


@Injectable()
export class DecadeCustomMomentDateAdapter extends MomentDateAdapter {
  format(date: Moment, displayFormat: Object): string {
    let year = date.year();
    let remainder = year % 10;
    let decadeStart = year - remainder;
    let decadeEnd = decadeStart + 9;
    return decadeStart + "-" + decadeEnd;
  }
}

@Component({
    selector: 'app-profile-decade',
    templateUrl: './profile-decade.component.html',
    styleUrls: ['./profile-decade.component.css'],
    providers: [
        { provide: DateAdapter, useClass: DecadeCustomMomentDateAdapter }
    ],
    standalone: false
})
export class ProfileDecadeComponent implements OnInit {
  dpMaxStartTime: Moment;
  defaultStartTime: Moment;

  constructor(private log: NGXLogger) {
    var now = moment();
    let remainder = now.year() % 10;
    let decade = now.year() - remainder;
    this.dpMaxStartTime = moment([now.year(), 11, 31, 12, 0, 0, 0]);
    this.defaultStartTime = moment([decade, 0, 1, 0, 0, 0, 0]);
  }

  ngOnInit() {
  }

  decadeFilter = (m: Moment | null): boolean => {
    if (m === undefined || m === null) return false;

    if (m.year() > moment().year()) return false;

    return (m.year() % 10) == 0;
  };
}

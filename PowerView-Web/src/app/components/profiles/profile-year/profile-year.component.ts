import { Component, OnInit, Injectable } from '@angular/core';
import { NGXLogger } from 'ngx-logger';

import { DateAdapter } from '@angular/material/core';
import { MomentDateAdapter } from '@angular/material-moment-adapter';

import { Moment } from 'moment'
import * as moment from 'moment';


@Injectable()
export class YearCustomMomentDateAdapter extends MomentDateAdapter {
  format(date: Moment, displayFormat: Object): string {
    var formatString = 'YYYY';
    return date.format(formatString);
  }
}

@Component({
  selector: 'app-profile-year',
  templateUrl: './profile-year.component.html',
  styleUrls: ['./profile-year.component.css'],
  providers: [
    { provide: DateAdapter, useClass: YearCustomMomentDateAdapter }
  ]
})
export class ProfileYearComponent implements OnInit {
  dpMaxStartTime: Moment;
  defaultStartTime: Moment;

  constructor(private log: NGXLogger) { 
    var now = moment();
    this.dpMaxStartTime = moment([now.year(), 0, 1, 0, 0, 0, 0]);
    this.defaultStartTime = moment([now.year(), 0, 1, 0, 0, 0, 0]);
  }

  ngOnInit() {
  }
}

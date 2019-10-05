import { Component, OnInit } from '@angular/core';
import { NGXLogger } from 'ngx-logger';

import { Moment } from 'moment'
import * as moment from 'moment';

@Component({
  selector: 'app-profile-last31d',
  templateUrl: './profile-last31d.component.html',
  styleUrls: ['./profile-last31d.component.css']
})
export class ProfileLast31dComponent implements OnInit {
  maxStartTime: Moment;
  defaultStartTime: Moment;

  constructor(private log: NGXLogger) { 
    var now = moment();
    var rewindDate = now.subtract(32, "days");
    var start = moment([rewindDate.year(), rewindDate.month(), rewindDate.day(), 0, 0, 0, 0]);

    this.maxStartTime = start;
    this.defaultStartTime = start;
  }

  ngOnInit() {
  }

}

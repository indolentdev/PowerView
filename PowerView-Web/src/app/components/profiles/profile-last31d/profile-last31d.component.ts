import { Component, OnInit } from '@angular/core';
import { NGXLogger } from 'ngx-logger';

import { Moment } from 'moment'
import moment from 'moment';

@Component({
    selector: 'app-profile-last31d',
    templateUrl: './profile-last31d.component.html',
    styleUrls: ['./profile-last31d.component.css'],
    standalone: false
})
export class ProfileLast31dComponent implements OnInit {
  maxStartTime: Moment;
  defaultStartTime: Moment;

  constructor(private log: NGXLogger) { 
    var now = moment();
    var rewindDate = now.subtract(1, "months");
    var start = moment([rewindDate.year(), rewindDate.month(), rewindDate.date(), 0, 0, 0, 0]);

    this.maxStartTime = start;
    this.defaultStartTime = start;
  }

  ngOnInit() {
  }

}

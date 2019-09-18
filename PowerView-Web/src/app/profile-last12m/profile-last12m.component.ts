import { Component, OnInit } from '@angular/core';
import { NGXLogger } from 'ngx-logger';

import { Moment } from 'moment'
import * as moment from 'moment';

@Component({
  selector: 'app-profile-last12m',
  templateUrl: './profile-last12m.component.html',
  styleUrls: ['./profile-last12m.component.css']
})
export class ProfileLast12mComponent implements OnInit {
  maxStartTime: Moment;
  defaultStartTime: Moment;

  constructor(private log: NGXLogger) { 
    var now = moment();
    var start = now.startOf('month');
//    start = start.set('minute', Math.floor((start.minute() / 10))*10); // truncate down to nearest 10 mins.
    start = start.subtract(1, "years");
    this.maxStartTime = start;
    this.defaultStartTime = start;
  }

  ngOnInit() {
  }

}

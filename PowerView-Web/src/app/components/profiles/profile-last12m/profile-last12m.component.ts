import { Component, OnInit } from '@angular/core';
import { NGXLogger } from 'ngx-logger';

import { Moment } from 'moment'
import moment from 'moment';

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
    var month = now.month(); + 1;
    if (month >= 12) month = 0;
    var start = moment([now.year()-1, month, 1, 0, 0, 0, 0]);

    this.maxStartTime = start;
    this.defaultStartTime = start;
  }

  ngOnInit() {
  }

}

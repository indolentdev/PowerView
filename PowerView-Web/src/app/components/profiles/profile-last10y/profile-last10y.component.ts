import { Component, OnInit } from '@angular/core';
import { NGXLogger } from 'ngx-logger';

import { Moment } from 'moment'
import moment from 'moment';

@Component({
  selector: 'app-profile-last10y',
  templateUrl: './profile-last10y.component.html',
  styleUrls: ['./profile-last10y.component.css']
})
export class ProfileLast10yComponent implements OnInit {
  maxStartTime: Moment;
  defaultStartTime: Moment;

  constructor(private log: NGXLogger) {
    var now = moment();
    let start = moment([now.year()-10, 0, 1, 0, 0, 0, 0]);

    this.maxStartTime = start;
    this.defaultStartTime = start;
  }

  ngOnInit() {
  }

}

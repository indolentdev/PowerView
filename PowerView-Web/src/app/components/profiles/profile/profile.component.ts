import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Router, ActivatedRoute, NavigationEnd } from "@angular/router";
import { NGXLogger } from 'ngx-logger';
import { MatDatepicker } from '@angular/material/datepicker';
import { MatDatepickerInputEvent } from '@angular/material/datepicker';
import { ProfileService } from '../../../services/profile.service';
import { ProfilePage } from '../../../model/profilePage';
import { ProfileTotalValue } from '../../../model/profileTotalValue';

import { Moment } from 'moment'
import * as moment from 'moment';

const pageParam = "page";
const startTimeParam = "startTime";

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  page: string;
  dpMinStartTime = moment("2010-01-01T00:00:00Z");
  fcStartTime = new FormControl(null);

  profileSet: ProfilePage;
  profileTotalValues: ProfileTotalValue[] = [];

  @ViewChild(MatDatepicker, { static: false }) dp: MatDatepicker<Moment>;

  @Input('profilePeriod') profilePeriod: string;
  @Input('profileHeading') profileHeading: string;
  @Input('timeFormat') timeFormat: string;
  @Input('defaultStartTime') defaultStartTime: Moment;

  @Input('dpShow') dpShow: boolean;
  @Input('dpChooseResource') dpChooseResource: string;
  @Input('dpStartView') dpStartView: string;
  @Input('dpMaxStartTime') dpMaxStartTime: Moment;

  constructor(private log: NGXLogger, private router: Router, private route: ActivatedRoute, private profileService: ProfileService) { 
  }

  ngOnInit() {
    this.route.queryParamMap.subscribe(queryParams => {
      if (!queryParams.has(pageParam)) {
        this.log.error("Query parameter missing:", pageParam);
        return;
      }
      if (!queryParams.has(startTimeParam)) {
        this.navigate({ startTime: this.defaultStartTime.toISOString() });
        return;
      }

      this.page = queryParams.get(pageParam);

      const startTimeString = queryParams.get(startTimeParam);
      var startTime = moment(startTimeString);
      if (startTime.isValid() && !startTime.isSame(this.fcStartTime.value)) {
        this.fcStartTime.setValue(startTime);
      }

      if (this.fcStartTime.value != null) {
        this.getProfile(this.profilePeriod, this.page, this.fcStartTime.value);
      }
    });
  }

  monthSelected(event: Moment) {
    if (this.dpStartView == "year") {
      this.dp.close();
      this.navigate({ startTime: event.toISOString() });
    }
  }

  yearSelected(event: Moment) {
    if (this.dpStartView == "multi-year") {
      this.dp.close();
      this.navigate({ startTime: event.toISOString() });
    }
  }

  startTimeChangeEvent(event: MatDatepickerInputEvent<Moment>) {
    if (event == null) return;
    if (event.value == null) return;

    this.navigate({ startTime: event.value.toISOString() });
  }

  navigate(queryParams: any): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: queryParams,
      queryParamsHandling: "merge",
      replaceUrl: true
    });
  }

  getProfile(profilePeriod: string, page: string, startTime: Moment): void {
    this.profileService.getProfilePage(profilePeriod, page, startTime).subscribe(x => { 
      this.profileSet = x;
      this.profileTotalValues = x.periodTotals;
    });
  }

}

import { Component, OnInit, Input, ViewChild, OnDestroy } from '@angular/core';
import { UntypedFormControl } from '@angular/forms';
import { Router, ActivatedRoute, NavigationEnd } from "@angular/router";
import { NGXLogger } from 'ngx-logger';
import { MatDatepicker } from '@angular/material/datepicker';
import { Observable, interval, Subscription, timer } from 'rxjs';
import { MatDatepickerInputEvent } from '@angular/material/datepicker';
import { ProfileService } from '../../../services/profile.service';
import { ProfilePage } from '../../../model/profilePage';
import { ProfileTotalValue } from '../../../model/profileTotalValue';

import { Moment } from 'moment'
import moment from 'moment';

const pageParam = "page";
const startTimeParam = "startTime";

@Component({
    selector: 'app-profile',
    templateUrl: './profile.component.html',
    styleUrls: ['./profile.component.css'],
    standalone: false
})
export class ProfileComponent implements OnInit, OnDestroy {
  page: string;
  dpMinStartTime = moment("2010-01-01T00:00:00");
  fcStartTime = new UntypedFormControl(null);

  autoRefresh: Subscription;

  profileSet: ProfilePage;
  profileTotalValues: ProfileTotalValue[] = [];

  @ViewChild(MatDatepicker) dp: MatDatepicker<Moment>;

  @Input('profilePeriod') profilePeriod: string;
  @Input('profileHeading') profileHeading: string;
  @Input('timeFormat') timeFormat: string;
  @Input('defaultStartTime') defaultStartTime: Moment;

  @Input('dpShow') dpShow: boolean;
  @Input('dpChooseResource') dpChooseResource: string;
  @Input('dpStartView') dpStartView: string;
  @Input('dpMaxStartTime') dpMaxStartTime: Moment;
  @Input('dpFilter') dpFilter: any;

  constructor(private log: NGXLogger, private router: Router, private route: ActivatedRoute, private profileService: ProfileService) { 
  }

  ngOnInit() {
    this.route.queryParamMap.subscribe(queryParams => {
      if (!queryParams.has(pageParam)) {
        this.log.error("Query parameter missing:", pageParam);
        return;
      }
      if (!queryParams.has(startTimeParam)) {
        this.navigateMerge({ startTime: this.defaultStartTime.toISOString() });
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

  ngOnDestroy() {
    if (this.autoRefresh) {
      this.autoRefresh.unsubscribe();
    }
  }

  monthSelected(event: Moment) {
    if (this.dpStartView == "year") {
      this.dp.close();
      this.navigateMerge({ startTime: event.toISOString() });
    }
  }

  yearSelected(event: Moment) {
    if (this.dpStartView == "multi-year") {
      this.dp.close();
      this.navigateMerge({ startTime: event.toISOString() });
    }
  }

  startTimeChangeEvent(event: MatDatepickerInputEvent<Moment>) {
    if (event == null) return;
    if (event.value == null) return;

    this.navigateMerge({ startTime: event.value.toISOString() });
  }

  navigatePrevious() {
    this.navigateMerge({ startTime: this.getAdjacentStartTime(false).toISOString() });
  }

  previousDisabled(): boolean {
    if (this.getAdjacentStartTime(false) < this.dpMinStartTime) return true;
    return false;
  }

  navigateNext() {
    this.navigateMerge({ startTime: this.getAdjacentStartTime(true).toISOString() });
  }

  nextDisabled(): boolean {
    if (this.getAdjacentStartTime(true) > this.dpMaxStartTime) return true;
    return false;
  }

  private getAdjacentStartTime(next: boolean) : Moment {
    let stTime = this.fcStartTime.value;
    if (stTime === null) return null;
    let [val, comp] = this.getPeriodParameters();
    let factor = next ? 1 : -1;
    stTime = stTime.clone().add(val*factor, comp);
    return stTime;
  }

  private getPeriodParameters(): [number, string] {
    if (this.profilePeriod == "day") return [1, "days"];
    if (this.profilePeriod == "month") return [1, "months"];
    if (this.profilePeriod == "year") return [1, "years"];
    if (this.profilePeriod == "decade") return [10, "years"];
    return [0, "days"]
  }

  navigateMerge(queryParams: any): void {
    this.navigate(queryParams, "merge");
  }

  navigate(queryParams: any, queryParamsHandling: any): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: queryParams,
      queryParamsHandling: queryParamsHandling,
      replaceUrl: true
    });
  }
  
  getProfile(profilePeriod: string, page: string, startTime: Moment): void {
    this.profileService.getProfilePage(profilePeriod, page, startTime).subscribe(x => { 
      this.profileSet = x;
      this.profileTotalValues = x.periodTotals;

      if (this.autoRefresh) {
        this.autoRefresh.unsubscribe();
        this.autoRefresh = null;
      }

      let now = moment();
      let diff = now.diff(startTime, 'minutes');
      if (profilePeriod === "day" && diff > 0 && diff <= 60*24 + 60 ) {
        let delayMinutes = 5;
        this.log.debug("Activating auto refresh timer. Minutes:" + delayMinutes);
        this.autoRefresh = timer(delayMinutes * 60 * 1000).subscribe(
          x => {
            this.log.debug("Auto refresh timer event.");
            this.navigate({ page: page }, ""); // "" replaces existing query params.
          },
          (err) => {
            this.log.warn("Auto refresh timer error.", err);
          }
        );
      }
    });
  }

}

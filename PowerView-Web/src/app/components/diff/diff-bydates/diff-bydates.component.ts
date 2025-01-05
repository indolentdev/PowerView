import { Component, OnInit } from '@angular/core';
import { UntypedFormControl } from '@angular/forms';
import { Router, ActivatedRoute } from "@angular/router";
import { NGXLogger } from 'ngx-logger';
import { MatDatepickerInputEvent } from '@angular/material/datepicker';
import { DiffService } from '../../../services/diff.service';
import { DiffValueSet } from '../../../model/diffValueSet';

import { Moment } from 'moment'
import moment from 'moment';

const fromParam = "from";
const toParam = "to";

@Component({
    selector: 'app-diff-bydates',
    templateUrl: './diff-bydates.component.html',
    styleUrls: ['./diff-bydates.component.css'],
    standalone: false
})
export class DiffBydatesComponent implements OnInit {
  minDateFrom = moment("2010-01-01T00:00:00Z");
  maxDateFrom = moment().subtract(2, 'days');
  minDateTo = moment("2010-01-02T00:00:00Z");
  maxDateTo = moment().subtract(1, 'days');
  fcFromDate = new UntypedFormControl(null);
  fcToDate = new UntypedFormControl(null);

  diffValueSet: DiffValueSet;

  constructor(private log: NGXLogger, private router: Router, private route: ActivatedRoute, private diffService: DiffService) {
  }

  ngOnInit() {
    this.route.queryParamMap.subscribe(queryParams => {
      const fromDateString = queryParams.get(fromParam);
      this.parseDateSetFormControl(fromDateString, this.fcFromDate);

      const toDateString = queryParams.get(toParam);
      this.parseDateSetFormControl(toDateString, this.fcToDate);

      if (this.fcFromDate.value != null && this.fcToDate.value != null) {
        this.getDiffValues(this.fcFromDate.value, this.fcToDate.value);
      }
    });
  }

  parseDateSetFormControl(dateString: string, fcDate: UntypedFormControl): void {
    var parsedDate = moment(dateString);
    if (parsedDate.isValid() && !parsedDate.isSame(fcDate.value))
    {
      fcDate.setValue(parsedDate);
    }
  }

  fromDateChangeEvent(event: MatDatepickerInputEvent<Moment>) {
    if (event == null) return;
    if (event.value == null) return;

    this.minDateTo = moment(event.value.toISOString()).add(1, 'days');

    this.navigate({ from: event.value.toISOString()});
  }

  toDateChangeEvent(event: MatDatepickerInputEvent<Moment>) {
    if (event == null) return;
    if (event.value == null) return;

    this.maxDateFrom = moment(event.value.toISOString()).subtract(1, 'days');

    this.navigate({ to: event.value.toISOString()});
  }

  navigate(queryParams: any): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: queryParams,
      queryParamsHandling: "merge",
      replaceUrl: true
    });
  }

  getDiffValues(from: Moment, to: Moment): void {
    this.diffService.getDiffValues(from, to.add(1,'days')).subscribe(x => { 
      this.diffValueSet = x; 
    });
  }

}

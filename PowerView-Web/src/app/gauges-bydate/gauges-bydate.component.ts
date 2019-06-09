import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Router, ActivatedRoute } from "@angular/router";
import { NGXLogger } from 'ngx-logger';
import { MatDatepickerInputEvent } from '@angular/material/datepicker';
import { GaugesService } from '../services/gauges.service';
import { GaugeValueGroupSet } from '../model/gaugeValueGroupSet';


import { Moment } from 'moment'
import * as moment from 'moment';

const dateParam = "date";

@Component({
  selector: 'app-gauges-bydate',
  templateUrl: './gauges-bydate.component.html',
  styleUrls: ['./gauges-bydate.component.css']
})
export class GaugesBydateComponent implements OnInit {
  minDate = moment("2010-01-01T00:00:00Z");
  maxDate = moment().subtract(1, 'days');
  fcDate = new FormControl(null);

  gaugeValueGroupSet: GaugeValueGroupSet;

  constructor(private log: NGXLogger, private router: Router, private route: ActivatedRoute, private gaugesService: GaugesService) {
  }

  ngOnInit() {
    this.route.queryParamMap.subscribe(queryParams => {
      const dateString = queryParams.get(dateParam);
      var parsedDate = moment(dateString);
      if (parsedDate.isValid() && !parsedDate.isSame(this.fcDate.value))
      {
        this.fcDate.setValue(parsedDate);
      }

      if (this.fcDate.value != null) {
        this.getGuageValues(this.fcDate.value);
      }
    });
  }

  dateChangeEvent(event: MatDatepickerInputEvent<Moment>) {
    if (event == null) return;
    if (event.value == null) return;

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { date: event.value.toISOString() },
      queryParamsHandling: "merge",
      replaceUrl: true
    });
  }

  getGuageValues(timestamp: Moment): void {
    this.gaugesService.getCustomGaugeValues(timestamp).subscribe(x => { 
      this.gaugeValueGroupSet = x; 
    });
  }

}

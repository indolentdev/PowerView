import { Component, OnInit } from '@angular/core';
import { UntypedFormControl } from '@angular/forms';
import { Router, ActivatedRoute } from "@angular/router";
import { NGXLogger } from 'ngx-logger';
import { MatDatepickerInputEvent } from '@angular/material/datepicker';
import { LabelsService } from 'src/app/services/labels.service';

import { Moment } from 'moment'
import moment from 'moment';

const labelParam = "label";
const dateParam = "date";

@Component({
    selector: 'app-data-crude-bydate',
    templateUrl: './data-crude-bydate.component.html',
    styleUrls: ['./data-crude-bydate.component.css'],
    standalone: false
})
export class DataCrudeByDateComponent implements OnInit {
  minDate = moment("2010-01-01T00:00:00Z");
  maxDate = moment();
  fcLabel = new UntypedFormControl(null);
  fcDate = new UntypedFormControl(null);

  labels: string[];

  constructor(private log: NGXLogger, private router: Router, private route: ActivatedRoute, private labelsService: LabelsService) { }

  ngOnInit(): void {
    this.getLabels();

    this.route.queryParamMap.subscribe(queryParams => {
      const labelString = queryParams.get(labelParam);
      this.fcLabel.setValue(labelString);

      const dateString = queryParams.get(dateParam);
      var parsedDate = moment(dateString);
      if (parsedDate.isValid() && !parsedDate.isSame(this.fcDate.value)) {
        this.fcDate.setValue(parsedDate);
      }
    });
  }

  private getLabels(): void {
    if (this.labels == null) {
      this.labelsService.getLabels().subscribe(x => {
        this.labels = x.sort((a: string, b: string) => (a > b) ? 1 : -1);;
      });
    }
  }

  labelChangeEvent(event) {
    if (event == null) return;
    if (event.value == null) return;

    this.navigate({ label: event.value });
  }

  dateChangeEvent(event: MatDatepickerInputEvent<Moment>) {
    if (event == null) return;
    if (event.value == null) return;

    this.navigate({ date: event.value.toISOString() });
  }

  private navigate(queryParams: any): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: queryParams,
      queryParamsHandling: "merge",
      replaceUrl: true
    });
  }

}

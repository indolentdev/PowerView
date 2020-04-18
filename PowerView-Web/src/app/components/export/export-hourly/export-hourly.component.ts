import { Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, AbstractControl, FormGroup, Validators, FormGroupDirective, NgForm } from '@angular/forms';
import { Router, ActivatedRoute } from "@angular/router";
import { NGXLogger } from 'ngx-logger';
import { MatDatepickerInputEvent } from '@angular/material/datepicker';
import { ExportService } from '../../../services/export.service';
//import { DiffValueSet } from '../../../model/diffValueSet';

import { Moment } from 'moment'
import * as moment from 'moment';

const labelsParam = "labels";
const fromParam = "from";
const toParam = "to";

@Component({
  selector: 'app-export-hourly',
  templateUrl: './export-hourly.component.html',
  styleUrls: ['./export-hourly.component.css']
})
export class ExportHourlyComponent implements OnInit {
  minDateFrom = moment("2010-01-01T00:00:00Z");
  maxDateFrom = moment().subtract(2, 'days');
  minDateTo = moment("2010-01-02T00:00:00Z");
  maxDateTo = moment().subtract(1, 'days');

  labels: string[];

  formGroup: FormGroup;
  @ViewChild('form', { static: true }) form;

  constructor(private log: NGXLogger, private router: Router, private route: ActivatedRoute, private exportService: ExportService) {
  }

  ngOnInit() {
    this.formGroup = new FormGroup({
      labels: new FormControl('', [Validators.required]),
      fromDate: new FormControl('', [Validators.required]),
      toDate: new FormControl('', [Validators.required])
    });

    this.getLabels();

    this.route.queryParamMap.subscribe(queryParams => {
      let fromDateCtl = this.getControl("fromDate");
      console.log("fromdDateCtl");
      console.log(fromDateCtl.value);
      const fromDateString = queryParams.get(fromParam);
      this.parseDateSetFormControl(fromDateString, fromDateCtl);

      let toDateCtl = this.getControl("toDate");
      console.log("toDateCtl");
      console.log(toDateCtl.value);
      const toDateString = queryParams.get(toParam);
      this.parseDateSetFormControl(toDateString, toDateCtl);

      let labels = null;
      let labelsSelectControl = this.getControl("labels");
      const labelsString = queryParams.get(labelsParam);
      if (labelsString != null)
      {
        labels = labelsString.split(",");
      }
      labelsSelectControl.setValue(labels);

      if (fromDateCtl.value != null && moment(fromDateCtl.value).isValid() &&
        toDateCtl.value != null && moment(toDateCtl.value).isValid() &&
        labelsSelectControl.value != null && labelsSelectControl.value.length > 0) {
          console.log("Good to query");
//        this.getDiffValues(this.fcFromDate.value, this.fcToDate.value);
      }
    });
    
  }

  private getLabels(): void {
    if (this.labels == null)
    {
      this.exportService.getLabels().subscribe(x => { 
        this.labels = x;
      });
    }
  }

  public getControl(controlName: string): AbstractControl {
    return this.formGroup.controls[controlName];
  }

  parseDateSetFormControl(dateString: string, fcDate: AbstractControl): void {
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

  labelsChangeEvent(event) {
    if (event == null) return;
    if (event.value == null) return;

    let labels = event.value.join();

    this.navigate({ labels: labels });
  }

  navigate(queryParams: any): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: queryParams,
      queryParamsHandling: "merge",
      replaceUrl: true
    });
  }

}

import { Component, OnInit,ViewChild, Input, Output, OnChanges, SimpleChanges, EventEmitter } from '@angular/core';
import { FormControl, AbstractControl, FormGroup, Validators, FormGroupDirective, NgForm } from '@angular/forms';
import { Router, ActivatedRoute } from "@angular/router";
import { NGXLogger } from 'ngx-logger';
import { MatDatepickerInputEvent } from '@angular/material/datepicker';
import { ExportService } from '../../../services/export.service';
import { ExportSpec } from '../../../model/exportSpec';

import { Moment } from 'moment'
import * as moment from 'moment';

const labelsParam = "labels";
const fromParam = "from";
const toParam = "to";

@Component({
  selector: 'app-export',
  templateUrl: './export.component.html',
  styleUrls: ['./export.component.css']
})
export class ExportComponent implements OnInit {

  private absoluteMaxDateFrom = moment().subtract(2, 'days');
  private absoluteMaxDateTo = moment().subtract(1, 'days');

  minDateFrom = moment("2010-01-01T00:00:00Z");
  maxDateFrom = this.absoluteMaxDateFrom.clone();
  minDateTo = moment("2010-01-02T00:00:00Z");
  maxDateTo = this.absoluteMaxDateTo.clone();;

  labels: string[];

  formGroup: FormGroup;
  @ViewChild('form', { static: true }) form;

  @Output('export') exportAction: EventEmitter<ExportSpec> = new EventEmitter();

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
      let labelsArray = [];
      let labelsSelectControl = this.getControl("labels");
      const labelsString = queryParams.get(labelsParam);
      if (labelsString != null)
      {
        labelsArray = labelsString.split(",");
      }
      if (labelsArray.length === 1 && labelsArray[0] === "") // we don't want &labels= to mean anything
      {
        labelsArray = [];
      }
      labelsSelectControl.setValue(labelsArray);

      let fromDateCtl = this.getControl("fromDate");
      const fromDateString = queryParams.get(fromParam);
      this.parseDateSetFormControl(fromDateString, fromDateCtl);

      let toDateCtl = this.getControl("toDate");
      const toDateString = queryParams.get(toParam);
      this.parseDateSetFormControl(toDateString, toDateCtl);
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

  private parseDateSetFormControl(dateString: string, fcDate: AbstractControl): void {
    let parsedDate = moment(dateString);
    if (parsedDate.isValid() && !parsedDate.isSame(fcDate.value))
    {
      fcDate.setValue(parsedDate);
    }
  }

  fromDateChangeEvent(event: MatDatepickerInputEvent<Moment>) {
    if (event == null) return;
    if (event.value == null) return;

    this.minDateTo = moment(event.value.toISOString()).add(1, 'days');
    this.maxDateTo = moment(event.value.toISOString()).add(2, 'days').add(3, 'months');
    if (this.maxDateTo > this.absoluteMaxDateTo)
    {
      this.maxDateTo = this.absoluteMaxDateTo.clone();
    }

    this.navigate({ from: event.value.toISOString() });
  }

  toDateChangeEvent(event: MatDatepickerInputEvent<Moment>) {
    if (event == null) return;
    if (event.value == null) return;

    this.maxDateFrom = moment(event.value.toISOString()).subtract(1, 'days'); 
    if (this.maxDateFrom > this.absoluteMaxDateFrom)
    {
      this.maxDateFrom = this.absoluteMaxDateFrom.clone();
    }
    this.minDateFrom = moment(event.value.toISOString()).subtract(2, 'days').subtract(3, 'months');

    this.navigate({ to: event.value.toISOString() });
  }

  labelsChangeEvent(event) {
    if (event == null) return;
    if (event.value == null) return;

    let labels = event.value.join();

    this.navigate({ labels: labels });
  }

  private navigate(queryParams: any): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: queryParams,
      queryParamsHandling: "merge",
      replaceUrl: true
    });
  }

  public submitForm(formGroupValue: any) {
    if (!this.formGroup.valid) {
      return;
    }

    if (formGroupValue.labels != null && formGroupValue.labels.length > 0 &&
      formGroupValue.fromDate != null && moment(formGroupValue.fromDate).isValid() &&
      formGroupValue.toDate != null && moment(formGroupValue.toDate).isValid() ) {
        let exportSpec =  { labels: formGroupValue.labels, 
          from: moment(formGroupValue.fromDate), to: moment(formGroupValue.toDate) };
        this.navigate({ labels: null, to: null, from: null });
        this.form.resetForm();
        this.exportAction.emit(exportSpec);
    }
  }

}

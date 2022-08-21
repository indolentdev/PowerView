import { Component, OnInit,ViewChild, Input, Output, OnChanges, SimpleChanges, EventEmitter } from '@angular/core';
import { UntypedFormControl, AbstractControl, UntypedFormGroup, Validators, FormGroupDirective, NgForm } from '@angular/forms';
import { Router, ActivatedRoute } from "@angular/router";
import { NGXLogger } from 'ngx-logger';
import { MatDatepickerInputEvent } from '@angular/material/datepicker';
import { ExportService } from '../../../services/export.service';
import { ExportSpec } from '../../../model/exportSpec';

import { Moment } from 'moment'
import * as moment from 'moment';

const labelParam = "label";
const fromParam = "from";
const toParam = "to";
const decimalSeparatorParam = "decimalSeparator";

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

  decimalSeparators: any[];

  formGroup: UntypedFormGroup;
  @ViewChild('form', { static: true }) form;

  @Output('export') exportAction: EventEmitter<ExportSpec> = new EventEmitter();

  constructor(private log: NGXLogger, private router: Router, private route: ActivatedRoute, private exportService: ExportService) {
  }

  ngOnInit() {
    this.formGroup = new UntypedFormGroup({
      labels: new UntypedFormControl('', [Validators.required]),
      fromDate: new UntypedFormControl('', [Validators.required]),
      toDate: new UntypedFormControl('', [Validators.required]),
      decimalSeparator: new UntypedFormControl('', [Validators.required])
    });

    this.getLabels();
    this.decimalSeparators = [ { name: "locale", value: "locale" }, { name: "dot", value: "." }, { name: "comma", value: "," } ];

    this.setDecimalSeparator("locale");

    this.route.queryParamMap.subscribe(queryParams => {
      let labelsSelectControl = this.getControl("labels");
      let labels = queryParams.getAll(labelParam);
      labelsSelectControl.setValue(labels);

      let fromDateCtl = this.getControl("fromDate");
      const fromDateString = queryParams.get(fromParam);
      this.parseDateSetFormControl(fromDateString, fromDateCtl);

      let toDateCtl = this.getControl("toDate");
      const toDateString = queryParams.get(toParam);
      this.parseDateSetFormControl(toDateString, toDateCtl);

      const decimalSeparatorString = queryParams.get(decimalSeparatorParam);
      var decimalSeparatorItem = this.fetchDecimalSeparator(decimalSeparatorString);
      this.setDecimalSeparator(decimalSeparatorItem.name);
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

  private setDecimalSeparator(value: string): void {
    this.getControl("decimalSeparator").patchValue(value);
  }

  private fetchDecimalSeparator(name: string): any {
    var match = this.decimalSeparators.find(x => x.name === name);
    if (match === undefined)
    {
      match = this.fetchDecimalSeparator("locale");
    }
    return match;
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

    this.navigate({ label: event.value });
  }

  decimalSeparatorChangeEvent(event) {
    if (event == null) return;
    if (event.value == null) return;

    let decimalSeparator = event.value;

    this.navigate({ decimalSeparator: decimalSeparator });
  }

  private navigate(queryParams: any): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: queryParams,
      queryParamsHandling: "merge",
      replaceUrl: true
    });
  }

  public hasError(controlName: string, errorName: string) {
    return this.formGroup.controls[controlName].hasError(errorName);
  }

  public submitForm(formGroupValue: any) {
    if (!this.formGroup.valid) {
      return;
    }

    let decimalSeparatorValue = this.fetchDecimalSeparator(formGroupValue.decimalSeparator).value;

    if (formGroupValue.labels != null && formGroupValue.labels.length > 0 &&
      formGroupValue.fromDate != null && moment(formGroupValue.fromDate).isValid() &&
      formGroupValue.toDate != null && moment(formGroupValue.toDate).isValid() ) {
        let exportSpec =  { labels: formGroupValue.labels, 
          decimalSeparator: decimalSeparatorValue,
          from: moment(formGroupValue.fromDate), to: moment(formGroupValue.toDate) };
        this.navigate({ labels: null, to: null, from: null });
        this.form.resetForm();
        this.exportAction.emit(exportSpec);
    }
  }

}

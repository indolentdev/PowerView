import { Component, OnInit, ViewChild, Input, Output, OnChanges, SimpleChanges, EventEmitter } from '@angular/core';
import { UntypedFormControl, UntypedFormGroup, Validators, FormGroupDirective, NgForm } from '@angular/forms';
import { MatDialog, MatDialogConfig, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ConfirmComponent } from '../../../confirm/confirm.component'
import { MatSnackBar, MatSnackBarRef, SimpleSnackBar } from '@angular/material/snack-bar';
import { NGXLogger } from 'ngx-logger';
import { MatDatepickerInputEvent } from '@angular/material/datepicker';
import { TranslateService } from '@ngx-translate/core';
import { SettingsService, AddCostBreakdownError } from '../../../../services/settings.service';
import { CostBreakdownEntry } from '../../../../model/costBreakdownEntry';

import { Moment } from 'moment'
import * as moment from 'moment';

@Component({
  selector: 'app-settings-cost-breakdown-entry',
  templateUrl: './settings-cost-breakdown-entry.component.html',
  styleUrls: ['./settings-cost-breakdown-entry.component.css']
})
export class SettingsCostBreakdownEntryComponent {
  private snackBarRef: MatSnackBarRef<SimpleSnackBar>;

  formGroup: UntypedFormGroup;
  @ViewChild('form', { static: true }) form;

  @Input('costBreakdownTitle') costBreakdownTitle: string;
  @Input('costBreakdownCurrency') costBreakdownCurrency: string;

  @Output('addedCostBreakdownEntry') addAction: EventEmitter<CostBreakdownEntry> = new EventEmitter();

  minDateFrom = moment("2010-01-01T00:00:00Z");
  maxDateFrom = moment("2050-01-01T00:00:00Z");
  minDateTo = moment("2010-01-02T00:00:00Z");
  maxDateTo = moment("2050-01-02T00:00:00Z");

  startTimeMinValue = 0;
  startTimeMaxValue = 22;
  endTimeMinValue = 1;
  endTimeMaxValue = 23;

  constructor(private log: NGXLogger, private settingsService: SettingsService, private snackBar: MatSnackBar, private translateService: TranslateService) {
  }

  ngOnInit() {
    this.formGroup = new UntypedFormGroup({
      fromDate: new UntypedFormControl('', [Validators.required]),
      toDate: new UntypedFormControl('', [Validators.required]),
      name: new UntypedFormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(25)]),
      startTime: new UntypedFormControl('', this.getStartTimeValidators()),
      endTime: new UntypedFormControl('', this.getEndTimeValidators()),
      amount: new UntypedFormControl('', [Validators.required, Validators.min(0), Validators.max(1000)])
    });
  }

  private getStartTimeValidators() {
    return [Validators.required, Validators.min(this.startTimeMinValue), Validators.max(this.startTimeMaxValue)];
  }

  private getEndTimeValidators() {
    return [Validators.required, Validators.min(this.endTimeMinValue), Validators.max(this.endTimeMaxValue)];
  }

  ngOnChanges(changes: SimpleChanges) {
    if (this.formGroup != null) {
      this.resetForm();
    }       
  }

  fromDateChangeEvent(event: MatDatepickerInputEvent<Moment>) {
    if (event == null) return;
    if (event.value == null) return;

    this.minDateTo = moment(event.value.toISOString()).add(1, 'days');

    let ctl = this.formGroup.controls["toDate"];
    if (ctl.value != undefined && ctl.value != null && ctl.value != '' && ctl.value <= event.value) {
      ctl.setValue(moment(event.value.toISOString()).add(1, 'days'));
    }
  }

  toDateChangeEvent(event: MatDatepickerInputEvent<Moment>) {
    if (event == null) return;
    if (event.value == null) return;

    this.maxDateFrom = moment(event.value.toISOString()).subtract(1, 'days');

    let ctl = this.formGroup.controls["fromDate"];
    if (ctl.value != undefined && ctl.value != null && ctl.value != '' && ctl.value >= event.value) {
      ctl.setValue( moment(event.value.toISOString()).subtract(1, 'days') ); 
    }
  }

  onStartTimeChange(event: any) {
    let value = this.formGroup.controls["startTime"].value;

    if (value == null) value = 1;

    this.endTimeMinValue = value + 1;
    this.formGroup.controls["endTime"].setValidators(this.getEndTimeValidators());
  };

  onEndTimeChange(event: any) {
    let value = this.formGroup.controls["endTime"].value;

    if (value == null) value = 22;

    this.startTimeMaxValue = value - 1;
    this.formGroup.controls["startTime"].setValidators(this.getStartTimeValidators());
  };


  public hasError(controlName: string, errorName: string) {
    return this.formGroup.controls[controlName].hasError(errorName);
  }

  public submitForm(formGroupValue: any) {
    if (!this.formGroup.valid) {
      return;
    }

    this.dismissSnackBar();

    let costBreakdownEntry: CostBreakdownEntry = formGroupValue;
    this.log.debug("Adding cost breakdown entry", costBreakdownEntry);
/*
    this.settingsService.addCostBreakdown(costBreakdown).subscribe(_ => {
      this.log.debug("Add ok");
      this.translateService.get('forms.settings.pricing.costBreakdown.confirmAdd').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
        this.resetForm();
      });
    }, err => {
      this.log.debug("Add failed", err);
      var translateIds = ['forms.settings.pricing.costBreakdown.errorAdd'];
      var addCostBreakdownError = err as AddCostBreakdownError;
      //      if (addCostBreakdownError === AddCostBreakdownError.??) {
      //        translateIds.push('forms.settings.pricing.costBreakdown.errorAdjustFields');
      //      }
      this.translateService.get(translateIds).subscribe(messages => {
        var message = "";
        for (var key in messages) {
          message += messages[key];
        }
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 9000 });
      });
    });
*/    
  }

  resetForm() {
    this.form.resetForm();
    this.minDateFrom = moment("2010-01-01T00:00:00Z");
    this.maxDateFrom = moment("2050-01-01T00:00:00Z");
    this.minDateTo = moment("2010-01-02T00:00:00Z");
    this.maxDateTo = moment("2050-01-02T00:00:00Z");
    this.startTimeMaxValue = 22;
    this.formGroup.controls["startTime"].setValidators(this.getStartTimeValidators());
    this.endTimeMinValue = 1;
    this.formGroup.controls["endTime"].setValidators(this.getEndTimeValidators());
  }

  private dismissSnackBar(): void {
    if (this.snackBarRef != null) {
      this.snackBarRef.dismiss();
    }
  }

}

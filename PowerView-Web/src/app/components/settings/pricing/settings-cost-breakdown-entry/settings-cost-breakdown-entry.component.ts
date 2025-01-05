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
import moment from 'moment';

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
  @Input('createMode') createMode: boolean;
  @Input('clear') clear: string;
  @Input('editEntry') editEntry: CostBreakdownEntry;

  @Output('addCostBreakdownEntry') addAction: EventEmitter<CostBreakdownEntry> = new EventEmitter();
  @Output('updateCostBreakdownEntry') updateAction: EventEmitter<CostBreakdownEntry> = new EventEmitter();

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
      name: new UntypedFormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(30)]),
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

    if (changes["editEntry"]) {
      let changesEditEntry = changes["editEntry"].currentValue;
      if (changesEditEntry != null) {
        this.setForm(changesEditEntry);
      }
    }
  }

  setForm(entry: any) {
    let costBreakDownEntry: CostBreakdownEntry = entry;

    if (costBreakDownEntry == null || costBreakDownEntry == undefined) {
      this.log.info("Skipping edit cost breakdown entry. Cost breakdown entry unspecified", entry);
      return;
    }

    this.dismissSnackBar();

    this.log.debug("Editing cost break down entry");

    let fromDateControl = this.formGroup.get('fromDate');
    fromDateControl.setValue(moment(costBreakDownEntry.fromDate));
    this.fromDateChangeEvent();

    let toDateControl = this.formGroup.get('toDate');
    toDateControl.setValue(moment(costBreakDownEntry.toDate));
    this.toDateChangeEvent();

    let nameControl = this.formGroup.get('name');
    nameControl.setValue(costBreakDownEntry.name);

    let startTimeControl = this.formGroup.get('startTime');
    startTimeControl.setValue(costBreakDownEntry.startTime);
    this.onStartTimeChange();

    let endTimeControl = this.formGroup.get('endTime');
    endTimeControl.setValue(costBreakDownEntry.endTime);
    this.onEndTimeChange();

    let amountControl = this.formGroup.get('amount');
    amountControl.setValue(costBreakDownEntry.amount);
  }

  fromDateChangeEvent() {
    let value = this.formGroup.controls["fromDate"].value;

    this.minDateTo = moment(value.toISOString()).add(1, 'days');

    let ctl = this.formGroup.controls["toDate"];
    if (ctl.value != undefined && ctl.value != null && ctl.value != '' && ctl.value <= value) {
      ctl.setValue(moment(value.toISOString()).add(1, 'days'));
    }
  }

  toDateChangeEvent() {
    let value = this.formGroup.controls["toDate"].value;

    this.maxDateFrom = moment(value.toISOString()).subtract(1, 'days');

    let ctl = this.formGroup.controls["fromDate"];
    if (ctl.value != undefined && ctl.value != null && ctl.value != '' && ctl.value >= value) {
      ctl.setValue( moment(value.toISOString()).subtract(1, 'days') ); 
    }
  }

  onStartTimeChange() {
    let value = this.formGroup.controls["startTime"].value;

    if (value == null) value = 1;

    this.endTimeMinValue = value + 1;
    this.formGroup.controls["endTime"].setValidators(this.getEndTimeValidators());
  };

  onEndTimeChange() {
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
    if (!this.createMode) {
      return;
    }

    this.dismissSnackBar();

    let costBreakdownEntry: CostBreakdownEntry = formGroupValue;
    this.log.debug("Adding cost breakdown entry (before emit)", costBreakdownEntry);

    this.addAction.emit(costBreakdownEntry);
  }

  updateClick() {
    if (!this.formGroup.valid) {
      return;
    }
    if (this.createMode) {
      return;
    }

    this.dismissSnackBar();

    let costBreakdownEntry: CostBreakdownEntry = this.formGroup.value;
    this.log.debug("Updating cost breakdown entry (before emit)", costBreakdownEntry);

    this.updateAction.emit(costBreakdownEntry);
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

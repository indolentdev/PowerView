import { Component, OnInit, ViewChild } from '@angular/core';
import { UntypedFormControl, AbstractControl, UntypedFormGroup, Validators, FormGroupDirective, NgForm } from '@angular/forms';
import { MatSnackBar, MatSnackBarRef, SimpleSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { NGXLogger } from 'ngx-logger';
import { MatDatepickerInputEvent } from '@angular/material/datepicker';
import { LabelsService } from 'src/app/services/labels.service';
import { AddCrudeValueError, CrudeDataService } from 'src/app/services/crude-data.service';
import { ObisService } from 'src/app/services/obis.service';
import { MissingDate } from 'src/app/model/missingDate';
import { CrudeValue } from 'src/app/model/crudeValue';

import { Moment } from 'moment'
import * as moment from 'moment';

@Component({
  selector: 'app-data-crude-add',
  templateUrl: './data-crude-add.component.html',
  styleUrls: ['./data-crude-add.component.css']
})
export class DataCrudeAddComponent implements OnInit {
  private snackBarRef: MatSnackBarRef<SimpleSnackBar>;

  labels: string[];
  missingDays: MissingDate[];
  previousCrudeValues: any[];
  nextCrudeValues: CrudeValue[];

  crudeValues: CrudeValue[];

  selectedLabel: string;
  selectedMissingDate: MissingDate;
  selectedRegister: CrudeValue;

  minValue: number;
  maxValue: number;

  formGroup: UntypedFormGroup;
  @ViewChild('form', { static: true }) form;
  @ViewChild('formDirective') private formDirective: NgForm;

  constructor(private log: NGXLogger, private labelsService: LabelsService, private crudeDataService: CrudeDataService, private obisService: ObisService, private translateService: TranslateService, private snackBar: MatSnackBar) { }

  ngOnInit(): void {
    this.formGroup = new UntypedFormGroup({
      label: new UntypedFormControl('', [Validators.required]),
      date: new UntypedFormControl({ value: '', disabled: true }, [Validators.required]),
      register: new UntypedFormControl({ value: '', disabled: true }, [Validators.required]),
      value: new UntypedFormControl({ value: '', disabled: true }, [Validators.required]),
    });

    this.getLabels();
    
    this.onChanges();
  }

  private getLabels(): void {
    if (this.labels == null) {
      this.labelsService.getLabels().subscribe(x => {
        this.labels = x.sort((a: string, b: string) => (a > b) ? 1 : -1);;
      });
    }
  }

  private onChanges(): void {
    this.formGroup.get('label').valueChanges.subscribe(selectedLabel => {
      this.labelOnChange(selectedLabel);
    });

    this.formGroup.get('date').valueChanges.subscribe(selectedMissingDate => {
      this.missingDayeOnChange(selectedMissingDate);
    });

    this.formGroup.get('register').valueChanges.subscribe(selectedRegister => {
      this.registerOnChange(selectedRegister);
    });
  }

  private labelOnChange(selectedLabel: string) {
    this.selectedLabel = selectedLabel;

    this.getControl("date").reset();

    if (this.selectedLabel == undefined || this.selectedLabel == null) return;

    this.crudeValues = [];

    this.crudeDataService.getDaysMissingCrudeValues(selectedLabel).subscribe(x => {
      this.missingDays = x.sort(function (obj1, obj2) {
        if (obj1.timestamp === obj2.timestamp) {
          return 0;
        }
        else {
          return obj1.timestamp < obj2.timestamp ? -1 : 1;
        }
      });

      if (this.missingDays.length > 0) {
        this.getControl("date").enable();
      }
    });
  }

  private missingDayeOnChange(selectedMissingDate: MissingDate) {
    this.selectedMissingDate = selectedMissingDate;

    this.getControl("register").reset();

    if (this.selectedLabel == null || selectedMissingDate == null) return;

    this.crudeValues = [];

    this.crudeDataService.getCrudeValuesOnDate(this.selectedLabel, this.selectedMissingDate.previousTimestamp).subscribe(x => {
      this.previousCrudeValues = this.obisService.AddRegisterProperty(x).sort((a, b) => a.register > b.register ? 1 : -1);

      if (this.previousCrudeValues.length > 0) {
        this.getControl("register").enable();
      }
    });
  }

  private registerOnChange(selectedRegister: CrudeValue) {
    this.selectedRegister = selectedRegister;

    this.getControl("value").reset();

    if (this.selectedMissingDate == null || this.selectedRegister == null) return;

    this.crudeValues = [];

    this.crudeDataService.getCrudeValuesOnDate(this.selectedLabel, this.selectedMissingDate.nextTimestamp).subscribe(x => {
      this.nextCrudeValues = x;

      if (this.nextCrudeValues.length > 0) {
        let contextList = [];
        contextList.push(this.selectedRegister);
        var nextRegister = this.nextCrudeValues.find(e => e.obisCode === this.selectedRegister.obisCode);
        contextList.push(nextRegister);
        this.crudeValues = contextList;

        if (this.crudeValues.length === 2) {
          this.minValue = this.selectedRegister.value;
          this.maxValue = nextRegister.value;

          var valueControl = this.getControl("value");
          
          valueControl.clearValidators();
          valueControl.addValidators([Validators.required, Validators.min(this.minValue), Validators.max(this.maxValue)]);

          valueControl.enable();
        }
      }
    });
  }

  public getControl(controlName: string): AbstractControl {
    return this.formGroup.controls[controlName];
  }

  public hasError(controlName: string, errorName: string) {
    return this.formGroup.controls[controlName].hasError(errorName);
  }

  public submitForm(formGroupValue: any) {
    if (!this.formGroup.valid) {
      return;
    }

    this.dismissSnackBar();

    let crudeValue: CrudeValue = {
      timestamp: formGroupValue.date.timestamp,
      obisCode: formGroupValue.register.obisCode,
      scale: formGroupValue.register.scale,
      unit: formGroupValue.register.unit,
      deviceId: formGroupValue.register.deviceId,
      value: formGroupValue.value
    };
    this.log.debug("Adding crude value", crudeValue);

    this.crudeDataService.addManualReading(formGroupValue.label, crudeValue).subscribe(_ => {
      this.log.debug("Add ok");
      this.translateService.get('forms.crudeData.add.confirmAdd').subscribe(message => {
        this.getControl("value").clearValidators();
        this.formDirective.resetForm();
        this.crudeValues = [];
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 9000 });
      });
    }, err => {
      this.log.debug("Add failed", err);
      var translateIds = ['forms.crudeData.add.errorAdd'];
      var addCrudeValueError = err as AddCrudeValueError;
      if (addCrudeValueError === AddCrudeValueError.RequestContentDuplicate) {
        translateIds.push('forms.crudeData.add.errorDuplicate');
      }
      this.translateService.get(translateIds).subscribe(messages => {
        var message = "";
        for (var key in messages) {
          message += messages[key];
        }
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 9000 });
      });
    });    
  }

  private dismissSnackBar(): void {
    if (this.snackBarRef != null) {
      this.snackBarRef.dismiss();
    }
  }

}

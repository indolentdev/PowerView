import { Component, OnInit, ViewChild } from '@angular/core';
import { UntypedFormControl, AbstractControl, UntypedFormGroup, Validators, FormGroupDirective, NgForm } from '@angular/forms';
import { MatSnackBar, MatSnackBarRef, SimpleSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { NGXLogger } from 'ngx-logger';
import { LabelsService } from 'src/app/services/labels.service';
import { ObisService } from 'src/app/services/obis.service';
import { AddCrudeValueError, CrudeDataService } from 'src/app/services/crude-data.service';
import { ObisTranslateService } from 'src/app/services/obis-translate.service';
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
  registers: any[];
  missingDays: MissingDate[];
  previousCrudeValue: any;
  nextCrudeValue: any;
  crudeValues: CrudeValue[];

  minValue: number;
  maxValue: number;

  selectedLabel: string;
  selectedRegister: string;
  selectedMissingDate: MissingDate;

  formGroup: UntypedFormGroup;
  @ViewChild('form', { static: true }) form;
  @ViewChild('formDirective') private formDirective: NgForm;

  constructor(private log: NGXLogger, private labelsService: LabelsService, private obisService: ObisService, private crudeDataService: CrudeDataService, private obisTranslateService: ObisTranslateService, private translateService: TranslateService, private snackBar: MatSnackBar) { }

  ngOnInit(): void {
    this.formGroup = new UntypedFormGroup({
      label: new UntypedFormControl('', [Validators.required]),
      register: new UntypedFormControl({ value: '', disabled: true }, [Validators.required]),
      date: new UntypedFormControl({ value: '', disabled: true }, [Validators.required]),
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

    this.formGroup.get('register').valueChanges.subscribe(selectedRegister => {
      this.registerOnChange(selectedRegister);
    });

    this.formGroup.get('date').valueChanges.subscribe(selectedMissingDate => {
      this.missingDayOnChange(selectedMissingDate);
    });
  }

  private labelOnChange(selectedLabel: string) {
    this.selectedLabel = selectedLabel;

    if (this.selectedLabel == undefined || this.selectedLabel == null) return;

    this.getControl("register").reset();
    this.getControl("date").reset();
    this.crudeValues = [];

    this.obisService.getObisCodes(this.selectedLabel).subscribe(x => {
      let obisCodes = x.obisCodes.map(oc => ({ obisCode: oc }));
      this.registers = this.obisTranslateService.AddRegisterProperty(obisCodes).sort((a, b) => a.register > b.register ? 1 : -1);

      if (this.registers.length > 0) {
        this.getControl("register").enable();
      }
    });    
  }

  private registerOnChange(selectedRegister) {
    if (selectedRegister == null) return;

    this.selectedRegister = selectedRegister.obisCode;

    if (this.selectedRegister == undefined || this.selectedRegister == null) return;

    this.getControl("date").reset();
    this.crudeValues = [];

    this.crudeDataService.getDaysMissingCrudeValues(this.selectedLabel, this.selectedRegister).subscribe(x => {
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


  private missingDayOnChange(selectedMissingDate: MissingDate) {
    if (selectedMissingDate == null) return;
    
    this.selectedMissingDate = selectedMissingDate;

    if (this.selectedMissingDate == undefined || selectedMissingDate == null) return;

    this.crudeValues = [];

    this.crudeDataService.getCrudeValuesOnDate(this.selectedLabel, this.selectedMissingDate.previousTimestamp, this.selectedRegister).subscribe(prevCrudeValue => {
      let previousCrudeValueArray= [];
      previousCrudeValueArray.push(prevCrudeValue);
      this.previousCrudeValue = this.obisTranslateService.AddRegisterProperty(previousCrudeValueArray)[0];

      this.crudeDataService.getCrudeValuesOnDate(this.selectedLabel, this.selectedMissingDate.nextTimestamp, this.selectedRegister).subscribe(nextCrudeValue => {
        let nextCrudeValueArray = [];
        nextCrudeValueArray.push(nextCrudeValue);
        this.nextCrudeValue = this.obisTranslateService.AddRegisterProperty(nextCrudeValueArray)[0];

        let contextList = [];
        contextList.push(this.previousCrudeValue);
        contextList.push(this.nextCrudeValue);
        this.crudeValues = contextList;

        this.minValue = this.previousCrudeValue.value;
        this.maxValue = this.nextCrudeValue.value;

        var valueControl = this.getControl("value");

        valueControl.clearValidators();
        valueControl.addValidators([Validators.required, Validators.min(this.minValue), Validators.max(this.maxValue)]);

        valueControl.enable();
      });
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
      scale: this.previousCrudeValue.scale,
      unit: this.previousCrudeValue.unit,
      deviceId: this.previousCrudeValue.deviceId,
      value: formGroupValue.value,
      tags: ["Manual"]
    };
    this.log.debug("Adding crude value", crudeValue);

    this.crudeDataService.addManualReading(formGroupValue.label, crudeValue).subscribe(_ => {
      this.log.debug("Add ok");
      this.translateService.get('forms.crudeData.add.confirmAdd').subscribe(message => {
        this.getControl("value").clearValidators();
        this.formDirective.resetForm();
        this.selectedLabel = null;
        this.selectedRegister = null;
        this.selectedMissingDate = null;
        this.registers = [];
        this.missingDays = [];
        this.previousCrudeValue = null;
        this.nextCrudeValue = null;
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

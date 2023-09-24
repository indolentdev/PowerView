import { Component, OnInit, ViewChild, Input, Output, OnChanges, SimpleChanges, EventEmitter } from '@angular/core';
import { UntypedFormControl, UntypedFormGroup, Validators, FormGroupDirective, NgForm } from '@angular/forms';
import { NGXLogger } from 'ngx-logger';
import { ImportCreate } from '../../../../model/importCreate';

import { Moment } from 'moment'
import * as moment from 'moment';

@Component({
  selector: 'app-settings-import-energi-data-service',
  templateUrl: './settings-import-energi-data-service.component.html',
  styleUrls: ['./settings-import-energi-data-service.component.css']
})
export class SettingsImportEnergiDataServiceComponent {
  channels = [ 'DK1', 'DK2' ]; // TODO Get these from the server..
  currencies = ['DKK', 'EUR']; // TODO: Get these from the server..

  formGroup: UntypedFormGroup;
  @ViewChild('form', { static: true }) form;

  @Input('clear') clear: string;
  @Output('addImport') addAction: EventEmitter<ImportCreate> = new EventEmitter();

  minDateFrom = moment("2020-01-01T00:00:00Z");
  maxDateFrom = moment("2080-01-01T00:00:00Z");

  constructor(private log: NGXLogger) {
  }

  ngOnInit() {
    this.formGroup = new UntypedFormGroup({
      label: new UntypedFormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(25)]),
      channel: new UntypedFormControl('', [Validators.required]),
      currency: new UntypedFormControl('', [Validators.required]),
      fromTimestamp: new UntypedFormControl('', [Validators.required])
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (this.formGroup != null) {
      this.resetForm();
    }
  }

  resetForm() {
    this.form.resetForm();
  }

  public hasError(controlName: string, errorName: string) {
    return this.formGroup.controls[controlName].hasError(errorName);
  }

  public submitForm(formGroupValue: any) {
    if (!this.formGroup.valid) {
      return;
    }

    let item: ImportCreate = formGroupValue;
    this.log.debug("Adding import", item);
    
    this.addAction.emit(item);
  }

}

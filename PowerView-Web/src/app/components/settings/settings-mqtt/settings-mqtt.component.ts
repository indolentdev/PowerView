import { Component, OnInit } from '@angular/core';
import { UntypedFormControl, UntypedFormGroup, Validators } from '@angular/forms';
import { MatSnackBar, MatSnackBarRef, SimpleSnackBar } from '@angular/material/snack-bar';
import { NGXLogger } from 'ngx-logger';
import { TranslateService } from '@ngx-translate/core';
import { SettingsService, SaveMqttParamsError, TestMqttParamsError } from '../../../services/settings.service';
import { MqttParams } from '../../../model/mqttParams';

@Component({
    selector: 'app-settings-mqtt',
    templateUrl: './settings-mqtt.component.html',
    styleUrls: ['./settings-mqtt.component.css'],
    standalone: false
})
export class SettingsMqttComponent implements OnInit {
  private snackBarRef:  MatSnackBarRef<SimpleSnackBar>;
  formGroup: UntypedFormGroup;
 
  constructor(private log: NGXLogger, private settingsService: SettingsService, private snackBar: MatSnackBar, private translateService: TranslateService) {
  }

  ngOnInit() {
    this.formGroup = new UntypedFormGroup({
      server: new UntypedFormControl('', [Validators.required, Validators.minLength(7), Validators.maxLength(39)]),
      port: new UntypedFormControl('', [Validators.required, Validators.min(1), Validators.max(65535)]),
      publishEnabled: new UntypedFormControl('', []),
      clientId: new UntypedFormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(23), Validators.pattern('^[a-zA-Z0-9]+$')])
    });
    this.getMqttParams();
  }

  private getMqttParams(): void {
    this.settingsService.getMqttParams().subscribe(mqttParams => {
      this.formGroup.controls["server"].setValue(mqttParams.server);
      this.formGroup.controls["port"].setValue(mqttParams.port);
      this.formGroup.controls["publishEnabled"].setValue(mqttParams.publishEnabled);
      this.formGroup.controls["clientId"].setValue(mqttParams.clientId);
    });
  }

  public hasError(controlName: string, errorName: string) {
    return this.formGroup.controls[controlName].hasError(errorName);
  }

  public submitForm(formGroupValue: any) {
    if (!this.formGroup.valid) {
      return;
    }

    this.dismissSnackBar();

    var mqttParams:MqttParams = formGroupValue;
    this.log.debug("Saving MQTT params", mqttParams);
    this.settingsService.saveMqttParams(mqttParams).subscribe(_ => {
      this.log.debug("Save ok");
      this.translateService.get('forms.settings.mqtt.confirmSave').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
      });
    }, err => {
      this.log.debug("Save failed", err);
      var translateIds = ['forms.settings.mqtt.errorSave'];
      var saveMqttParamsError = err as SaveMqttParamsError;
      if (saveMqttParamsError === SaveMqttParamsError.RequestContentIncomplete)
      {
        translateIds.push('forms.settings.mqtt.errorAdjustFields');
      }
      this.translateService.get(translateIds).subscribe(messages => {
        var message = "";
        for(var key in messages) {
          message +=  messages[key];
        }
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 9000 });
      });
    });
  }

  testClick() {
    this.dismissSnackBar();

    var mqttParams:MqttParams = this.formGroup.value;
    this.log.debug("Testing MQTT params", mqttParams);
    this.settingsService.testMqttParams(mqttParams).subscribe(_ => {
      this.log.debug("Test ok");
      this.translateService.get('forms.settings.mqtt.confirmTest').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
      });
    }, err => {
      this.log.debug("Test failed", err);
      var translateIds = ['forms.settings.mqtt.errorTest'];
      var testMqttParamsError = err as TestMqttParamsError;
      if (testMqttParamsError === TestMqttParamsError.RequestContentIncomplete)
      {
        translateIds.push('forms.settings.mqtt.errorAdjustFields');
      }
      if (testMqttParamsError === TestMqttParamsError.NoConnection)
      {
        translateIds.push('forms.settings.mqtt.errorTestNoConnection');
      }
      this.translateService.get(translateIds).subscribe(messages => {
        var message = "";
        for(var key in messages) {
          message +=  messages[key];
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

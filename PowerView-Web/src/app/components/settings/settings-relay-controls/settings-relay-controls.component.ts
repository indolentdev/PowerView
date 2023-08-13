import { Component, OnInit, ViewChild } from '@angular/core';
import { UntypedFormControl, UntypedFormGroup, Validators, FormGroupDirective, NgForm } from '@angular/forms';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatLegacySnackBar as MatSnackBar, MatLegacySnackBarRef as MatSnackBarRef, LegacySimpleSnackBar as SimpleSnackBar } from '@angular/material/legacy-snack-bar';
import { NGXLogger } from 'ngx-logger';
import { TranslateService } from '@ngx-translate/core';
import { ObisTranslateService } from '../../../services/obis-translate.service';
import { SettingsService, AddDisconnectRuleError } from '../../../services/settings.service';
import { DisconnectRule } from '../../../model/disconnectRule';
import { DisconnectRuleSet } from '../../../model/disconnectRuleSet';

/** Error when the parent is invalid */
class CrossFieldErrorMatcher implements ErrorStateMatcher {
  isErrorState(control: UntypedFormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    return control.dirty && form.invalid;
  }
}

@Component({
  selector: 'app-settings-relay-controls',
  templateUrl: './settings-relay-controls.component.html',
  styleUrls: ['./settings-relay-controls.component.css']
})
export class SettingsRelayControlsComponent implements OnInit {
  private snackBarRef:  MatSnackBarRef<SimpleSnackBar>;

  disconnectControlOptions: any[];
  evaluationSerieOptions: any[];
  durationMinuteOptions: number[];
  disconnectRuleSet: DisconnectRuleSet;

  private helpRelayName: string;
  private helpSerieName: string;
  private helpDuration: string;
  private helpDisconnectToConnect: string;
  private helpConnectToDisconnect: string;
  private helpUnit: string;
  helpText: string;

  formGroup: UntypedFormGroup;
  errorMatcher = new CrossFieldErrorMatcher();
  @ViewChild('form', { static: true }) form;

  constructor(private log: NGXLogger, private settingsService: SettingsService, private snackBar: MatSnackBar, private obisService: ObisTranslateService, private translateService: TranslateService) {
  }

  ngOnInit() {
    this.disconnectControlOptions = [];
    this.evaluationSerieOptions = [];
    this.durationMinuteOptions = [15, 30, 45, 60];

    this.helpRelayName = this.helpSerieName = this.helpDuration = this.helpDisconnectToConnect = this.helpConnectToDisconnect = this.helpUnit = "-";
    this.updateHelpText();

    this.formGroup = new UntypedFormGroup({
      relayName: new UntypedFormControl('', [Validators.required]),
      serieName: new UntypedFormControl('', [Validators.required]),
      durationMinutes: new UntypedFormControl('', [Validators.required]),
      disconnectToConnectValue: new UntypedFormControl('', [Validators.required, Validators.min(1), Validators.max(65535)]),
      connectToDisconnectValue: new UntypedFormControl('', [Validators.required, Validators.min(1), Validators.max(65535)])
    }, 
      this.connectToDisconnectValueLessThan
    );
    
    this.getDisconnectRuleOptions()
    this.getDisconnectRules();
  }

  connectToDisconnectValueLessThan(form: UntypedFormGroup) {
    var disconnectToConnect = form.get('disconnectToConnectValue');
    var connectToDisconnect = form.get('connectToDisconnectValue');

    if (disconnectToConnect == null || connectToDisconnect == null) {
      return null;
    }

    if (connectToDisconnect.value >= disconnectToConnect.value) {
      return { valueGreaterThanOrEqual: true };
    }

    return null;
  }

  private getDisconnectRuleOptions(): void {
    this.settingsService.getDisconnectRuleOptions().subscribe(x => { 
      this.disconnectControlOptions = this.obisService.AddSerieProperty(x.disconnectControlItems);
      this.evaluationSerieOptions = this.obisService.AddSerieProperty(x.evaluationItems);
    });
  }

  private getDisconnectRules(): void {
    this.settingsService.getDisconnectRules().subscribe(x => { 
      this.disconnectRuleSet = x; 
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

    var disconnectRule: DisconnectRule = Object.assign(
      {}, 
      ...function _flatten(o) { 
        return [].concat(...Object.keys(o)
          .map(k => 
            typeof o[k] === 'object' ?
              _flatten(o[k]) : 
              ({[k]: o[k]})
          )
        );
      }(formGroupValue)
    );
    this.log.debug("Adding relay control", disconnectRule);

    this.settingsService.addDisconnectRule(disconnectRule).subscribe(_ => {
      this.log.debug("Add ok");
      this.translateService.get('forms.settings.relayControls.confirmAdd').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
        this.form.resetForm();
        this.getDisconnectRuleOptions()
        this.getDisconnectRules();
      });
    }, err => {
      this.log.debug("Add failed", err);
      var translateIds = ['forms.settings.relayControls.errorAdd'];
      var addDisconnectRuleError = err as AddDisconnectRuleError;
      if (addDisconnectRuleError === AddDisconnectRuleError.RequestContentIncomplete)
      {
        translateIds.push('forms.settings.relayControls.errorAdjustFields');
      }
      this.translateService.get(translateIds).subscribe(messages => {
        var message = "";
        for(var key in messages) {
          message +=  messages[key];
        }
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 9000 });
      });
    })
  }

  deleteDisconnectRule(event: any) {
    var disconnectRule: DisconnectRule = event;

    if (disconnectRule == null || disconnectRule == undefined) {
      this.log.info("Skipping delete disconnect rule. Disconnect rule unspecified", event);
      return;
    }

    this.dismissSnackBar();

    this.log.debug("Deleting disconnect rule");

    this.settingsService.deleteDisconnectRule(disconnectRule.nameLabel, disconnectRule.nameObisCode).subscribe(_ => {
      this.log.debug("Delete ok");
      this.translateService.get('forms.settings.relayControls.confirmActionDelete').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
        this.getDisconnectRuleOptions()
        this.getDisconnectRules();
      });
    }, err => {
      this.log.debug("Delete failed", err);
      this.translateService.get('forms.settings.relayControls.errorActionDelete').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 9000 });
      });
    });
  }

  private dismissSnackBar(): void {
    if (this.snackBarRef != null) {
      this.snackBarRef.dismiss();
    }
  }

  inputChanged(event) {
    var id = event && event.target && event.target.id || null;
    var displayValue = event && event.target && event.target.value || null;
    this.updateHelp(id, displayValue);
  }
    
  optionSelected(event) {
    var id = event && event.source && event.source._id || null;
    var displayValue = event && event.source && event.source.selected && event.source.selected._element &&
      event.source.selected._element.nativeElement && event.source.selected._element.nativeElement.innerText &&
      event.source.selected._element.nativeElement && event.source.selected._element.nativeElement.innerText.trim() || null;
    this.updateHelp(id, displayValue);

    if (id === "serieName") {
      var unitDisplayValue = event && event.source && event.source._value && event.source._value.unit || null;
      if (unitDisplayValue != null) {
        this.updateHelp("unit", unitDisplayValue);
      }
    }
  }

  private updateHelp(id: string, displayValue: string) {
    if (id == null || displayValue == null) {
      return;
    }

    switch (id) {
      case "relayName": 
        this.helpRelayName = displayValue;
        break;
      case "serieName":
        this.helpSerieName = displayValue;
        break;
      case "durationMinutes":
        this.helpDuration = displayValue;
        break;
      case "disconnectToConnectValue":
        this.helpDisconnectToConnect = displayValue;
        break;
      case "connectToDisconnectValue":
        this.helpConnectToDisconnect = displayValue
        break;
      case "unit":
        this.helpUnit = displayValue;
        break;
    }
    this.updateHelpText();
  }

  private updateHelpText() {
    var hintParam = {
      relayName: this.helpRelayName,
      serieName: this.helpSerieName,
      duration: this.helpDuration,
      disconnectToConnect: this.helpDisconnectToConnect,
      connectToDisconnect: this.helpConnectToDisconnect,
      unit: this.helpUnit
    };
    this.translateService.get('forms.settings.relayControls.formHint', hintParam).subscribe(message => {
      this.helpText = message;
    });
  }  
}


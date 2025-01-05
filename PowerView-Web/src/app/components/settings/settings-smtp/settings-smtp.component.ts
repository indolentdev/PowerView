import { Component, OnInit } from '@angular/core';
import { UntypedFormControl, UntypedFormGroup, Validators } from '@angular/forms';
import { MatSnackBar, MatSnackBarRef, SimpleSnackBar } from '@angular/material/snack-bar';
import { NGXLogger } from 'ngx-logger';
import { TranslateService } from '@ngx-translate/core';
import { SettingsService, SaveSmtpParamsError } from '../../../services/settings.service';
import { SmtpParams } from '../../../model/smtpParams';

@Component({
    selector: 'app-settings-smtp',
    templateUrl: './settings-smtp.component.html',
    styleUrls: ['./settings-smtp.component.css'],
    standalone: false
})
export class SettingsSmtpComponent implements OnInit {
  private snackBarRef:  MatSnackBarRef<SimpleSnackBar>;
  formGroup: UntypedFormGroup;

  constructor(private log: NGXLogger, private settingsService: SettingsService, private snackBar: MatSnackBar, private translateService: TranslateService) {
  }

  ngOnInit() {
    this.formGroup = new UntypedFormGroup({
      server: new UntypedFormControl('', [Validators.required, Validators.minLength(7), Validators.maxLength(39)]),
      port: new UntypedFormControl('', [Validators.required, Validators.min(1), Validators.max(65535)]),
      user: new UntypedFormControl('', [Validators.required, Validators.minLength(7), Validators.maxLength(39)]),
      auth: new UntypedFormControl('', [Validators.required, Validators.minLength(7), Validators.maxLength(39)]),
      email: new UntypedFormControl('', [Validators.required, Validators.minLength(7), Validators.maxLength(39)])
    });
    this.getSmtpParams();
  }

  private getSmtpParams(): void {    
    this.settingsService.getSmtpParams().subscribe(smtpParams => {
      this.formGroup.controls["server"].setValue(smtpParams.server);
      this.formGroup.controls["port"].setValue(smtpParams.port);
      this.formGroup.controls["user"].setValue(smtpParams.user);
      this.formGroup.controls["auth"].setValue(smtpParams.auth);
      this.formGroup.controls["email"].setValue(smtpParams.email);
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

    var smtpParams:SmtpParams = formGroupValue;
    this.log.debug("Saving SMTP params", smtpParams.server, smtpParams.port, smtpParams.user, smtpParams.email);

    this.settingsService.saveSmtpParams(smtpParams).subscribe(_ => {
      this.log.debug("Save ok");
      this.translateService.get('forms.settings.smtp.confirmSave').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
      });
    }, err => {
      this.log.debug("Save failed", err);
      var translateIds = ['forms.settings.smtp.errorSave'];
      var saveSmtpParamsError = err as SaveSmtpParamsError;
      if (saveSmtpParamsError === SaveSmtpParamsError.RequestContentIncomplete)
      {
        translateIds.push('forms.settings.smtp.errorAdjustFields');
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

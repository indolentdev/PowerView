import { Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup, Validators, FormGroupDirective, NgForm } from '@angular/forms';
import { MatSnackBar, MatSnackBarRef, SimpleSnackBar, ErrorStateMatcher } from '@angular/material';
import { NGXLogger } from 'ngx-logger';
import { TranslateService } from '@ngx-translate/core';
import { SerieService } from '../services/serie.service';
import { SettingsService, AddDisconnectRuleError, TestEmailRecipientError } from '../services/settings.service';
import { EmailRecipient } from '../model/emailRecipient';
import { EmailRecipientSet } from '../model/emailRecipientSet';

@Component({
  selector: 'app-settings-recipients',
  templateUrl: './settings-recipients.component.html',
  styleUrls: ['./settings-recipients.component.css']
})
export class SettingsRecipientsComponent implements OnInit {
  private snackBarRef:  MatSnackBarRef<SimpleSnackBar>;
  emailRecipientSet: EmailRecipientSet;
  formGroup: FormGroup;
  @ViewChild('form') form;

  constructor(private log: NGXLogger, private settingsService: SettingsService, private snackBar: MatSnackBar, private serieService: SerieService, private translateService: TranslateService) {
  }

  ngOnInit() {
    this.formGroup = new FormGroup({
      name: new FormControl('', [Validators.required, Validators.minLength(3), Validators.max(39)]),
      emailAddress: new FormControl('', [Validators.required, Validators.minLength(3), Validators.max(39)]),
    });
    
    this.getEmailRecipients();
  }

  private getEmailRecipients(): void {
    this.settingsService.getEmailRecipients().subscribe(x => { 
      this.emailRecipientSet = x; 
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

    var emailRecipeint: EmailRecipient = formGroupValue;
    this.log.debug("Adding email recipient", emailRecipeint);

    this.settingsService.addEmailRecipient(emailRecipeint).subscribe(_ => {
      this.log.debug("Add ok");
      this.translateService.get('forms.settings.emailRecipients.confirmAdd').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
        this.form.resetForm();
        this.getEmailRecipients();
      });
    }, err => {
      this.log.debug("Add failed", err);
      var translateIds = ['forms.settings.emailRecipients.errorAdd'];
      var addEmailRecipientError = err as AddDisconnectRuleError;
      if (addEmailRecipientError === AddDisconnectRuleError.RequestContentDuplicate)
      {
        translateIds.push('forms.settings.emailRecipients.errorAdjustEmailField');
      }
      if (addEmailRecipientError === AddDisconnectRuleError.RequestContentIncomplete)
      {
        translateIds.push('forms.settings.emailRecipients.errorAdjustFields');
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
  
  deleteEmailRecipient(event: any) {
    var emailRecipient: EmailRecipient = event;

    if (emailRecipient == null || emailRecipient == undefined) {
      this.log.info("Skipping delete email recipient. Email recipient unspecified", event);
      return;
    }

    this.dismissSnackBar();

    this.log.debug("Deleting email recipient");
    this.settingsService.deleteEmailRecipient(emailRecipient.emailAddress).subscribe(_ => {
      this.log.debug("Delete ok");
      this.translateService.get('forms.settings.emailRecipients.confirmActionDelete').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
        this.getEmailRecipients();
      });
    }, err => {
      this.log.debug("Delete failed", err);
      this.translateService.get('forms.settings.emailRecipients.errorActionDelete').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 9000 });
      });
    });
  }

  testEmailRecipient(event: any) {
    var emailRecipient: EmailRecipient = event;

    if (emailRecipient == null || emailRecipient == undefined) {
      this.log.info("Skipping test email recipient. Email recipient unspecified", event);
      return;
    }

    this.dismissSnackBar();

    this.log.debug("Testing email recipient");
    this.settingsService.testEmailRecipient(emailRecipient.emailAddress).subscribe(_ => {
      this.log.debug("Test ok");
      this.translateService.get('forms.settings.emailRecipients.confirmActionTest').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
      });
    }, err => {
      this.log.debug("Test failed", err);
      var translateIds = ['forms.settings.emailRecipients.errorActionTest'];
      var testEmailRecipientError = err as TestEmailRecipientError;
      if (testEmailRecipientError === TestEmailRecipientError.EmailServerConnectionFailed)
      {
        translateIds.push('forms.settings.emailRecipients.errorSmtpConnection');
      }
      if (testEmailRecipientError === TestEmailRecipientError.EmailServerAuthenticationFailed)
      {
        translateIds.push('forms.settings.emailRecipients.errorSmtpAuthentification');
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


import { Component, OnInit, ViewChild } from '@angular/core';
import { UntypedFormControl, UntypedFormGroup, Validators, FormGroupDirective, NgForm } from '@angular/forms';
import { MatDialog, MatDialogConfig, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ConfirmComponent } from '../../../confirm/confirm.component'
import { MatSnackBar, MatSnackBarRef, SimpleSnackBar } from '@angular/material/snack-bar';
import { NGXLogger } from 'ngx-logger';
import { TranslateService } from '@ngx-translate/core';
import { SettingsService, AddCostBreakdownError } from '../../../../services/settings.service';
import { CostBreakdown } from '../../../../model/costBreakdown';
import { CostBreakdownEntry } from '../../../../model/costBreakdownEntry';

@Component({
  selector: 'app-settings-cost-breakdown',
  templateUrl: './settings-cost-breakdown.component.html',
  styleUrls: ['./settings-cost-breakdown.component.css']
})
export class SettingsCostBreakdownComponent {
  private snackBarRef: MatSnackBarRef<SimpleSnackBar>;

  formGroup: UntypedFormGroup;
  @ViewChild('form', { static: true }) form;

  costBreakdowns: CostBreakdown[];
  selectedCostBreakdown: CostBreakdown;
  selectedCostBreakdownTitle: string;

  constructor(private log: NGXLogger, public dialog: MatDialog, private settingsService: SettingsService, private snackBar: MatSnackBar, private translateService: TranslateService) {    
  }

  ngOnInit() {
    this.costBreakdowns = [];

    this.formGroup = new UntypedFormGroup({
      title: new UntypedFormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(25)]),
      vat: new UntypedFormControl('', [Validators.required]),
    });

    this.getCostBreakdowns();
  }

  private getCostBreakdowns(): void {
    this.settingsService.getCostBreakdowns().subscribe(x => {
      var sorted = x.costBreakdowns.sort(function (obj1, obj2) {
        if (obj1.title === obj2.title) {
          return 0;
        }
        else {
          return obj1.title < obj2.title ? -1 : 1;
        }
      });

      this.costBreakdowns = sorted;
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

    let costBreakdown: CostBreakdown = formGroupValue;
    this.log.debug("Adding cost breakdown", costBreakdown);

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
  }
  
  resetForm() {
    this.form.resetForm();
    this.selectedCostBreakdown = null;
    this.selectedCostBreakdownTitle = null;
    this.getCostBreakdowns();
  }

  selectCostBreakdown(event: any) {
    let costBreakdown: CostBreakdown = event;

    if (costBreakdown == null || costBreakdown == undefined) {
      this.log.info("Skipping select cost breakdown. Cost breakdown unspecified", event);
      return;
    }

    this.selectedCostBreakdown = costBreakdown;
    this.selectedCostBreakdownTitle = costBreakdown.title;
  }

  deleteCostBreakdown(event: any) {
    let costBreakdown: CostBreakdown = event;

    if (costBreakdown == null || costBreakdown == undefined) {
      this.log.info("Skipping delete cost breakdown. Cost breakdown unspecified", event);
      return;
    }

    this.dismissSnackBar();

    const dialogConfig = new MatDialogConfig();
    dialogConfig.autoFocus = true;
    dialogConfig.data = { title: 'headings.costBreakdownDelete', message: 'forms.settings.pricing.costBreakdown.deleteMessage', placeholderConfirm: 'forms.settings.pricing.costBreakdown.placeholderDelete', confirm: costBreakdown.title };

    const dialogRef = this.dialog.open(ConfirmComponent, dialogConfig);

    dialogRef.afterClosed().subscribe(result => {
      if (!(result == costBreakdown.title)) return;

      this.log.debug("Deleting cost breakdown");

      this.settingsService.deleteCostBreakdown(costBreakdown.title).subscribe(_ => {
        this.log.debug("Delete ok");
        this.translateService.get('forms.settings.pricing.costBreakdown.confirmActionDelete').subscribe(message => {
          this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
          this.getCostBreakdowns();
        });
      }, err => {
        this.log.debug("Delete failed", err);
        this.translateService.get('forms.settings.pricing.costBreakdown.errorActionDelete').subscribe(message => {
          this.snackBarRef = this.snackBar.open(message, undefined, { duration: 9000 });
        });
      });

    });

  }

  addedCostBreakdownEntry(event: any) {
    let costBreakdownEntry: CostBreakdownEntry = event;


    // TODO: Refresh stuff..
  }

  private dismissSnackBar(): void {
    if (this.snackBarRef != null) {
      this.snackBarRef.dismiss();
    }
  }

}
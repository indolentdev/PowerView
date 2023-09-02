import { Component, OnInit, ViewChild } from '@angular/core';
import { UntypedFormControl, UntypedFormGroup, Validators, FormGroupDirective, NgForm } from '@angular/forms';
import { MatDialog, MatDialogConfig, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ConfirmComponent } from '../../../confirm/confirm.component'
import { MatSnackBar, MatSnackBarRef, SimpleSnackBar } from '@angular/material/snack-bar';
import { NGXLogger } from 'ngx-logger';
import { TranslateService } from '@ngx-translate/core';
import { SettingsService, AddCostBreakdownError, AddCostBreakdownEntryError, UpdateCostBreakdownEntryError } from '../../../../services/settings.service';
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

  currencies = ['DKK', 'EUR']; // TODO: Get these from the server..

  costBreakdowns: CostBreakdown[];
  selectedCostBreakdown: CostBreakdown;
  selectedCostBreakdownTitle: string;
  entryClear: string; // changing this will cause the entry-component to clear itself

  entryCreateMode: boolean;
  costBreakdownEntryEdit: CostBreakdownEntry;

  constructor(private log: NGXLogger, public dialog: MatDialog, private settingsService: SettingsService, private snackBar: MatSnackBar, private translateService: TranslateService) {    
  }

  ngOnInit() {
    this.costBreakdowns = [];

    this.entryCreateMode = true;
    this.costBreakdownEntryEdit = null;

    this.formGroup = new UntypedFormGroup({
      title: new UntypedFormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(25)]),
      currency: new UntypedFormControl('', [Validators.required]),
      vat: new UntypedFormControl('', [Validators.required, Validators.min(0), Validators.max(100)]),
    });

    this.getCostBreakdowns();
  }

  reset() {
    this.form.resetForm();
    this.selectedCostBreakdown = null;
    this.getCostBreakdowns();

    this.entryCreateMode = true;
    this.costBreakdownEntryEdit = null;
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
      this.autoSelectCostBreakdown();
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
        this.selectedCostBreakdownTitle = null;
        this.reset();
      });
    }, err => {
      this.log.debug("Add failed", err);
      var translateIds = ['forms.settings.pricing.costBreakdown.errorAdd'];
      var addCostBreakdownError = err as AddCostBreakdownError;
      if (addCostBreakdownError === AddCostBreakdownError.RequestContentIncomplete || addCostBreakdownError === AddCostBreakdownError.RequestContentDuplicate) {
        translateIds.push('forms.settings.pricing.costBreakdown.errorAdjustFields');
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
  
  selectCostBreakdown(event: any) {
    let costBreakdown: CostBreakdown = event;

    if (costBreakdown == null || costBreakdown == undefined) {
      this.log.info("Skipping select cost breakdown. Cost breakdown unspecified", event);
      return;
    }

    this.selectedCostBreakdownTitle = costBreakdown.title;
    this.autoSelectCostBreakdown();
  }

  autoSelectCostBreakdown() {
    if (this.selectedCostBreakdownTitle == null) {
      return;
    }

    let costBreakdown = this.costBreakdowns.find((x) => x.title == this.selectedCostBreakdownTitle);

    // TODO: Sort entries
/*    
    var sortedEntries = costBreakdown.entries.sort(function (obj1, obj2) {
      if (obj1.title === obj2.title) {
        return 0;
      }
      else {
        return obj1.title < obj2.title ? -1 : 1;
      }
    });

    this.costBreakdowns = sorted;
*/
    this.selectedCostBreakdown = costBreakdown;
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
          this.selectedCostBreakdownTitle = null;
          this.reset();
        });
      }, err => {
        this.log.debug("Delete failed", err);
        this.translateService.get('forms.settings.pricing.costBreakdown.errorActionDelete').subscribe(message => {
          this.snackBarRef = this.snackBar.open(message, undefined, { duration: 9000 });
        });
      });
    });

  }

  addCostBreakdownEntry(event: any) {
    let costBreakdownEntry: CostBreakdownEntry = event;

    this.dismissSnackBar();

    this.log.debug("Adding cost breakdown entry", costBreakdownEntry);

    if (this.selectedCostBreakdownTitle == null) {
      this.log.debug("No selected cost breakdown. Skipping add entry.");
      return;
    }

    this.settingsService.addCostBreakdownEntry(this.selectedCostBreakdownTitle, costBreakdownEntry).subscribe(_ => {
      this.log.debug("Add ok");
      this.translateService.get('forms.settings.pricing.costBreakdown.confirmAddEntry').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
        this.entryClear = crypto.randomUUID();
        this.reset();
      });
    }, err => {
      this.log.debug("Add failed", err);
      var translateIds = ['forms.settings.pricing.costBreakdown.errorAddEntry'];
      var addCostBreakdownEntryError = err as AddCostBreakdownEntryError;
      if (addCostBreakdownEntryError === AddCostBreakdownEntryError.RequestContentIncomplete || addCostBreakdownEntryError === AddCostBreakdownEntryError.RequestContentDuplicate ) {
        translateIds.push('forms.settings.pricing.costBreakdown.errorAdjustEntryFields');
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

  updateCostBreakdownEntry(event: any) {
    let costBreakdownEntry: CostBreakdownEntry = event;

    this.dismissSnackBar();

    if (this.selectedCostBreakdownTitle == null) {
      this.log.debug("No selected cost breakdown. Skipping update entry.");
      return;
    }

    if (this.costBreakdownEntryEdit == null) {
      this.log.debug("No edit-selected cost breakdown entry. Skipping update entry.");
      return;
    }

    this.log.debug("Updating cost breakdown entry", costBreakdownEntry);

    this.settingsService.updateCostBreakdownEntry(this.selectedCostBreakdownTitle, this.costBreakdownEntryEdit.fromDate, this.costBreakdownEntryEdit.toDate, this.costBreakdownEntryEdit.name, costBreakdownEntry).subscribe(_ => {
      this.log.debug("Update ok");
      this.translateService.get('forms.settings.pricing.costBreakdown.confirmUpdateEntry').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
        this.entryClear = crypto.randomUUID();
        this.reset();
      });
    }, err => {
      this.log.debug("Update failed", err);
      var translateIds = ['forms.settings.pricing.costBreakdown.errorUpdateEntry'];
      var updateCostBreakdownEntryError = err as UpdateCostBreakdownEntryError;
      if (updateCostBreakdownEntryError === UpdateCostBreakdownEntryError.RequestContentIncomplete || updateCostBreakdownEntryError === UpdateCostBreakdownEntryError.RequestContentDuplicate) {
        translateIds.push('forms.settings.pricing.costBreakdown.errorAdjustEntryFields');
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

  deleteCostBreakdownEntry(event: any) {
    let costBreakdownEntry: CostBreakdownEntry = event;

    if (costBreakdownEntry == null || costBreakdownEntry == undefined) {
      this.log.info("Skipping delete cost breakdown entry. Cost breakdown entry unspecified", event);
      return;
    }

    if (this.selectedCostBreakdown == null) {
      this.log.info("Skipping delete cost breakdown entry. No selected cost breakdown");
    }

    this.dismissSnackBar();

    this.log.debug("Deleting cost breakdown entry");

    this.settingsService.deleteCostBreakdownEntry(this.selectedCostBreakdown.title, costBreakdownEntry.fromDate, costBreakdownEntry.toDate, costBreakdownEntry.name).subscribe(_ => {
      this.log.debug("Delete ok");
      this.translateService.get('forms.settings.pricing.costBreakdown.confirmActionDeleteEntry').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
        this.reset();
      });
    }, err => {
      this.log.debug("Delete failed", err);
      this.translateService.get('forms.settings.pricing.costBreakdown.errorActionDeleteEntry').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 9000 });
      });
    });

  }

  editCostBreakdownEntry(event: any) {
    let costBreakdownEntry: CostBreakdownEntry = event;

    this.costBreakdownEntryEdit = costBreakdownEntry;
    this.entryCreateMode = false;
  }

  private dismissSnackBar(): void {
    if (this.snackBarRef != null) {
      this.snackBarRef.dismiss();
    }
  }

}
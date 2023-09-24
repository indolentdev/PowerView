import { Component, OnInit, ViewChild } from '@angular/core';
import { MatSnackBar, MatSnackBarRef, SimpleSnackBar } from '@angular/material/snack-bar';
import { NGXLogger } from 'ngx-logger';
import { TranslateService } from '@ngx-translate/core';
import { SettingsService, AddImportError, DeleteImportError } from '../../../../services/settings.service';
import { Import } from 'src/app/model/import';
import { ImportCreate } from 'src/app/model/importCreate';

@Component({
  selector: 'app-settings-import',
  templateUrl: './settings-import.component.html',
  styleUrls: ['./settings-import.component.css']
})
export class SettingsImportComponent {
  private snackBarRef: MatSnackBarRef<SimpleSnackBar>;

  imports: Import[];
  entryClear: string; // changing this will cause the entry-component to clear itself

  constructor(private log: NGXLogger, private settingsService: SettingsService, private snackBar: MatSnackBar, private translateService: TranslateService) {
  }

  ngOnInit() {
    this.imports = [];
    this.getImports();
  }

  private getImports(): void {
    this.settingsService.getImports().subscribe(x => {
      this.imports = x.imports;
    });
  }

  addImport(event: any) {
    let importCreate: ImportCreate = event;

    this.dismissSnackBar();

    this.log.debug("Adding import", importCreate);

    this.settingsService.addImport(importCreate).subscribe(_ => {
      this.log.debug("Add ok");
      this.translateService.get('forms.settings.pricing.import.confirmAdd').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
        this.entryClear = Math.random().toString(10);
      });
    }, err => {
      this.log.debug("Add failed", err);
      var translateIds = ['forms.settings.pricing.import.errorAdd'];
      var addImportError = err as AddImportError;
      if (addImportError === AddImportError.RequestContentIncomplete || addImportError === AddImportError.RequestContentDuplicate) {
        translateIds.push('forms.settings.pricing.import.errorAdjustEntryFields');
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

  deleteImport(event: any) {
    let imprt: Import = event;

    this.dismissSnackBar();

    this.log.debug("Deleting import", imprt);

    this.settingsService.deleteImport(imprt.label).subscribe(_ => {
      this.log.debug("Delete ok");
      this.translateService.get('forms.settings.pricing.import.confirmDelete').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
      });
    }, err => {
      this.log.debug("Delete failed", err);
      var translateIds = ['forms.settings.pricing.import.errorDelete'];
      this.translateService.get(translateIds).subscribe(messages => {
        var message = "";
        for (var key in messages) {
          message += messages[key];
        }
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 9000 });
      });
    });
  }

  toggleImport(event: any) {
    let imprt: Import = event;

    this.dismissSnackBar();

    this.log.debug("Toggle import", imprt);

    this.settingsService.toggleImport(imprt.label, imprt.enabled).subscribe(_ => {
      this.log.debug("Toggle ok");
      this.translateService.get('forms.settings.pricing.import.confirmUpdate').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
      });
    }, err => {
      this.log.debug("Toggle failed", err);
      var translateIds = ['forms.settings.pricing.import.errorUpdate'];
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

import { Component, OnInit, Input, OnChanges, SimpleChanges } from '@angular/core';
import { UntypedFormControl, UntypedFormGroup, Validators, FormGroupDirective, NgForm } from '@angular/forms';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatDialog, MatDialogConfig, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ConfirmComponent } from '../../confirm/confirm.component'
import { MatSnackBar, MatSnackBarRef, SimpleSnackBar } from '@angular/material/snack-bar';
import { NGXLogger } from 'ngx-logger';
import { TranslateService } from '@ngx-translate/core';
import { CrudeDataService } from '../../../services/crude-data.service';
import { CrudeValueSet } from '../../../model/crudeValueSet';
import { CrudeValue } from '../../../model/crudeValue';

import { Moment } from 'moment'
import * as moment from 'moment';

@Component({
  selector: 'app-data-crude-bydate-table',
  templateUrl: './data-crude-bydate-table.component.html',
  styleUrls: ['./data-crude-bydate-table.component.css']
})
export class DataCrudeBydateTableComponent implements OnInit, OnChanges {
  private snackBarRef: MatSnackBarRef<SimpleSnackBar>;

  @Input('label') label: string;
  @Input('from') from: Moment;

  crudeValueSet: CrudeValueSet;

  constructor(private log: NGXLogger, public dialog: MatDialog, private snackBar: MatSnackBar, private crudeDataService: CrudeDataService, private translateService: TranslateService) {
  }

  ngOnInit() {
    this.refresh();
  }

  ngOnChanges(changes: SimpleChanges) {
    this.refresh();
  }

  private refresh(): void {
    if (this.label == null || this.from == null) return;

    this.crudeDataService.getCrudeValues(this.label, this.from).subscribe(x => {
      this.crudeValueSet = x;
    });
  }

  deleteCrudeValue(event: any) {
    var crudeValue: CrudeValue = event;

    if (crudeValue == null || crudeValue == undefined) {
      this.log.info("Skipping delete crude value. Crude value unspecified", event);
      return;
    }

    this.dismissSnackBar();

    const dialogConfig = new MatDialogConfig();
    dialogConfig.autoFocus = true;
    dialogConfig.data = { title: 'headings.crudeDataDelete', message: 'forms.crudeData.list.deleteMessage', confirm: this.label };

    const dialogRef = this.dialog.open(ConfirmComponent, dialogConfig);

    dialogRef.afterClosed().subscribe(result => {
      if (!(result == this.label)) return;

      this.log.debug("Deleting crude value");
      this.crudeDataService.deleteCrudeValue(this.label, crudeValue.timestamp, crudeValue.obisCode).subscribe(_ => {
        this.log.debug("Delete ok");
        this.translateService.get('forms.crudeData.list.confirmActionDelete').subscribe(message => {
          this.snackBarRef = this.snackBar.open(message, undefined, { duration: 9000 });
          this.refresh();
        });
      }, err => {
        this.log.debug("Delete failed", err);
        this.translateService.get('forms.crudeData.list.errorActionDelete').subscribe(message => {
          this.snackBarRef = this.snackBar.open(message, undefined, { duration: 9000 });
        });
      });

    });

  }

  private dismissSnackBar(): void {
    if (this.snackBarRef != null) {
      this.snackBarRef.dismiss();
    }
  }

}

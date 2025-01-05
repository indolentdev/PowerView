import { Component, OnInit } from '@angular/core';
import { MatSnackBar, MatSnackBarRef, SimpleSnackBar } from '@angular/material/snack-bar';
import { NGXLogger } from 'ngx-logger';
import { TranslateService } from '@ngx-translate/core';
import { SettingsService } from '../../../services/settings.service';
import { SerieColorSet } from '../../../model/serieColorSet';

@Component({
    selector: 'app-settings-series-colors',
    templateUrl: './settings-series-colors.component.html',
    styleUrls: ['./settings-series-colors.component.css'],
    standalone: false
})
export class SettingsSeriesColorsComponent implements OnInit {
  private snackBarRef:  MatSnackBarRef<SimpleSnackBar>;
  serieColorSet: SerieColorSet;

  constructor(private log: NGXLogger, private settingsService: SettingsService, private snackBar: MatSnackBar, private translateService: TranslateService) {
  }

  ngOnInit() {
    this.getSerieColors();
  }

  private getSerieColors(): void {
    this.settingsService.getSerieColors().subscribe(x => { 
      this.serieColorSet = x; 
    });
  }

  saveClick() {
    if (this.snackBarRef != null) {
      this.snackBarRef.dismiss();
    }

    this.log.debug("Saving", this.serieColorSet);

    this.settingsService.saveSerieColors(this.serieColorSet).subscribe(_ => {
      this.log.debug("Save ok");
      this.translateService.get('forms.settings.serieColors.confirmSave').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
      });
    }, err => {
      this.log.debug("Save failed", err);
      this.translateService.get('forms.settings.serieColors.errorSave').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 9000 });
      });
    });
  }

}

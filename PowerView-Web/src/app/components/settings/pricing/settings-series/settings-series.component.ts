import { Component, OnInit, ViewChild, Input, Output, OnChanges, SimpleChanges, EventEmitter } from '@angular/core';
import { MatSnackBar, MatSnackBarRef, SimpleSnackBar } from '@angular/material/snack-bar';
import { UntypedFormControl, UntypedFormGroup, Validators, FormGroupDirective, NgForm } from '@angular/forms';
import { NGXLogger } from 'ngx-logger';
import { CostBreakdownWithPeriod } from '../../../../model/costBreakdownWithPeriod';
import { GeneratorSeries } from '../../../../model/generatorSeries';
import { GeneratorSeriesSet } from '../../../../model/generatorSeriesSet';
import { SerieName } from '../../../../model/serieName';
import { ObisTranslateService } from '../../../../services/obis-translate.service';
import { TranslateService } from '@ngx-translate/core';
import { SettingsService, AddGeneratorSeriesError, DeleteImportError } from '../../../../services/settings.service';

import { Moment } from 'moment'
import * as moment from 'moment';

@Component({
  selector: 'app-settings-series',
  templateUrl: './settings-series.component.html',
  styleUrls: ['./settings-series.component.css']
})
export class SettingsSeriesComponent {
  private snackBarRef: MatSnackBarRef<SimpleSnackBar>;

  formGroup: UntypedFormGroup;
  @ViewChild('form', { static: true }) form;

  generatorSeries: GeneratorSeries[];
  costBreakdowns: CostBreakdownWithPeriod[];
  baseSeries: SerieName[];

  constructor(private log: NGXLogger, private settingsService: SettingsService, private snackBar: MatSnackBar, private obisTranslateService: ObisTranslateService, private translateService: TranslateService) {
  }

  ngOnInit() {
    this.generatorSeries = [];
    this.baseSeries = [];

    this.formGroup = new UntypedFormGroup({
      costBreakdown: new UntypedFormControl('', [Validators.required]),
      series: new UntypedFormControl('', [Validators.required]),
      label: new UntypedFormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(25)])
    });

    this.refresh();
  }

  refresh() {
    this.settingsService.getGeneratorsSeries().subscribe(x => {
      let withNameSeries = this.obisTranslateService.AddSerieProperty(x.items, "nameLabel", "nameObisCode", "nameSeries");
      let withNameAndBaseSeries = this.obisTranslateService.AddSerieProperty(withNameSeries, "baseLabel", "baseObisCode", "baseSeries");
      this.generatorSeries = withNameAndBaseSeries
    });

    this.settingsService.getGeneratorsBaseSeries().subscribe(x => {
      let series = this.obisTranslateService.AddSerieProperty(x.items, "baseLabel", "baseObisCode");
      this.baseSeries = series;
    });

    this.settingsService.getCostBreakdowns().subscribe(x => {
      this.costBreakdowns = x.costBreakdowns;
    });
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

    this.dismissSnackBar();

    let generatorSeries = {
      nameLabel: formGroupValue.label,
      nameObisCode: formGroupValue.series.obisCode,
      baseLabel: formGroupValue.series.baseLabel,
      baseObisCode: formGroupValue.series.baseObisCode,
      costBreakdownTitle: formGroupValue.costBreakdown.title
    };

    this.log.debug("Adding generator series", generatorSeries);

    this.settingsService.addGeneratorsSeries(generatorSeries).subscribe(_ => {
      this.log.debug("Add ok");
      this.translateService.get('forms.settings.pricing.series.confirmAdd').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 9000 });
        this.resetForm();
        this.refresh();
      });
    }, err => {
      this.log.debug("Add failed", err);
      var translateIds = ['forms.settings.pricing.series.errorAdd'];
      var addError = err as AddGeneratorSeriesError;
      if (addError === AddGeneratorSeriesError.RequestContentIncomplete || addError === AddGeneratorSeriesError.RequestContentDuplicate) {
        translateIds.push('forms.settings.pricing.series.errorAdjustEntryFields');
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

  deleteGeneratorSeries(event: any) {
    let generatorSeries: GeneratorSeries = event;

    if (generatorSeries == null || generatorSeries == undefined) {
      this.log.info("Skipping delete generator series. Generator series unspecified", event);
      return;
    }

    this.dismissSnackBar();

    this.log.debug("Deleting generator series");

    this.settingsService.deleteGeneratorSeries(generatorSeries.nameLabel, generatorSeries.nameObisCode).subscribe(_ => {
      this.log.debug("Delete ok");
      this.translateService.get('forms.settings.pricing.series.confirmDelete').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
        this.refresh();
      });
    }, err => {
      this.log.debug("Delete failed", err);
      this.translateService.get('forms.settings.pricing.series.errorDelete').subscribe(message => {
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

import { Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup, Validators, FormGroupDirective, NgForm } from '@angular/forms';
import { MatSnackBar, MatSnackBarRef, SimpleSnackBar } from '@angular/material';
import { NGXLogger } from 'ngx-logger';
import { TranslateService } from '@ngx-translate/core';
import { SerieService } from '../services/serie.service';
import { SettingsService, AddProfileGraphError } from '../services/settings.service';
import { MenuService } from '../services/menu.service';
import { ProfileGraph } from '../model/profileGraph';
import { ProfileGraphSerieSet } from '../model/profileGraphSerieSet';

@Component({
  selector: 'app-settings-profile-graphs',
  templateUrl: './settings-profile-graphs.component.html',
  styleUrls: ['./settings-profile-graphs.component.css']
})
export class SettingsProfileGraphsComponent implements OnInit {
  private snackBarRef:  MatSnackBarRef<SimpleSnackBar>;

  profileGraphPeriods: string[];
  profileGraphIntervals: any[];
  profileGraphSeries: any[];

  profileGraphSerieSet: ProfileGraphSerieSet;
  profileGraphGroups: ProfileGraph[][];

  formGroup: FormGroup;
  @ViewChild('form') form;

  constructor(private log: NGXLogger, private settingsService: SettingsService, private snackBar: MatSnackBar, private translateService: TranslateService, private serieService: SerieService, private menuService: MenuService) {
  }

  ngOnInit() {
    this.profileGraphPeriods = [ "day", "month", "year" ];
    this.profileGraphIntervals = [];
    this.profileGraphSeries = [];
  
    this.formGroup = new FormGroup({
      period: new FormControl('', [Validators.required]),
      page: new FormControl('', [Validators.maxLength(25)]),
      title: new FormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(25)]),
      interval: new FormControl({value:'', disabled:true}, [Validators.required]),
      series: new FormControl({value:'', disabled:true}, [Validators.required])
    });

    this.getProfileGraphsSeries();
    this.getProfileGraphs();

    this.onChanges();
  }

  private onChanges(): void {
    this.formGroup.get('period').valueChanges
    .subscribe(selectedPeriod => {
      this.onChangesPeriodAffectsIntervals(selectedPeriod);
      this.onChangesPeriodAffectsSeries(selectedPeriod);
    });
  }

  private onChangesPeriodAffectsIntervals(selectedPeriod: string): void {
    var intervalControl = this.formGroup.get('interval');
    intervalControl.enable();
    intervalControl.reset();

    var pgis = this.getProfileGraphIntervals().filter(x => x.period === selectedPeriod);
    for (let pgi of pgis) {
      var intervalVars = pgi.value.split("-");
      pgi.intervalValue = intervalVars[0];
      pgi.intervalUnit = intervalVars[1];
    }
    this.profileGraphIntervals = pgis;

    var defaultIntervals = this.profileGraphIntervals.filter(x => x.default);
    if (defaultIntervals.length > 0) {
      intervalControl.setValue(defaultIntervals[0].value);
    }
  }

  private onChangesPeriodAffectsSeries(selectedPeriod: string): void {
    var seriesControl = this.formGroup.get('series');
    seriesControl.enable();
    seriesControl.reset();

    var profileGraphSeriesForPeriod = this.profileGraphSerieSet.items.filter(x => x.period === selectedPeriod);
    this.profileGraphSeries = this.serieService.AddSerieProperty(profileGraphSeriesForPeriod);
  }

  private getProfileGraphsSeries(): void {
    this.settingsService.getProfileGraphsSeries().subscribe(x => { 
      this.profileGraphSerieSet = x;
    });
  }

  private getProfileGraphs(): void {
    this.settingsService.getProfileGraphs().subscribe(x => {
      var sorted = x.items.sort(function(obj1, obj2) {
        if (obj1.period === obj2.period) {
          if (obj1.page === obj2.page) {
            return 0;
          }
          else {
            return obj1.page < obj2.page ? -1 : 1;
          }
        }
        else {
          return obj1.period < obj2.period ? -1 : 1;
        }
      });

      const result = this.groupBy(sorted, (pg) => pg.period + pg.page);
      this.profileGraphGroups = Object.values(result);
    });
  }

  private groupBy(xs, f) {
    return xs.reduce((r, v, i, a, k = f(v)) => ((r[k] || (r[k] = [])).push(v), r), {});
  }

  public hasError(controlName: string, errorName: string) {
    return this.formGroup.controls[controlName].hasError(errorName);
  }

  public submitForm(formGroupValue: any) {
    if (!this.formGroup.valid) {
      return;
    }

    this.dismissSnackBar();

    var profileGraph: ProfileGraph = formGroupValue;
    profileGraph.series = formGroupValue.series.map(x => { return {label:x.label, obisCode:x.obisCode} });

    this.log.debug("Adding profile graph", profileGraph);

    this.settingsService.addProfileGraph(profileGraph).subscribe(_ => {
      this.log.debug("Add ok");
      this.translateService.get('forms.settings.profileGraphs.confirmAdd').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
        this.menuService.signalProfileGraphPagesChanged();
        this.form.resetForm();
        this.formGroup.reset({ page: '' }); // Provide default value for the optional field.
        this.getProfileGraphs();
      });
    }, err => {
      this.log.debug("Add failed", err);
      var translateIds = ['forms.settings.profileGraphs.errorAdd'];
      var addProfileGraphError = err as AddProfileGraphError;
      if (addProfileGraphError === AddProfileGraphError.RequestContentIncomplete 
        || addProfileGraphError === AddProfileGraphError.RequestContentDuplicate)
      {
        translateIds.push('forms.settings.profileGraphs.errorAdjustFields');
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
  
  deleteProfileGraph(event: any) {
    var profileGraph: ProfileGraph = event;

    if (profileGraph == null || profileGraph == undefined) {
      this.log.info("Skipping delete profile graph. Profile graph unspecified", event);
      return;
    }

    this.dismissSnackBar();

    this.log.debug("Deleting profile graph");

    this.settingsService.deleteProfileGraph(profileGraph.period, profileGraph.page, profileGraph.title).subscribe(_ => {
      this.log.debug("Delete ok");
      this.translateService.get('forms.settings.profileGraphs.confirmActionDelete').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
        this.menuService.signalProfileGraphPagesChanged();
        this.getProfileGraphs();
      });
    }, err => {
      this.log.debug("Delete failed", err);
      this.translateService.get('forms.settings.profileGraphs.errorActionDelete').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 9000 });
      });
    });
  }

  swapProfileGraphs(event: any) {
    var profileGraphs: ProfileGraph[] = event;

    if (profileGraphs == null || profileGraphs == undefined) {
      this.log.info("Skipping swap profile graph ranks. Profile graphs unspecified", event);
      return;
    }

    if (profileGraphs.length != 2 || profileGraphs[0].period != profileGraphs[1].period || 
      profileGraphs[0].page != profileGraphs[1].page || profileGraphs[0].title == profileGraphs[1].title) {
      this.log.info("Skipping swap profile graph ranks. Incompatible profile graph ranks", event);
      return;
    }

    this.dismissSnackBar();

    this.log.debug("Swapping profile graph ranks");

    this.settingsService.swapProfileGraphRank(profileGraphs[0].period, profileGraphs[0].page, profileGraphs[0].title, profileGraphs[1].title).subscribe(_ => {
      this.log.debug("Swap ok");
      this.translateService.get('forms.settings.profileGraphs.confirmActionSwap').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 4000 });
        this.getProfileGraphs();
      });
    }, err => {
      this.log.debug("Swap failed", err);
      this.translateService.get('forms.settings.profileGraphs.errorActionSwap').subscribe(message => {
        this.snackBarRef = this.snackBar.open(message, undefined, { duration: 9000 });
      });
    });
  }

  private dismissSnackBar(): void {
    if (this.snackBarRef != null) {
      this.snackBarRef.dismiss();
    }
  }

  private getProfileGraphIntervals(): any[] {
    return [
      {
        "period": "day",
        "value": "2-minutes"
      },
      {
        "period": "day",
        "value": "5-minutes",
        "default": true
      },
      {
        "period": "day",
        "value": "10-minutes"
      },
      {
        "period": "day",
        "value": "15-minutes"
      },
      {
        "period": "day",
        "value": "30-minutes"
      },
      {
        "period": "day",
        "value": "60-minutes"
      },
      {
        "period": "month",
        "value": "1-days",
        "default": true
      },
      {
        "period": "year",
        "value": "1-months",
        "default": true
      }
    ];
  }

}

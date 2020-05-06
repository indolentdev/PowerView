import { Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, AbstractControl, FormGroup, Validators, FormGroupDirective, NgForm } from '@angular/forms';
import { Router, ActivatedRoute } from "@angular/router";
import { TranslateService } from '@ngx-translate/core';
import { NGXLogger } from 'ngx-logger';
import { MatDatepickerInputEvent } from '@angular/material/datepicker';
import { ExportService } from '../../../services/export.service';
import { ExportSeriesSet } from '../../../model/exportSeriesSet';
import { ExportToCsv } from 'export-to-csv';

import { Moment } from 'moment'
import * as moment from 'moment';

const labelsParam = "labels";
const fromParam = "from";
const toParam = "to";

@Component({
  selector: 'app-export-hourly',
  templateUrl: './export-hourly.component.html',
  styleUrls: ['./export-hourly.component.css']
})
export class ExportHourlyComponent implements OnInit {
  minDateFrom = moment("2010-01-01T00:00:00Z");
  maxDateFrom = moment().subtract(2, 'days');
  minDateTo = moment("2010-01-02T00:00:00Z");
  maxDateTo = moment().subtract(1, 'days');

  labels: string[];

  formGroup: FormGroup;
  @ViewChild('form', { static: true }) form;

  constructor(private log: NGXLogger, private router: Router, private route: ActivatedRoute, private exportService: ExportService, private translateService: TranslateService) {
  }

  ngOnInit() {
    this.formGroup = new FormGroup({
      labels: new FormControl('', [Validators.required]),
      fromDate: new FormControl('', [Validators.required]),
      toDate: new FormControl('', [Validators.required])
    });

    this.getLabels();

    this.route.queryParamMap.subscribe(queryParams => {
      let selectedLabels = null;
      let labelsSelectControl = this.getControl("labels");
      const labelsString = queryParams.get(labelsParam);
      if (labelsString != null)
      {
        selectedLabels = labelsString.split(",");
      }
      labelsSelectControl.setValue(selectedLabels);

      let fromDateCtl = this.getControl("fromDate");
      const fromDateString = queryParams.get(fromParam);
      this.parseDateSetFormControl(fromDateString, fromDateCtl);

      let toDateCtl = this.getControl("toDate");
      const toDateString = queryParams.get(toParam);
      this.parseDateSetFormControl(toDateString, toDateCtl);

      if (labelsSelectControl.value != null && labelsSelectControl.value.length > 0 &&
        fromDateCtl.value != null && moment(fromDateCtl.value).isValid() &&
        toDateCtl.value != null && moment(toDateCtl.value).isValid() ) {
          let labels = labelsSelectControl.value;
          let fromMoment = moment(fromDateCtl.value);
          let toMoment = moment(toDateCtl.value);
          this.Export(labels, fromMoment, toMoment);
      }
    });    
  }

  private getLabels(): void {
    if (this.labels == null)
    {
      this.exportService.getLabels().subscribe(x => { 
        this.labels = x;
      });
    }
  }

  public getControl(controlName: string): AbstractControl {
    return this.formGroup.controls[controlName];
  }

  private parseDateSetFormControl(dateString: string, fcDate: AbstractControl): void {
    let parsedDate = moment(dateString);
    if (parsedDate.isValid() && !parsedDate.isSame(fcDate.value))
    {
      fcDate.setValue(parsedDate);
    }
  }

  fromDateChangeEvent(event: MatDatepickerInputEvent<Moment>) {
    if (event == null) return;
    if (event.value == null) return;

    this.minDateTo = moment(event.value.toISOString()).add(1, 'days');
    this.maxDateTo = moment(event.value.toISOString()).add(2, 'days').add(3, 'months');

    this.navigate({ from: event.value.toISOString() });
  }

  toDateChangeEvent(event: MatDatepickerInputEvent<Moment>) {
    if (event == null) return;
    if (event.value == null) return;

    this.maxDateFrom = moment(event.value.toISOString()).subtract(1, 'days');
    this.minDateFrom = moment(event.value.toISOString()).subtract(2, 'days').subtract(3, 'months');

    this.navigate({ to: event.value.toISOString() });
  }

  labelsChangeEvent(event) {
    if (event == null) return;
    if (event.value == null) return;

    let labels = event.value.join();

    this.navigate({ labels: labels });
  }

  private navigate(queryParams: any): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: queryParams,
      queryParamsHandling: "merge",
      replaceUrl: true
    });
  }

  private Export(labels: string[], from: Moment, to: Moment): void {
    this.exportService.getHourlyExport(labels, from, to.add(1,'days')).subscribe(exportSeriesSet => {
      const labelCount = exportSeriesSet.series
        .map(x => x.label)
        .filter((lbl, i, arr) => arr.findIndex(l => l === lbl) === i) // distinct
        .length;

      const data = this.getRows(exportSeriesSet);

      let params = {
        timestamp: moment().format("YYYY-MM-DD--HH-mm-ss"),
        labelCount: labelCount
       };
      this.translateService.get('export.filename', params).subscribe(filename => {
        const options = {
          fieldSeparator: ';',
          filename: filename,
          quoteStrings: '',
          decimalSeparator: 'locale',
          showLabels: true, 
          showTitle: false,
          useTextFile: false,
          useBom: true,
          useKeysAsHeaders: true,
        };       
        const csvExporter = new ExportToCsv(options);
        csvExporter.generateCsv(data);    
      });
  
    });
  }

  private getRows(exportSeriesSet: ExportSeriesSet): any[] {
    let data = [];

    for (let ix=0; ix < exportSeriesSet.timestamps.length; ix++)
    {
      let row = {}; 

      let name = this.translateService.instant("export.columnNameHour");
      row[name] = moment(exportSeriesSet.timestamps[ix]).format("YYYY-MM-DD HH");

      exportSeriesSet.series.forEach(s => {
        let label = s.label;
        let valueName = this.translateService.instant("serie." + s.obisCode);
        let params = {label: label, valueName: valueName };

        let exportValue = s.values[ix];

        name = this.translateService.instant("export.columnNameLabelTimestamp", params);
        row[name] = (exportValue.timestamp === undefined || exportValue.timestamp == null) ? "" : moment(exportValue.timestamp).format("YYYY-MM-DD HH:mm:ss");

        name = this.translateService.instant("export.columnNameLabel", params);
        row[name] = (exportValue.value === undefined || exportValue.value == null) ? "" : exportValue.value.toString();

        name = this.translateService.instant("export.columnNameLabelDiff", params);
        row[name] = (exportValue.diffValue === undefined || exportValue.diffValue == null) ? "" : exportValue.diffValue.toString();

        name = this.translateService.instant("export.columnNameLabelUnit", params);
        row[name] = (exportValue.unit === undefined || exportValue.unit == null) ? "" : exportValue.unit;

        name = this.translateService.instant("export.columnNameLabelDeviceId", params);
        row[name] = (exportValue.deviceId === undefined || exportValue.deviceId == null) ? "" : exportValue.deviceId;
      });

      data.push(row);
    }

    return data;
  }
}

import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { NGXLogger } from 'ngx-logger';
import { ExportSpec } from '../../../model/exportSpec';
import { ExportSeriesGaugeSet } from '../../../model/exportSeriesGaugeSet';
import { ExportService } from '../../../services/export.service';
import { mkConfig, generateCsv, download } from 'export-to-csv';

import { Moment } from 'moment'
import moment from 'moment';

@Component({
  selector: 'app-export-gauges-hourly',
  templateUrl: './export-gauges-hourly.component.html',
  styleUrls: ['./export-gauges-hourly.component.css']
})
export class ExportGaugesHourlyComponent implements OnInit {

  constructor(private log: NGXLogger, private exportService: ExportService, private translateService: TranslateService) { }

  ngOnInit(): void {
  }

  onExport(exportSpec: ExportSpec) {
    this.exportService.getGaugesExportHourly(exportSpec).subscribe(exportSeriesSet => {
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
        const options = mkConfig({
          fieldSeparator: ';',
          filename: filename,
          quoteStrings: false,
          decimalSeparator: exportSpec.decimalSeparator,
          //          showLabels: true,
          showTitle: false,
          useTextFile: false,
          useBom: true,
          useKeysAsHeaders: true,
        });
        const csv = generateCsv(options)(data);
        download(options)(csv);
      });

    });
  }

  private getRows(exportSeriesSet: ExportSeriesGaugeSet): any[] {
    let data = [];

    for (let ix = 0; ix < exportSeriesSet.timestamps.length; ix++) {
      let row = {};

      let name = "";
      exportSeriesSet.series.forEach(s => {
        let label = s.label;
        let valueName = this.translateService.instant("serie." + s.obisCode);
        let params = { label: label, valueName: valueName };

        let exportValue = s.values[ix];

        name = this.translateService.instant("export.columnNameLabelTimestamp", params);
        row[name] = (exportValue.timestamp === undefined || exportValue.timestamp == null) ? "" : moment(exportValue.timestamp).format("YYYY-MM-DD HH:mm:ss");

        name = this.translateService.instant("export.columnNameLabel", params);
        row[name] = (exportValue.value === undefined || exportValue.value == null) ? "" : exportValue.value;

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

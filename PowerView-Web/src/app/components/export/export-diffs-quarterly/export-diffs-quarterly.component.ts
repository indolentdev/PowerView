import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { NGXLogger } from 'ngx-logger';
import { ExportSpec } from '../../../model/exportSpec';
import { ExportSeriesDiffSet } from '../../../model/exportSeriesDiffSet';
import { ExportService } from '../../../services/export.service';
import { mkConfig, generateCsv, download } from 'export-to-csv';

import { Moment } from 'moment'
import moment from 'moment';

@Component({
  selector: 'app-export-diffs-quarterly',
  templateUrl: './export-diffs-quarterly.component.html',
  styleUrl: './export-diffs-quarterly.component.css',
  standalone: false
})
export class ExportDiffsQuarterlyComponent implements OnInit {

    constructor(private log: NGXLogger, private exportService: ExportService, private translateService: TranslateService) { }
  
    ngOnInit(): void {
    }
  
    onExport(exportSpec: ExportSpec) {
      this.exportService.getDiffsExportQuarterly(exportSpec).subscribe(exportSeriesSet => {
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
  
    private getRows(exportSeriesSet: ExportSeriesDiffSet): any[] {
      let data = [];
  
      for (let ix = 0; ix < exportSeriesSet.periods.length; ix++) {
        let row = {};
  
        let name = "";
        exportSeriesSet.series.forEach(s => {
          let label = s.label;
          let valueName = this.translateService.instant("serie." + s.obisCode);
          let params = { label: label, valueName: valueName };
  
          let exportValue = s.values[ix];
  
          name = this.translateService.instant("export.columnNameLabelFrom", params);
          row[name] = (exportValue.from === undefined || exportValue.from == null) ? "" : moment(exportValue.from).format("YYYY-MM-DD HH:mm:ss");
  
          name = this.translateService.instant("export.columnNameLabelTo", params);
          row[name] = (exportValue.to === undefined || exportValue.to == null) ? "" : moment(exportValue.to).format("YYYY-MM-DD HH:mm:ss");
  
          name = this.translateService.instant("export.columnNameLabel", params);
          row[name] = (exportValue.value === undefined || exportValue.value == null) ? "" : exportValue.value;
  
          name = this.translateService.instant("export.columnNameLabelUnit", params);
          row[name] = (exportValue.unit === undefined || exportValue.unit == null) ? "" : exportValue.unit;
        });
  
        data.push(row);
      }
  
      return data;
    }
  
}

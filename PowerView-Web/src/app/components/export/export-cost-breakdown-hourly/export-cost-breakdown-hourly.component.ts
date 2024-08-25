import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { NGXLogger } from 'ngx-logger';
import { ExportTitleSpec } from '../../../model/exportTitleSpec';
import { ExportCostBreakdown } from '../../../model/exportCostBreakdown';
import { ExportService } from '../../../services/export.service';
import { mkConfig, generateCsv, download } from 'export-to-csv';

import { Moment } from 'moment'
import * as moment from 'moment';

@Component({
  selector: 'app-export-cost-breakdown-hourly',
  templateUrl: './export-cost-breakdown-hourly.component.html',
  styleUrls: ['./export-cost-breakdown-hourly.component.css']
})
export class ExportCostBreakdownHourlyComponent {

  constructor(private log: NGXLogger, private exportService: ExportService, private translateService: TranslateService) { }

  ngOnInit(): void {
  }

  onExport(exportSpec: ExportTitleSpec) {
    console.log("Initiating export ", exportSpec); 
    
    this.exportService.getCostBreakdownExportHourly(exportSpec).subscribe(exportCb => {

      const data = this.getRows(exportCb);

      let params = {
        timestamp: moment().format("YYYY-MM-DD--HH-mm-ss"),
      };

      this.translateService.get('export.filenameCostBreakdown', params).subscribe(filename => {
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

  private getRows(exportCb: ExportCostBreakdown): any[] {
    let data = [];

    for (let ix = 0; ix < exportCb.periods.length; ix++) {
      let row = {};
      let propName = "";

      let period = exportCb.periods[ix];

      propName = this.translateService.instant("export.columnNameFrom");
      row[propName] = (period.from === undefined || period.from == null) ? "" : moment(period.from).format("YYYY-MM-DD HH:mm:ss");

      propName = this.translateService.instant("export.columnNameTo");
      row[propName] = (period.to === undefined || period.to == null) ? "" : moment(period.to).format("YYYY-MM-DD HH:mm:ss");

      propName = this.translateService.instant("export.columnNameTitle");
      row[propName] = (exportCb.title === undefined || exportCb.title == null) ? "" : exportCb.title;
      
      propName = this.translateService.instant("export.columnNameCurrency");
      row[propName] = (exportCb.currency === undefined || exportCb.currency == null) ? "" : exportCb.currency;

      propName = this.translateService.instant("export.columnNameVat");
      row[propName] = (exportCb.vat === undefined || exportCb.vat == null) ? "" : exportCb.vat;

      exportCb.entries.forEach(e => {
        let name = e.name;
        let exportValue = e.values[ix];

        propName = this.translateService.instant("export.columnNameNameExVat", { name: name });
        row[propName] = (exportValue.value === undefined || exportValue.value == null) ? "" : exportValue.value;
      });

      data.push(row);
    }

    return data;
  }

}

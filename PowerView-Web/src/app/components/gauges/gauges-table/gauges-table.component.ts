import { Component, OnInit,ViewChild, Input, OnChanges, SimpleChanges } from '@angular/core';
import { MatSort, MatSortable } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { ObisTranslateService } from '../../../services/obis-translate.service';
import { GaugeValueGroup } from '../../../model/gaugeValueGroup';

@Component({
    selector: 'app-gauges-table',
    templateUrl: './gauges-table.component.html',
    styleUrls: ['./gauges-table.component.css'],
    standalone: false
})
export class GaugesTableComponent implements OnInit, OnChanges {
  displayedColumns = ['serie', 'timestamp', 'gaugeValue', 'unit', 'deviceId'];
  dataSource: MatTableDataSource<any>;

  @ViewChild(MatSort, { static: true }) sort: MatSort;

  @Input('gaugeValueGroup') gaugeValueGroup: GaugeValueGroup;

  constructor(private log: NGXLogger, private obisService: ObisTranslateService) {
  }

  ngOnInit() {
    let empty = [];
    this.dataSource = new MatTableDataSource<any>(empty);

    this.sort.sort(<MatSortable>({id: 'serie', start: 'asc'}) );
    this.sort.disableClear = true;

    this.refresh();
  }

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['gaugeValueGroup']) {
      this.gaugeValueGroup = changes['gaugeValueGroup'].currentValue;
      this.refresh();
    }
  }

  private refresh(): void {
    if (this.dataSource != null && this.gaugeValueGroup != null) {
      this.dataSource.data = this.obisService.AddSerieProperty(this.gaugeValueGroup.registers);
    }
  }
}

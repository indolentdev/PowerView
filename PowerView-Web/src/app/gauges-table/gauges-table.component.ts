import { Component, OnInit,ViewChild, Input, OnChanges, SimpleChanges } from '@angular/core';
import { MatSort, MatSortable, MatTableDataSource } from '@angular/material';
import { NGXLogger } from 'ngx-logger';
import { SerieService } from '../services/serie.service';
import { GaugeValueGroup } from '../model/gaugeValueGroup';

@Component({
  selector: 'app-gauges-table',
  templateUrl: './gauges-table.component.html',
  styleUrls: ['./gauges-table.component.css']
})
export class GaugesTableComponent implements OnInit, OnChanges {
  displayedColumns = ['serie', 'timestamp', 'gaugeValue', 'unit', 'serialNumber'];
  dataSource: MatTableDataSource<any>;

  @ViewChild(MatSort) sort: MatSort;

  @Input('gaugeValueGroup') gaugeValueGroup: GaugeValueGroup;

  constructor(private log: NGXLogger, private serieService: SerieService) {
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
      this.dataSource.data = this.serieService.AddSerieProperty(this.gaugeValueGroup.registers);
    }
  }
}

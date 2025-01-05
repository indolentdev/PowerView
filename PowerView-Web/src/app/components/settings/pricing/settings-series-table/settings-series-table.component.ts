import { Component, OnInit, ViewChild, Input, Output, OnChanges, SimpleChanges, EventEmitter } from '@angular/core';
import { MatSort, MatSortable } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { GeneratorSeries } from '../../../../model/generatorSeries';

@Component({
    selector: 'app-settings-series-table',
    templateUrl: './settings-series-table.component.html',
    styleUrls: ['./settings-series-table.component.css'],
    standalone: false
})
export class SettingsSeriesTableComponent {
  displayedColumns = ['nameSeries', 'baseSeries', 'costBreakdownTitle', 'actions'];
  dataSource: MatTableDataSource<any>;

  @ViewChild(MatSort, { static: true }) sort: MatSort;

  @Input('generatorSeries') generatorSeries: GeneratorSeries[];

  @Output('deleteGeneratorSeries') deleteAction: EventEmitter<GeneratorSeries> = new EventEmitter();

  constructor(private log: NGXLogger) {
  }

  ngOnInit() {
    let empty = [];
    this.dataSource = new MatTableDataSource<any>(empty);

    this.sort.sort(<MatSortable>({ id: 'nameSeries', start: 'asc' }));
    this.sort.disableClear = true;

    this.refresh();
  }

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['generatorSeries']) {
      this.generatorSeries = changes['generatorSeries'].currentValue;
      this.refresh();
    }
  }

  private refresh(): void {
    if (this.dataSource != null && this.generatorSeries != null) {
      this.dataSource.data = this.generatorSeries;
    }
  }

  deleteClick(item: any) {
    if (item == null) {
      return;
    }
    this.log.debug("Delete clicked", item);
    this.deleteAction.emit(item);
  }

}

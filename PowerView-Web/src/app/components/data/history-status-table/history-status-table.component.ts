import { Component, OnInit, ViewChild, Input, OnChanges, SimpleChanges } from '@angular/core';
import { MatSort, MatSortable } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { HistoryStatus } from 'src/app/model/historyStatus';

@Component({
  selector: 'app-history-status-table',
  templateUrl: './history-status-table.component.html',
  styleUrls: ['./history-status-table.component.css']
})
export class HistoryStatusTableComponent {
  displayedColumns = ['label', 'latestTimestamp'];
  dataSource: MatTableDataSource<any>;

  @ViewChild(MatSort, { static: true }) sort: MatSort;

  @Input('historyStatus') historyStatus: HistoryStatus;

  constructor(private log: NGXLogger) {
    let empty = [];
    this.dataSource = new MatTableDataSource<any>(empty);
  }

  ngOnInit() {
    this.sort.sort(<MatSortable>({ id: 'label', start: 'asc' }));
    this.sort.disableClear = true;

//    this.refresh(null);
  }

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
  }

  ngOnChanges(changes: SimpleChanges) {
    this.refresh(changes.historyStatus.currentValue);
  }

  private refresh(values): void {
    if (values == undefined || values == null) {
      this.dataSource.data = [];
      return;
    }
    this.dataSource.data = values.labelTimestamps;
  }

}

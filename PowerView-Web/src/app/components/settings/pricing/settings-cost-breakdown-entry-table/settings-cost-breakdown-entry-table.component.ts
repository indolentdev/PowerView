import { Component, OnInit, ViewChild, Input, Output, OnChanges, SimpleChanges, EventEmitter } from '@angular/core';
import { MatSort, MatSortable } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { CostBreakdown } from '../../../../model/costBreakdown';
import { CostBreakdownEntry } from '../../../../model/costBreakdownEntry';

@Component({
    selector: 'app-settings-cost-breakdown-entry-table',
    templateUrl: './settings-cost-breakdown-entry-table.component.html',
    styleUrls: ['./settings-cost-breakdown-entry-table.component.css'],
    standalone: false
})
export class SettingsCostBreakdownEntryTableComponent {
  displayedColumns = ['fromDate', 'toDate', 'name', 'startTime', 'endTime', 'amount', 'actions'];
  dataSource: MatTableDataSource<any>;

  @ViewChild(MatSort, { static: true }) sort: MatSort;

  @Input('costBreakdownEntries') costBreakdownEntries: CostBreakdownEntry[];

  @Output('editCostBreakdownEntry') editAction: EventEmitter<CostBreakdownEntry> = new EventEmitter();
  @Output('deleteCostBreakdownEntry') deleteAction: EventEmitter<CostBreakdownEntry> = new EventEmitter();

  constructor(private log: NGXLogger) {
  }

  ngOnInit() {
    let empty = [];
    this.dataSource = new MatTableDataSource<any>(empty);

    this.sort.sort(<MatSortable>({ id: 'name', start: 'asc' }));
    this.sort.disableClear = true;

    this.refresh();
  }

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['costBreakdownEntries']) {
      this.costBreakdownEntries = changes['costBreakdownEntries'].currentValue;
      this.refresh();
    }
  }

  private refresh(): void {
    if (this.dataSource != null && this.costBreakdownEntries != null) {

      this.dataSource.data = this.costBreakdownEntries;
    }
  }

  deleteClick(item: any) {
    if (item == null) {
      return;
    }
    this.log.debug("Delete clicked", item);
    this.deleteAction.emit(item);
  }

  editClick(item: any) {
    if (item == null) {
      return;
    }
    this.log.debug("Edit clicked", item);
    this.editAction.emit(item);
  }

}

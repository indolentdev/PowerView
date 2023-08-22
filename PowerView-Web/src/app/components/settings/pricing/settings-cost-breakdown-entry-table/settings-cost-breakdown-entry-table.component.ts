import { Component, OnInit, ViewChild, Input, Output, OnChanges, SimpleChanges, EventEmitter } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { CostBreakdown } from '../../../../model/costBreakdown';
import { CostBreakdownEntry } from '../../../../model/costBreakdownEntry';

@Component({
  selector: 'app-settings-cost-breakdown-entry-table',
  templateUrl: './settings-cost-breakdown-entry-table.component.html',
  styleUrls: ['./settings-cost-breakdown-entry-table.component.css']
})
export class SettingsCostBreakdownEntryTableComponent {
  displayedColumns = ['fromDate', 'toDate', 'name', 'startTime', 'endTime', 'amount', 'currency', 'actions'];
  dataSource: MatTableDataSource<any>;

  @Input('costBreakdown') costBreakdown: CostBreakdown;

  @Output('deleteCostBreakdown') deleteAction: EventEmitter<CostBreakdownEntry> = new EventEmitter();

  constructor(private log: NGXLogger) {
  }

  ngOnInit() {
    let empty = [];
    this.dataSource = new MatTableDataSource<any>(empty);

    this.refresh();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['costBreakdown']) {
      this.costBreakdown = changes['costBreakdown'].currentValue;
      this.refresh();
    }
  }

  private refresh(): void {
    if (this.dataSource != null && this.costBreakdown != null) {

      this.dataSource.data = this.costBreakdown.entries;
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

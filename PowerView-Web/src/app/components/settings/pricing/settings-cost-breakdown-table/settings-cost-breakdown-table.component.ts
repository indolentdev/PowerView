import { Component, OnInit, ViewChild, Input, Output, OnChanges, SimpleChanges, EventEmitter } from '@angular/core';
import { MatSort, MatSortable } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { CostBreakdown } from '../../../../model/costBreakdown';

@Component({
  selector: 'app-settings-cost-breakdown-table',
  templateUrl: './settings-cost-breakdown-table.component.html',
  styleUrls: ['./settings-cost-breakdown-table.component.css']
})
export class SettingsCostBreakdownTableComponent {
  displayedColumns = ['title', 'currency', 'vat', 'entries', 'actions'];
  dataSource: MatTableDataSource<any>;

  @ViewChild(MatSort, { static: true }) sort: MatSort;

  @Input('costBreakdowns') costBreakdowns: CostBreakdown[];

  @Output('selectCostBreakdown') selectAction: EventEmitter<CostBreakdown> = new EventEmitter();
  @Output('deleteCostBreakdown') deleteAction: EventEmitter<CostBreakdown> = new EventEmitter();

  constructor(private log: NGXLogger) {
  }

  ngOnInit() {
    let empty = [];
    this.dataSource = new MatTableDataSource<any>(empty);

    this.sort.sort(<MatSortable>({ id: 'title', start: 'asc' }));
    this.sort.disableClear = true;

    this.refresh();
  }

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['costBreakdowns']) {
      this.costBreakdowns = changes['costBreakdowns'].currentValue;
      this.refresh();
    }
  }

  private refresh(): void {
    if (this.dataSource != null && this.costBreakdowns != null) {

      this.dataSource.data = this.costBreakdowns;
    }
  }

  selectClick(item: any) {
    if (item == null) {
      return;
    }
    this.log.debug("Select clicked", item);
    this.selectAction.emit(item);
  }

  deleteClick(item: any) {
    if (item == null) {
      return;
    }
    this.log.debug("Delete clicked", item);
    this.deleteAction.emit(item);
  }

}

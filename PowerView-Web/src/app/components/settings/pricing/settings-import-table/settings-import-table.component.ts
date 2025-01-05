import { Component, OnInit, ViewChild, Input, Output, OnChanges, SimpleChanges, EventEmitter } from '@angular/core';
import { MatSort, MatSortable } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { Import } from '../../../../model/import';

@Component({
    selector: 'app-settings-import-table',
    templateUrl: './settings-import-table.component.html',
    styleUrls: ['./settings-import-table.component.css'],
    standalone: false
})
export class SettingsImportTableComponent {

  displayedColumns = ['label', 'channel', 'currency', 'fromTimestamp', 'currentTimestamp', 'enabled', 'actions'];
  dataSource: MatTableDataSource<any>;

  @ViewChild(MatSort, { static: true }) sort: MatSort;

  @Input('imports') imports: Import[];

  @Output('toggleImport') toggleAction: EventEmitter<Import> = new EventEmitter();
  @Output('deleteImport') deleteAction: EventEmitter<Import> = new EventEmitter();

  constructor(private log: NGXLogger) {
  }

  ngOnInit() {
    let empty = [];
    this.dataSource = new MatTableDataSource<any>(empty);

    this.sort.sort(<MatSortable>({ id: 'label', start: 'asc' }));
    this.sort.disableClear = true;

    this.refresh();
  }

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['imports']) {
      this.imports = changes['imports'].currentValue;
      this.refresh();
    }
  }

  private refresh(): void {
    if (this.dataSource != null && this.imports != null) {

      this.dataSource.data = this.imports;
    }
  }

  enableClick(item: any) {
    if (item == null) {
      return;
    }
    this.log.debug("Enable clicked", item);
    let imp: Import = item;
    imp.enabled = true;
    this.toggleAction.emit(imp);
  }

  disableClick(item: any) {
    if (item == null) {
      return;
    }
    this.log.debug("Disable clicked", item);
    let imp: Import = item;
    imp.enabled = false;
    this.toggleAction.emit(imp);
  }

  deleteClick(item: any) {
    if (item == null) {
      return;
    }

    this.log.debug("Delete clicked", item);
    this.deleteAction.emit(item);
  }

}

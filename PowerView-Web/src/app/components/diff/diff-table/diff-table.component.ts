import { Component, OnInit,ViewChild, Input, OnChanges, SimpleChanges } from '@angular/core';
import { MatSort, MatSortable } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { ObisTranslateService } from '../../../services/obis-translate.service';
import { DiffValueSet } from '../../../model/diffValueSet';

@Component({
    selector: 'app-diff-table',
    templateUrl: './diff-table.component.html',
    styleUrls: ['./diff-table.component.css'],
    standalone: false
})
export class DiffTableComponent implements OnInit, OnChanges {
  displayedColumns = ['serie', 'from', 'to', 'value', 'unit'];
  dataSource: MatTableDataSource<any>;

  @ViewChild(MatSort, { static: true }) sort: MatSort;

  @Input('diffValueSet') diffValueSet: DiffValueSet;

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
    if (changes['diffValueSet']) {
      this.diffValueSet = changes['diffValueSet'].currentValue;
      this.refresh();
    }
  }

  private refresh(): void {
    if (this.dataSource != null && this.diffValueSet != null) {
      this.dataSource.data = this.obisService.AddSerieProperty(this.diffValueSet.registers);
    }
  }
}

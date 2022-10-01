import { Component, OnInit, ViewChild, Input, OnChanges, SimpleChanges } from '@angular/core';
import { MatSort, MatSortable } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { ObisService } from '../../../services/obis.service';
import { CrudeValue } from '../../../model/crudeValue';

@Component({
  selector: 'app-data-crude-table',
  templateUrl: './data-crude-table.component.html',
  styleUrls: ['./data-crude-table.component.css']
})
export class DataCrudeTableComponent implements OnInit, OnChanges {

  displayedColumns = ['register', 'value', 'scale', 'unit'];
  dataSource: MatTableDataSource<any>;

//  @ViewChild(MatSort, { static: true }) sort: MatSort;

  @Input('crudeValues') crudeValues: CrudeValue[];

  constructor(private log: NGXLogger, private obisService: ObisService) {
  }

  ngOnInit() {
    let empty = [];
    this.dataSource = new MatTableDataSource<any>(empty);

//    this.sort.sort(<MatSortable>({ id: 'serie', start: 'asc' }));
//    this.sort.disableClear = true;

    this.refresh();
  }

  ngAfterViewInit() {
//    this.dataSource.sort = this.sort;
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['crudeValues']) {
      this.crudeValues = changes['crudeValues'].currentValue;
      this.refresh();
    }
  }

  private refresh(): void {
    if (this.dataSource != null && this.crudeValues != null) {
      this.dataSource.data = this.obisService.AddRegisterProperty(this.crudeValues);
    }
  }

}

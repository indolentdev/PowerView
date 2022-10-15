import { Component, OnInit, ViewChild, Input, OnChanges, SimpleChanges } from '@angular/core';
import { MatSort, MatSortable } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { ObisService } from '../../../services/obis.service';
import { CrudeValue } from '../../../model/crudeValue';

import { Moment } from 'moment'
import * as moment from 'moment';

@Component({
  selector: 'app-data-crude-table',
  templateUrl: './data-crude-table.component.html',
  styleUrls: ['./data-crude-table.component.css']
})
export class DataCrudeTableComponent implements OnInit, OnChanges {

  displayedColumns = ['timestamp', 'register', 'value', 'scale', 'unit', 'deviceId'];
  dataSource: MatTableDataSource<any>;

//  @ViewChild(MatSort, { static: true }) sort: MatSort;

  @Input('label') label: string;
  @Input('from') from: Moment;

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
    if (changes['label']) {
      console.log(changes['label'].currentValue);
      console.log(changes);

//      this.crudeValues = changes['crudeValues'].currentValue;
      this.refresh();
    }

    if (changes['from']) {
      console.log(changes['from'].currentValue);

      //      this.crudeValues = changes['crudeValues'].currentValue;
//      this.refresh();
    }

  }

  private refresh(): void {
//    if (this.dataSource != null && this.crudeValues != null) {
//      this.dataSource.data = this.obisService.AddRegisterProperty(this.crudeValues);
//    }
  }

}

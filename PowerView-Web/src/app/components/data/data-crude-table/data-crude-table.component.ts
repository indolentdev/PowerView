import { Component, OnInit, ViewChild, Input, OnChanges, SimpleChanges } from '@angular/core';
import { MatSort, MatSortable } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { CrudeDataService } from '../../../services/crude-data.service';
import { ObisService } from '../../../services/obis.service';
import { CrudeValueSet } from '../../../model/crudeValueSet';

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

  crudeValueSet: CrudeValueSet;

  constructor(private log: NGXLogger, private obisService: ObisService, private crudeDataService: CrudeDataService) {
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
    this.refresh();
  }

  private refresh(): void {
    if (this.label == null || this.from == null) return;

    this.crudeDataService.getCrudeValues(this.label, this.from).subscribe(x => {
      this.crudeValueSet = x;
      this.dataSource.data = this.obisService.AddRegisterProperty(this.crudeValueSet.values);
    });
  }

}

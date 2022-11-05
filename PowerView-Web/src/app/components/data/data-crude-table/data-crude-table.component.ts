import { Component, OnInit, Input, OnChanges, SimpleChanges } from '@angular/core';
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

  displayedColumns = ['timestamp', 'register', 'value', 'scale', 'unit', 'deviceId'];
  dataSource: MatTableDataSource<any>;

  @Input('crudeValues') crudeValues: CrudeValue[];

  constructor(private log: NGXLogger, private obisService: ObisService) {
    let empty = [];
    this.dataSource = new MatTableDataSource<any>(empty);
  }

  ngOnInit() {
  }

  ngOnChanges(changes: SimpleChanges) {
    this.refresh(changes.crudeValues.currentValue);
  }

  private refresh(values): void {
    if (values == undefined || values == null) {
      this.dataSource.data = [];
      return;
    }
    this.dataSource.data = this.obisService.AddRegisterProperty(values);
  }

}

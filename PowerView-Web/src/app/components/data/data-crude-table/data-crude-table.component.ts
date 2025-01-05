import { Component, OnInit, Input, Output, OnChanges, SimpleChanges, EventEmitter } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { ObisTranslateService } from '../../../services/obis-translate.service';
import { CrudeValue } from '../../../model/crudeValue';

@Component({
    selector: 'app-data-crude-table',
    templateUrl: './data-crude-table.component.html',
    styleUrls: ['./data-crude-table.component.css'],
    standalone: false
})
export class DataCrudeTableComponent implements OnInit, OnChanges {

  displayedColumns = ['timestamp', 'register', 'value', 'scale', 'unit', 'deviceId', 'tags'];
  dataSource: MatTableDataSource<any>;

  @Input('crudeValues') crudeValues: CrudeValue[];
  @Input('allowDelete') allowDelete: boolean;

  @Output('deleteCrudeValue') deleteAction: EventEmitter<CrudeValue> = new EventEmitter();

  constructor(private log: NGXLogger, private obisService: ObisTranslateService) {
    let empty = [];
    this.dataSource = new MatTableDataSource<any>(empty);
  }

  ngOnInit() {
    if (this.allowDelete) {
      this.displayedColumns.push('actions');
    }      
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

  deleteClick(item: any) {
    if (item == null) {
      return;
    }
    this.log.debug("Delete clicked", item);
    this.deleteAction.emit(item);
  }

}

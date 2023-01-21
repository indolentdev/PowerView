import { Component, OnInit, ViewChild, Input, OnChanges, SimpleChanges } from '@angular/core';
import { MatSort, MatSortable } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { ObisTranslateService } from '../../../services/obis-translate.service';
import { ProfileTotalValue } from '../../../model/profileTotalValue';

@Component({
  selector: 'app-profile-total-table',
  templateUrl: './profile-total-table.component.html',
  styleUrls: ['./profile-total-table.component.css']
})
export class ProfileTotalTableComponent implements OnInit, OnChanges {
  displayedColumns = ['serie', 'value', 'unit'];
  dataSource: MatTableDataSource<any>;

  @ViewChild(MatSort, { static: true }) sort: MatSort;

  @Input('profileTotalValues') profileTotalValues: ProfileTotalValue[];

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
    if (changes['profileTotalValues']) {
      this.profileTotalValues = changes['profileTotalValues'].currentValue;
      this.refresh();
    }
  }

  private refresh(): void {
    if (this.dataSource != null && this.profileTotalValues != null) {
      this.dataSource.data = this.obisService.AddSerieProperty(this.profileTotalValues);
    }
  }
}

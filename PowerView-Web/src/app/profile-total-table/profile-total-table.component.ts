import { Component, OnInit, ViewChild, Input, OnChanges, SimpleChanges } from '@angular/core';
import { MatSort, MatSortable, MatTableDataSource } from '@angular/material';
import { NGXLogger } from 'ngx-logger';
import { SerieService } from '../services/serie.service';
import { ProfileTotalValue } from '../model/profileTotalValue';

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

  constructor(private log: NGXLogger, private serieService: SerieService) {
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
      this.dataSource.data = this.serieService.AddSerieProperty(this.profileTotalValues);
    }
  }
}

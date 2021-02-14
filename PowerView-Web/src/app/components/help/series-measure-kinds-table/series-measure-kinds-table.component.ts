import { Component, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'app-series-measure-kinds-table',
  templateUrl: './series-measure-kinds-table.component.html',
  styleUrls: ['./series-measure-kinds-table.component.css']
})
export class SeriesMeasureKindsTableComponent implements OnInit {
  displayedColumns = ['name', 'associatedTo', 'description', 'hint'];
  dataSource: MatTableDataSource<any>;

  measureKinds = ['current', 'cumulative', 'entryAverage', 'entryDifference', 'periodDifference', 'netEntryDifference', 'netPeriodDifference'];

  constructor() { }

  ngOnInit(): void {
    let empty = [];
    this.dataSource = new MatTableDataSource<any>(empty);

    this.refresh();
  }

  private refresh(): void {
    if (this.dataSource != null) {
      this.dataSource.data = this.measureKinds;
    }
  }
  
}
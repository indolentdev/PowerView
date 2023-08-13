import { Component, OnInit } from '@angular/core';
import { MatLegacyTableDataSource as MatTableDataSource } from '@angular/material/legacy-table';

@Component({
  selector: 'app-help-series-descriptions-table',
  templateUrl: './help-series-descriptions-table.component.html',
  styleUrls: ['./help-series-descriptions-table.component.css']
})
export class HelpSeriesDescriptionsTableComponent implements OnInit {
  displayedColumns = ['name', 'measureKind', 'description', 'typicalUnits', 'hint'];
  dataSource: MatTableDataSource<any>;

  constructor() { }

  ngOnInit(): void {
    let empty = [];
    this.dataSource = new MatTableDataSource<any>(empty);

    this.refresh();
  }

  private refresh(): void {
    let obisCodesList = this.getObisCodes();
    if (this.dataSource != null) {
      this.dataSource.data = obisCodesList;
    }
  }

  private getObisCodes() {
    let obisCodes = [
      "1.0.1.7.0.255",  // Current imported/consumed el. power
      "1.0.21.7.0.255", // Current imported/consumed el. power L1
      "1.0.41.7.0.255", // Current imported/consumed el. power L2
      "1.0.61.7.0.255", // Current imported/consumed el. power L3
      "1.0.2.7.0.255",  // Current exported/produced el. power
      "1.0.22.7.0.255", // Current exported/produced el. power L1
      "1.0.42.7.0.255", // Current exported/produced el. power L2
      "1.0.62.7.0.255", // Current exported/produced el. power L3

      "1.0.1.8.0.255",  // Cumulative imported/consumed el. energy
      "1.0.2.8.0.255",  // Cumulative exported/produced el. energy

      "1.67.1.7.0.255", // Average imported/consumed el. power
      "1.67.2.7.0.255", // Average exported/produced el. power

      "1.65.1.8.0.255", // Entry difference imported/consumed el. energy
      "1.65.2.8.0.255", // Entry difference imported/consumed el. energy
      "1.66.1.8.0.255", // Period difference imported/consumed el. energy
      "1.66.2.8.0.255", // Period difference imported/consumed el. energy

      "1.65.16.8.0.255",// Net entry difference imported/consumed el. energy
      "1.65.26.8.0.255",// Net entry difference exported/produced el. energy

      "6.0.8.0.0.255",  // Current imported/consumed hot water power
      "6.0.9.0.0.255",  // Current imported/consumed water flow
      "6.0.10.0.0.255", // Current imported/consumed water temperature inlet
      "6.0.11.0.0.255", // Current imported/consumed water temperature outlet

      "6.67.8.0.0.255", // Average imported/consumed hot water power
      "6.67.9.0.0.255", // Average imported/consumed water flow

      "6.0.1.0.0.255",  // Cumulative imported/consumed hot water energy
      "6.0.2.0.0.255",  // Cumulative imported/consumed water volume
      "6.65.1.0.0.255", // Entry difference imported/consumed hot water energy 
      "6.65.2.0.0.255", // Entry difference imported/consumed water volume
      "6.66.1.0.0.255", // Period difference imported/consumed hot water energy
      "6.66.2.0.0.255", // Period difference imported/consumed water volume

      "15.0.223.0.0.255", // Ambient temperature
      "15.0.223.0.2.255",  // Relative humidity

      "0.1.96.3.10.255", // Relay
    ];
    return obisCodes;
  }

}

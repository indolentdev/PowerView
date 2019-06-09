import { Component, OnInit,ViewChild, Input, OnChanges, SimpleChanges } from '@angular/core';
import { MatSort, MatSortable, MatTableDataSource } from '@angular/material';
import { NGXLogger } from 'ngx-logger';
import { SerieService } from '../services/serie.service';
import { SerieColorSet } from '../model/serieColorSet';

@Component({
  selector: 'app-settings-serie-colors-table',
  templateUrl: './settings-serie-colors-table.component.html',
  styleUrls: ['./settings-serie-colors-table.component.css']
})
export class SettingsSerieColorsTableComponent implements OnInit, OnChanges {
  displayedColumns = ['serie', 'color'];
  dataSource: MatTableDataSource<any>;

  @ViewChild(MatSort) sort: MatSort;

  @Input('serieColorSet') serieColorSet: SerieColorSet;

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
    if (changes['serieColorSet']) {
      this.serieColorSet = changes['serieColorSet'].currentValue;
      this.refresh();
    }
  }

  private refresh(): void {
    if (this.dataSource != null && this.serieColorSet != null) {
      this.dataSource.data = this.serieService.AddSerieProperty(this.serieColorSet.items);
    }
  }

}

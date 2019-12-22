import { Component, OnInit,ViewChild, Input, Output, OnChanges, SimpleChanges, EventEmitter } from '@angular/core';
import { MatSort, MatSortable, MatTableDataSource } from '@angular/material';
import { NGXLogger } from 'ngx-logger';
import { SerieService } from '../../../services/serie.service';
import { ProfileGraph } from '../../../model/profileGraph';

@Component({
  selector: 'app-settings-profile-graphs-table',
  templateUrl: './settings-profile-graphs-table.component.html',
  styleUrls: ['./settings-profile-graphs-table.component.css']
})
export class SettingsProfileGraphsTableComponent implements OnInit, OnChanges {
  displayedColumns = ['title', 'interval', 'series', 'actions'];
  dataSource: MatTableDataSource<any>;

  @ViewChild(MatSort, { static: true }) sort: MatSort;

  @Input('profileGraphs') profileGraphs: ProfileGraph[];

  @Output('deleteProfileGraph') deleteAction: EventEmitter<ProfileGraph> = new EventEmitter();
  @Output('swapProfileGraphs') swapAction: EventEmitter<ProfileGraph[]> = new EventEmitter();

  constructor(private log: NGXLogger, private serieService: SerieService) { 
  }

  ngOnInit() {
    let empty = [];
    this.dataSource = new MatTableDataSource<any>(empty);

    this.sort.sort(<MatSortable>({id: 'rank', start: 'asc'}) );
    this.sort.disableClear = true;

    this.refresh();
  }

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['profileGraphs']) {
      this.profileGraphs = changes['profileGraphs'].currentValue;
      this.refresh();
    }
  }

  private refresh(): void {
    if (this.dataSource != null && this.profileGraphs != null) {
      var ranks = this.getProfileGraphRanks();

      var distinctRanks = ranks.filter((value, index, self) => self.indexOf(value) === index);
      var allowChangeRanks = true;
      if (distinctRanks.length !== ranks.length) {
        this.log.error("Profile graph ranks must be unique", this.profileGraphs);
        allowChangeRanks = false;
      }
      var minRank = Math.min(...ranks);
      var maxRank = Math.max(...ranks);

      let rows = [];
      for (let pg of this.profileGraphs) {
        let row: any = pg;

        var intervalVars = pg.interval.split("-");
        row.intervalValue = intervalVars[0];
        row.intervalUnit = intervalVars[1];

        this.serieService.AddSerieProperty(pg.series);

        if (allowChangeRanks) {
          row.isFirst = false;
          if (pg.rank === minRank) {
            row.isFirst = true;
          }
          row.isLast = false;
          if (pg.rank === maxRank) {
            row.isLast = true;
          }
        }

        rows.push(row);
      }
      this.dataSource.data = rows;
    }
  }

  getSerie(ob: any[]): any[] {
    return ob.map(x => x.serie);
  }

  deleteClick(item: any) {
    if (item == null) {
      return;
    }
    this.log.debug("Delete clicked", item);
    this.deleteAction.emit(item);
  }

  downClick(item: any) {
    if (item == null) {
      return;
    }
    this.log.debug("Down clicked", item);
    this.swapRanks(item, true);
  }

  upClick(item: any) {
    if (item == null) {
      return;
    }
    this.log.debug("Up clicked", item);
    this.swapRanks(item, false);
  }

  private swapRanks(item: ProfileGraph, withNext: boolean) {
    var ranks = this.getProfileGraphRanks();
    var swapIxDelta = withNext ? 1 : -1;
    var swapRank = ranks[ranks.indexOf(item.rank) + swapIxDelta];
    var swapProfileGraph = this.profileGraphs.filter(x => x.rank === swapRank);
    var swapProfileGraphs = [item, ...swapProfileGraph];
    this.log.debug("Swapping profile graph ranks", swapProfileGraphs);
    this.swapAction.emit(swapProfileGraphs);
  }

  private getProfileGraphRanks(): number[] {
    return this.profileGraphs.map(x => x.rank);

  }

}

import { Component, OnInit,ViewChild, Input, Output, OnChanges, SimpleChanges, EventEmitter } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { ObisTranslateService } from '../../../services/obis-translate.service';
import { ProfileGraph } from '../../../model/profileGraph';

@Component({
    selector: 'app-settings-profile-graphs-table',
    templateUrl: './settings-profile-graphs-table.component.html',
    styleUrls: ['./settings-profile-graphs-table.component.css'],
    standalone: false
})
export class SettingsProfileGraphsTableComponent implements OnInit, OnChanges {
  displayedColumns = ['title', 'interval', 'series', 'actions'];
  dataSource: MatTableDataSource<any>;

  @Input('profileGraphs') profileGraphs: ProfileGraph[];

  @Output('editProfileGraph') editAction: EventEmitter<ProfileGraph> = new EventEmitter();
  @Output('deleteProfileGraph') deleteAction: EventEmitter<ProfileGraph> = new EventEmitter();
  @Output('swapProfileGraphs') swapAction: EventEmitter<ProfileGraph[]> = new EventEmitter();

  constructor(private log: NGXLogger, private obisService: ObisTranslateService) { 
  }

  ngOnInit() {
    let empty = [];
    this.dataSource = new MatTableDataSource<any>(empty);

    this.refresh();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['profileGraphs']) {
      this.profileGraphs = changes['profileGraphs'].currentValue;
      this.refresh();
    }
  }

  private refresh(): void {
    if (this.dataSource != null && this.profileGraphs != null) {

      let rows = [];
      this.profileGraphs.forEach((pg, index) => { 
        let row: any = pg;

        var intervalVars = pg.interval.split("-");
        row.intervalValue = intervalVars[0];
        row.intervalUnit = intervalVars[1];
        this.obisService.AddSerieProperty(pg.series);
        row.isFirst = false;
        if (index == 0) {
          row.isFirst = true;
        }
        row.isLast = false;
        if (index == this.profileGraphs.length - 1) {
          row.isLast = true;
        }

        rows.push(row);
      });

      this.dataSource.data = rows;
    }
  }

  getSerie(ob: any[]): any[] {
    return ob.map(x => x.serie);
  }

  editClick(item: any) {
    if (item == null) {
      return;
    }
    this.log.debug("Edit clicked", item);
    this.editAction.emit(item);
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
    let swapIxDelta = withNext ? 1 : -1;
    let profileGraphIndex = this.profileGraphs.findIndex(x => item.period == x.period && item.page == x.page && item.title == x.title && item.interval == x.interval);
    let swapProfileGraph = this.profileGraphs[profileGraphIndex + swapIxDelta];
    let swapProfileGraphs = [item, swapProfileGraph];
    this.log.debug("Swapping profile graphs", swapProfileGraphs);
    this.swapAction.emit(swapProfileGraphs);
  }

}

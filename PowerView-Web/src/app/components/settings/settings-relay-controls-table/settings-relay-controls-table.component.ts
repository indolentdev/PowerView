import { Component, OnInit,ViewChild, Input, Output, OnChanges, SimpleChanges, EventEmitter } from '@angular/core';
import { MatSort, MatSortable } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { ObisTranslateService } from '../../../services/obis-translate.service';
import { DisconnectRuleSet } from '../../../model/disconnectRuleSet';
import { DisconnectRule } from '../../../model/disconnectRule';

@Component({
    selector: 'app-settings-relay-controls-table',
    templateUrl: './settings-relay-controls-table.component.html',
    styleUrls: ['./settings-relay-controls-table.component.css'],
    standalone: false
})
export class SettingsRelayControlsTableComponent implements OnInit, OnChanges {
  displayedColumns = ['nameSerie', 'evaluationSerie', 'durationMinutes', 'disconnectToConnectValue', 'connectToDisconnectValue', 'actions'];
  dataSource: MatTableDataSource<any>;

  @ViewChild(MatSort, { static: true }) sort: MatSort;

  @Input('disconnectRuleSet') disconnectRuleSet: DisconnectRuleSet;

  @Output('deleteDisconnectRule') deleteAction: EventEmitter<DisconnectRule> = new EventEmitter();

  constructor(private log: NGXLogger, private obisService: ObisTranslateService) {
  }

  ngOnInit() {
    let empty = [];
    this.dataSource = new MatTableDataSource<any>(empty);

    this.sort.sort(<MatSortable>({id: 'nameSerie', start: 'asc'}) );
    this.sort.disableClear = true;

    this.refresh();
  }

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['disconnectRuleSet']) {
      this.disconnectRuleSet = changes['disconnectRuleSet'].currentValue;
      this.refresh();
    }
  }

  private refresh(): void {
    if (this.dataSource != null && this.disconnectRuleSet != null) {
      let rows = [];
      rows = this.obisService.AddSerieProperty(this.disconnectRuleSet.items, "nameLabel", "nameObisCode", "nameSerie");
      rows = this.obisService.AddSerieProperty(rows, "evaluationLabel", "evaluationObisCode", "evaluationSerie");
      this.dataSource.data = rows;
    }
  }

  deleteClick(item: any) {
    if (item == null) {
      return;
    }
    this.log.debug("Delete clicked", item);
    this.deleteAction.emit(item);
  }
}

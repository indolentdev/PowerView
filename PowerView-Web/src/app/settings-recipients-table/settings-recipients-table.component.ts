import { Component, OnInit,ViewChild, Input, Output, OnChanges, SimpleChanges, EventEmitter } from '@angular/core';
import { MatSort, MatSortable, MatTableDataSource } from '@angular/material';
import { NGXLogger } from 'ngx-logger';
import { SerieService } from '../services/serie.service';
import { EmailRecipientSet } from '../model/emailRecipientSet';
import { EmailRecipient } from '../model/emailRecipient';

@Component({
  selector: 'app-settings-recipients-table',
  templateUrl: './settings-recipients-table.component.html',
  styleUrls: ['./settings-recipients-table.component.css']
})
export class SettingsRecipientsTableComponent implements OnInit, OnChanges {
  displayedColumns = ['name', 'emailAddress', 'actions'];
  dataSource: MatTableDataSource<any>;

  @ViewChild(MatSort, { static: true }) sort: MatSort;

  @Input('emailRecipientSet') emailRecipientSet: EmailRecipientSet;

  @Output('deleteEmailRecipient') deleteAction: EventEmitter<EmailRecipient> = new EventEmitter();
  @Output('testEmailRecipient') testAction: EventEmitter<EmailRecipient> = new EventEmitter();

  constructor(private log: NGXLogger, private serieService: SerieService) { 
  }

  ngOnInit() {
    let empty = [];
    this.dataSource = new MatTableDataSource<any>(empty);

    this.sort.sort(<MatSortable>({id: 'name', start: 'asc'}) );
    this.sort.disableClear = true;

    this.refresh();
  }

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['emailRecipientSet']) {
      this.emailRecipientSet = changes['emailRecipientSet'].currentValue;
      this.refresh();
    }
  }

  private refresh(): void {
    if (this.dataSource != null && this.emailRecipientSet != null) {
      this.dataSource.data = this.emailRecipientSet.items;
    }
  }

  deleteClick(item: any) {
    if (item == null) {
      return;
    }
    this.log.debug("Delete clicked", item);
    this.deleteAction.emit(item);
  }

  testClick(item: any) {
    if (item == null) {
      return;
    }
    this.log.debug("Test clicked", item);
    this.testAction.emit(item);
  }

}

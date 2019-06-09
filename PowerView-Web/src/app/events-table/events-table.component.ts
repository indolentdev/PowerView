import { Component, OnInit,ViewChild, Inject, Input, OnChanges, SimpleChanges, LOCALE_ID } from '@angular/core';
import { formatDate } from '@angular/common';
import { MatSort, MatSortable, MatTableDataSource } from '@angular/material';
import { TranslateService } from '@ngx-translate/core';
import { NGXLogger } from 'ngx-logger';
import { EventSet } from '../model/eventSet';
import { EventAmplificationLeak } from '../model/eventAmplificationLeak';

@Component({
  selector: 'app-events-table',
  templateUrl: './events-table.component.html',
  styleUrls: ['./events-table.component.css']
})
export class EventsTableComponent implements OnInit, OnChanges {
  displayedColumns = ['detectTimestamp', 'label', 'event', 'state', 'details' ];
  dataSource: MatTableDataSource<any>;

  @ViewChild(MatSort) sort: MatSort;

  @Input('eventSet') eventSet: EventSet;

  constructor(private log: NGXLogger, @Inject(LOCALE_ID) private locale: string, private translateService: TranslateService) {
  }

  ngOnInit() {
    let empty = [];
    this.dataSource = new MatTableDataSource<any>(empty);

    this.sort.sort(<MatSortable>({id: 'detectTimestamp', start: 'desc'}) );
    this.sort.disableClear = true;

    this.refresh();
  }

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['eventSet']) {
      this.eventSet = changes['eventSet'].currentValue;
      this.refresh();
    }
  }

  private refresh(): void {
    if (this.dataSource != null && this.eventSet != null) {
      let rows = [];
      for (let item of this.eventSet.items) {
        let row:any = item;

        row.details = "-";
        var leakAmplification = item.amplification as EventAmplificationLeak;
        if (leakAmplification != undefined && leakAmplification != null)
        {
          var leakParams = {
           newline: "\n",
           startTimestamp: formatDate(leakAmplification.startTimestamp, 'short', this.locale),
           endTimestamp: formatDate(leakAmplification.endTimestamp, 'short', this.locale),
           value: leakAmplification.value,
           unit: leakAmplification.unit
          };
          this.translateService.get('events.details_' + item.type.toLowerCase(), leakParams).subscribe(x => {
            row.details = x;
          });
        }
        rows.push(row);
      }

      this.dataSource.data = rows;
    }
  }

}

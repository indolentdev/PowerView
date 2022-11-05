import { Component, OnInit, Input, OnChanges, SimpleChanges } from '@angular/core';
import { NGXLogger } from 'ngx-logger';
import { CrudeDataService } from '../../../services/crude-data.service';
import { CrudeValueSet } from '../../../model/crudeValueSet';

import { Moment } from 'moment'
import * as moment from 'moment';

@Component({
  selector: 'app-data-crude-bydate-table',
  templateUrl: './data-crude-bydate-table.component.html',
  styleUrls: ['./data-crude-bydate-table.component.css']
})
export class DataCrudeBydateTableComponent implements OnInit, OnChanges {

  @Input('label') label: string;
  @Input('from') from: Moment;

  crudeValueSet: CrudeValueSet;

  constructor(private log: NGXLogger, private crudeDataService: CrudeDataService) {
  }

  ngOnInit() {
    this.refresh();
  }

  ngOnChanges(changes: SimpleChanges) {
    this.refresh();
  }

  private refresh(): void {
    if (this.label == null || this.from == null) return;

    this.crudeDataService.getCrudeValues(this.label, this.from).subscribe(x => {
      this.crudeValueSet = x;
    });
  }

}

import { Component, OnInit, Input, OnChanges, SimpleChanges } from '@angular/core';
import { UntypedFormControl, UntypedFormGroup, Validators, FormGroupDirective, NgForm } from '@angular/forms';
import { ErrorStateMatcher } from '@angular/material/core';
import { NGXLogger } from 'ngx-logger';
import { TranslateService } from '@ngx-translate/core';
import { HistoryDataService } from '../../../services/history-data.service';
import { CrudeValueSet } from '../../../model/crudeValueSet';
import { HistoryStatusSet } from 'src/app/model/historyStatusSet';
import { HistoryStatus } from 'src/app/model/historyStatus';

import { Moment } from 'moment'
import * as moment from 'moment';

@Component({
  selector: 'app-history-status',
  templateUrl: './history-status.component.html',
  styleUrls: ['./history-status.component.css']
})
export class HistoryStatusComponent {

  historyStatusSet: HistoryStatusSet;

  constructor(private log: NGXLogger, private historyDataService: HistoryDataService) {
  }

  ngOnInit() {
    this.refresh();
  }

  ngOnChanges(changes: SimpleChanges) {
    this.refresh();
  }

  private refresh(): void {
    this.historyDataService.getHistoryStatus().subscribe(x => {
      this.historyStatusSet = x;
    });
  }

}

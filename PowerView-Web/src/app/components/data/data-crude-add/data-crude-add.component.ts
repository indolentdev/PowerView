import { Component, OnInit } from '@angular/core';
import { UntypedFormControl } from '@angular/forms';
import { Router, ActivatedRoute } from "@angular/router";
import { NGXLogger } from 'ngx-logger';
import { MatDatepickerInputEvent } from '@angular/material/datepicker';
import { LabelsService } from 'src/app/services/labels.service';
import { CrudeDataService } from 'src/app/services/crude-data.service';

import { Moment } from 'moment'
import * as moment from 'moment';

const labelParam = "label";

@Component({
  selector: 'app-data-crude-add',
  templateUrl: './data-crude-add.component.html',
  styleUrls: ['./data-crude-add.component.css']
})
export class DataCrudeAddComponent implements OnInit {
  fcLabel = new UntypedFormControl(null);

  labels: string[];

  constructor(private log: NGXLogger, private router: Router, private route: ActivatedRoute, private labelsService: LabelsService, private crudeDataService: CrudeDataService) { }

  ngOnInit(): void {
    this.getLabels();

    this.route.queryParamMap.subscribe(queryParams => {
      const labelString = queryParams.get(labelParam);
      this.fcLabel.setValue(labelString);
    });
  }

  private getLabels(): void {
    if (this.labels == null) {
      this.labelsService.getLabels().subscribe(x => {
        this.labels = x.sort((a: string, b: string) => (a > b) ? 1 : -1);;
      });
    }
  }

  labelChangeEvent(event) {
    if (event == null) return;
    if (event.value == null) return;

    this.navigate({ label: event.value });
  }

  private navigate(queryParams: any): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: queryParams,
      queryParamsHandling: "merge",
      replaceUrl: true
    });
  }

}

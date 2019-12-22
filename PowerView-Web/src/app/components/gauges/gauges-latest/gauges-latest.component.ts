import { Component, OnInit } from '@angular/core';
import { NGXLogger } from 'ngx-logger';
import { GaugesService } from '../../../services/gauges.service';
import { GaugeValueGroupSet } from '../../../model/gaugeValueGroupSet';

@Component({
  selector: 'app-guages-latest',
  templateUrl: './gauges-latest.component.html',
  styleUrls: ['./gauges-latest.component.css'],
})
export class GaugesLatestComponent implements OnInit {
  gaugeValueGroupSet: GaugeValueGroupSet;

  constructor(private log: NGXLogger, private gaugesService: GaugesService) {
  }

  ngOnInit() {
    this.getGuageValues();
  }

  getGuageValues(): void {
    this.gaugesService.getLatestGaugeValues().subscribe(x => { 
      this.gaugeValueGroupSet = x; 
    });
  }

}

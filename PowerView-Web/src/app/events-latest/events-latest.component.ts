import { Component, OnInit } from '@angular/core';
import { NGXLogger } from 'ngx-logger';
import { EventsService } from '../services/events.service';
import { EventSet } from '../model/eventSet';

@Component({
  selector: 'app-events-latest',
  templateUrl: './events-latest.component.html',
  styleUrls: ['./events-latest.component.css']
})
export class EventsLatestComponent implements OnInit {

  eventSet: EventSet;

  constructor(private log: NGXLogger, private eventService: EventsService) {
  }

  ngOnInit() {
    this.getDiffValues();
  }

  private getDiffValues(): void {
    this.eventService.getLatestEvents().subscribe(x => { 
      this.eventSet = x; 
    });
  }

}

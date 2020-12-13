import { TestBed } from '@angular/core/testing';
import { mock, instance, when, verify, anyString } from 'ts-mockito';
import { DataService } from './data.service';

import { EventsService } from './events.service';

describe('EventsService', () => {
  let dataService = mock(DataService);

  beforeEach(() => TestBed.configureTestingModule({
    providers: [
      {provide: DataService, useValue: instance(dataService)}
    ]
  }));

  it('should be created', () => {
    const service: EventsService = TestBed.inject(EventsService);
    expect(service).toBeTruthy();
  });
});

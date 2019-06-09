import { TestBed } from '@angular/core/testing';
import { DataService } from './data.service';
import { mock, instance, when, verify } from 'ts-mockito';
import { GaugesService } from './gauges.service';

describe('GuagesService', () => {
  let dataService = mock(DataService);

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [{provide: DataService, useValue: instance(dataService)}]
    });
  });

  it('should be created', () => {
    const service: GaugesService = TestBed.get(GaugesService);
    expect(service).toBeTruthy();
  });
});

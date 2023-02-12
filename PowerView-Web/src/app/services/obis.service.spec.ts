import { TestBed } from '@angular/core/testing';
import { DataService } from './data.service';
import { mock, instance, when, verify } from 'ts-mockito';
import { ObisService } from './obis.service';

describe('ObisService', () => {
  let dataService = mock(DataService);
  let service: ObisService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [{ provide: DataService, useValue: instance(dataService) }]
    });
    service = TestBed.inject(ObisService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

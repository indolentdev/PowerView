import { TestBed } from '@angular/core/testing';
import { DataService } from './data.service';
import { mock, instance, when, verify } from 'ts-mockito';
import { CrudeDataService } from './crude-data.service';

describe('CrudeDataService', () => {
  let dataService = mock(DataService);
  let service: CrudeDataService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [{ provide: DataService, useValue: instance(dataService) }]
    });
     service = TestBed.inject(CrudeDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

import { TestBed } from '@angular/core/testing';
import { DataService } from './data.service';
import { mock, instance, when, verify } from 'ts-mockito';
import { HistoryDataService } from './history-data.service';

describe('HistoryDataService', () => {
  let dataService = mock(DataService);
  let service: HistoryDataService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [{ provide: DataService, useValue: instance(dataService) }]
    });
    service = TestBed.inject(HistoryDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

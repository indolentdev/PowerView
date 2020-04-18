import { TestBed } from '@angular/core/testing';
import { DataService } from './data.service';
import { mock, instance, when, verify } from 'ts-mockito';

import { ExportService } from './export.service';

describe('ExportService', () => {
  let dataService = mock(DataService);

  beforeEach(() => { 
    TestBed.configureTestingModule({
      providers: [{provide: DataService, useValue: instance(dataService)}]
    });
  });

  it('should be created', () => {
    const service: ExportService = TestBed.get(ExportService);
    expect(service).toBeTruthy();
  });
});

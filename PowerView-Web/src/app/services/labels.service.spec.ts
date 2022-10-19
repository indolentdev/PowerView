import { TestBed } from '@angular/core/testing';
import { DataService } from './data.service';
import { mock, instance, when, verify } from 'ts-mockito';

import { LabelsService } from './labels.service';

describe('LabelService', () => {
  let dataService = mock(DataService);

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [{ provide: DataService, useValue: instance(dataService) }]
    });
  });

  it('should be created', () => {
    const service: LabelsService = TestBed.inject(LabelsService);
    expect(service).toBeTruthy();
  });
});

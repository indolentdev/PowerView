import { TestBed } from '@angular/core/testing';
import { DataService } from './data.service';
import { mock, instance, when, verify } from 'ts-mockito';
import { DiffService } from './diff.service';

describe('DiffService', () => {
  let dataService = mock(DataService);

  beforeEach(() => { 
    TestBed.configureTestingModule({
      providers: [{provide: DataService, useValue: instance(dataService)}]
    });
  });

  it('should be created', () => {
    const service: DiffService = TestBed.get(DiffService);
    expect(service).toBeTruthy();
  });
});

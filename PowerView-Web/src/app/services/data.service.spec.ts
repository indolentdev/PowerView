import { TestBed } from '@angular/core/testing';
import { NGXLogger } from 'ngx-logger';
import { HttpClient } from '@angular/common/http';
import { mock, instance, when, verify } from 'ts-mockito';

import { DataService } from './data.service';

describe('DataService', () => {
  let log = mock(NGXLogger);
  let httpClient = mock(HttpClient);

  beforeEach(() => { 
    TestBed.configureTestingModule({
      providers: [
        {provide: NGXLogger, useValue: instance(log)},
        {provide: HttpClient, useValue: instance(httpClient)}
      ]
    })
  });

  it('should be created', () => {
    const service: DataService = TestBed.get(DataService);
    expect(service).toBeTruthy();
  });
});

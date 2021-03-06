import { TestBed } from '@angular/core/testing';
import { DataService } from './data.service';
import { mock, instance, when, verify } from 'ts-mockito';

import { SettingsService } from './settings.service';

describe('SettingsService', () => {
  let dataService = mock(DataService);

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [{provide: DataService, useValue: instance(dataService)}]
    });
  });

  it('should be created', () => {
    const service: SettingsService = TestBed.inject(SettingsService);
    expect(service).toBeTruthy();
  });
});

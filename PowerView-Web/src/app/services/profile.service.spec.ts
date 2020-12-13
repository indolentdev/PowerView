import { TestBed } from '@angular/core/testing';
import { DataService } from './data.service';
import { mock, instance, when, verify } from 'ts-mockito';
import { ProfileService } from './profile.service';

describe('ProfileService', () => {
  let dataService = mock(DataService);

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [{provide: DataService, useValue: instance(dataService)}]
    });
  });

  it('should be created', () => {
    const service: ProfileService = TestBed.inject(ProfileService);
    expect(service).toBeTruthy();
  });
});

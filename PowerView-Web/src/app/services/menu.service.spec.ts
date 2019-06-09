import { TestBed } from '@angular/core/testing';
import { DataService } from './data.service';
import { mock, instance, when, verify } from 'ts-mockito';
import { MenuService } from './menu.service';

describe('MenuService', () => {
  let dataService = mock(DataService);

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [{provide: DataService, useValue: instance(dataService)}]
    });
  });

  it('should be created', () => {
    const service: MenuService = TestBed.get(MenuService);
    expect(service).toBeTruthy();
  });

  it('should signal profile graph pages changed event', () => {
    const service: MenuService = TestBed.get(MenuService);
    var signal = false;
    service.getProfileGraphPageChanges().subscribe(_ => {
      signal = true;
    });

    service.signalProfileGraphPagesChanged();

    expect(signal).toBeTruthy();
  });

});

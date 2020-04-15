import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ExportHourlyComponent } from './export-hourly.component';

describe('HourComponent', () => {
  let component: ExportHourlyComponent;
  let fixture: ComponentFixture<ExportHourlyComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ExportHourlyComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ExportHourlyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

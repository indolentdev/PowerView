import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SettingsCostBreakdownEntryComponent } from './settings-cost-breakdown-entry.component';

describe('SettingsCostBreakdownEntryComponent', () => {
  let component: SettingsCostBreakdownEntryComponent;
  let fixture: ComponentFixture<SettingsCostBreakdownEntryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SettingsCostBreakdownEntryComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SettingsCostBreakdownEntryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { TestBed } from '@angular/core/testing';

import { SerieService } from './serie.service';

import { NGXLogger } from 'ngx-logger';
import { TranslateService } from '@ngx-translate/core';
import { of } from 'rxjs';
import { mock, instance, when, verify, anyString } from 'ts-mockito';

describe('SerieService', () => {

  let log = mock(NGXLogger);
  let translateService:TranslateService = mock(TranslateService);

  beforeEach(() => TestBed.configureTestingModule({
    providers: [
      {provide: NGXLogger, useValue: instance(log)},
      {provide: TranslateService, useValue: instance(translateService)}
    ]
  }));

  it('should be created', () => {
    const service: SerieService = TestBed.inject(SerieService);
    expect(service).toBeTruthy();
  });

  it('AddSerieProperty returns input items', () => {
    // Arrange
    const service: SerieService = TestBed.inject(SerieService);
    var obj = { label: "Woop" };
    var array = [ obj ];

    // Act
    var output = service.AddSerieProperty(array);

    // Assert
    expect(output.length).toBe(array.length);
    expect(output[0].label).toBe("Woop");
  });

  it('AddSerieProperty should do nothing when only label exists', () => {
    // Arrange
    const service: SerieService = TestBed.inject(SerieService);
    var obj = { label: "Woop" };
    var array = [ obj ];

    // Act
    var output = service.AddSerieProperty(array);

    // Assert
    expect(output.length).toBe(array.length);
    expect(output[0].hasOwnProperty("serie")).toBeFalsy();
  });

  it('AddSerieProperty should do nothing when only obisCode exists', () => {
    // Arrange
    const service: SerieService = TestBed.inject(SerieService);
    var obj = { obisCode: "Woop" };
    var array = [ obj ];

    // Act
    var output = service.AddSerieProperty(array);

    // Assert
    expect(output.length).toBe(array.length);
    expect(output[0].hasOwnProperty("serie")).toBeFalsy();
  });

  it('AddSerieProperty should call TranslateService', () => {
    // Arrange
    when(translateService.get(anyString())).thenReturn(of("Habbada"));
    const service: SerieService = TestBed.inject(SerieService);
    var obj = { label: "Woop", obisCode: "theObis" };
    var array = [ obj ];

    // Act
    service.AddSerieProperty(array);

    // Assert
    verify(translateService.get("serie.theObis"));
    expect().nothing(); // suppress Spec has no expectations warning
  });

  it('AddSerieProperty should add serie property', () => {
    // Arrange
    when(translateService.get(anyString())).thenReturn(of("Habbada"));
    const service: SerieService = TestBed.inject(SerieService);
    var obj = { label: "Woop", obisCode: "theObis" };
    var array = [ obj ];

    // Act
    var output = service.AddSerieProperty(array);

    // Assert
    expect(output.length).toBe(array.length);
    expect(output[0].hasOwnProperty("serie")).toBeTruthy();
    expect(output[0].serie).toBe("Woop Habbada");
  });

  it('AddSerieProperty customizable property names', () => {
    // Arrange
    when(translateService.get(anyString())).thenReturn(of("Habbada"));
    const service: SerieService = TestBed.inject(SerieService);
    var obj = { lbl: "Woop", oc: "theObis" };
    var array = [ obj ];

    // Act
    var output = service.AddSerieProperty(array, "lbl", "oc", "se");

    // Assert
    expect(output.length).toBe(array.length);
    expect(output[0].hasOwnProperty("se")).toBeTruthy();
    expect(output[0].se).toBe("Woop Habbada");
  });

});

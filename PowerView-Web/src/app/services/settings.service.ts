import { Injectable } from '@angular/core';
import { DataService } from './data.service';
import { Observable, of, throwError, EMPTY } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';

import { CostBreakdownSet } from '../model/costBreakdownSet';
import { CostBreakdown } from '../model/costBreakdown';
import { CostBreakdownEntry } from '../model/costBreakdownEntry';
import { DisconnectRule } from '../model/disconnectRule';
import { DisconnectRuleSet } from '../model/disconnectRuleSet';
import { DisconnectRuleOptionSet } from '../model/disconnectRuleOptionSet';
import { EmailRecipient } from '../model/emailRecipient';
import { EmailRecipientSet } from '../model/emailRecipientSet';
import { Import } from '../model/import';
import { ImportCreate } from '../model/importCreate';
import { ImportSet } from '../model/importSet';
import { ApplicationProperties } from '../model/applicationProperties';
import { GeneratorSeries } from '../model/generatorSeries';
import { GeneratorSeriesSet } from '../model/generatorSeriesSet';
import { GeneratorBaseSeriesSet } from '../model/generatorBaseSeriesSet';
import { MqttParams } from '../model/mqttParams';
import { ProfileGraph } from '../model/profileGraph';
import { ProfileGraphSet } from '../model/profileGraphSet';
import { ProfileGraphSerieSet } from '../model/profileGraphSerieSet';
import { SerieColorSet } from '../model/serieColorSet';
import { SerieName } from '../model/serieName';
import { SmtpParams } from '../model/smtpParams';

import { Moment } from 'moment'
import * as moment from 'moment';
import { HttpParams } from '@angular/common/http';

const constLocal = {
  costBreakdowns: "settings/costbreakdowns",
  disconnectrules: "settings/disconnectrules",
  disconnectrulesNames: "settings/disconnectrules/names",
  disconnectrulesOptions: "settings/disconnectrules/options",
  emailRecipients: "settings/emailrecipients",
  imports: "settings/imports",
  generatorsSeries: "settings/generators/series",
  generatorsBaseSeries: "settings/generators/bases/series",
  application: "settings/application",
  mqtt: "settings/mqtt",
  mqttTest: "settings/mqtt/test",
  profileGraphs: "settings/profilegraphs",
  profileGraphsSwaprank: "settings/profilegraphs/swaprank",
  profileGraphsSeries: "settings/profilegraphs/series",
  serieColors: "settings/seriecolors",
  smtp: "settings/smtp"
};

@Injectable({
  providedIn: 'root'
})
export class SettingsService {

  constructor(private dataService: DataService) {
  }

  public getCostBreakdowns(): Observable<CostBreakdownSet> {
    return this.dataService.get<CostBreakdownSet>(constLocal.costBreakdowns, undefined, new CostBreakdownSet);
  }

  public addCostBreakdown(costBreakdown: CostBreakdown): Observable<any> {
    return this.dataService.post(constLocal.costBreakdowns, costBreakdown)
      .pipe(catchError(error => {
        return throwError(() => this.convertToAddCostBreakdownError(error));
      }));
  }

  private convertToAddCostBreakdownError(error: any): AddCostBreakdownError {
    if (!(error instanceof HttpErrorResponse)) {
      return AddCostBreakdownError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch (httpErrorResponse.status) {
      case 400:
        return AddCostBreakdownError.RequestContentIncomplete;
      case 409:
        return AddCostBreakdownError.RequestContentDuplicate;
      default:
        return AddCostBreakdownError.UnspecifiedError;
    }
  }

  public deleteCostBreakdown(title: string): Observable<any> {
    return this.dataService.delete(constLocal.costBreakdowns + "/" + encodeURIComponent(title))
      .pipe(catchError(error => {
        return throwError(() => this.convertToDeleteCostBreakdownError(error));
      }));
  }

  private convertToDeleteCostBreakdownError(error: any): DeleteCostBreakdownError {
    if (!(error instanceof HttpErrorResponse)) {
      return DeleteCostBreakdownError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch (httpErrorResponse.status) {
      default:
        return DeleteCostBreakdownError.UnspecifiedError;
    }
  }

  public addCostBreakdownEntry(costBreakdownTitle: string, costBreakdownEntry: CostBreakdownEntry): Observable<any> {
    return this.dataService.post(constLocal.costBreakdowns + "/" + encodeURIComponent(costBreakdownTitle) + "/entries", costBreakdownEntry)
      .pipe(catchError(error => {
        return throwError(() => this.convertToAddCostBreakdownEntryError(error));
      }));
  }

  private convertToAddCostBreakdownEntryError(error: any): AddCostBreakdownEntryError {
    if (!(error instanceof HttpErrorResponse)) {
      return AddCostBreakdownEntryError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch (httpErrorResponse.status) {
      case 400:
        return AddCostBreakdownEntryError.RequestContentIncomplete;
      case 409:
        return AddCostBreakdownEntryError.RequestContentDuplicate;
      default:
        return AddCostBreakdownEntryError.UnspecifiedError;
    }
  }

  public updateCostBreakdownEntry(costBreakdownTitle: string, fromDate: string, toDate: string, name: string, costBreakdownEntry: CostBreakdownEntry): Observable<any> {
    return this.dataService.put(constLocal.costBreakdowns + "/" + encodeURIComponent(costBreakdownTitle) + "/entries/" +
        encodeURIComponent(fromDate) + "/" + encodeURIComponent(toDate) + "/" + encodeURIComponent(name), costBreakdownEntry)
      .pipe(catchError(error => {
        return throwError(() => this.convertToUpdateCostBreakdownEntryError(error));
      }));
  }

  private convertToUpdateCostBreakdownEntryError(error: any): UpdateCostBreakdownEntryError {
    if (!(error instanceof HttpErrorResponse)) {
      return UpdateCostBreakdownEntryError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch (httpErrorResponse.status) {
      case 400:
        return UpdateCostBreakdownEntryError.RequestContentIncomplete;
      case 409:
        return UpdateCostBreakdownEntryError.RequestContentDuplicate;
      default:
        return UpdateCostBreakdownEntryError.UnspecifiedError;
    }
  }

  public deleteCostBreakdownEntry(title: string, fromDate: string, toDate: string, name: string): Observable<any> {
    return this.dataService.delete(constLocal.costBreakdowns + "/" + encodeURIComponent(title) + "/entries/" +
        encodeURIComponent(fromDate) + "/" + encodeURIComponent(toDate) + "/" + encodeURIComponent(name))
      .pipe(catchError(error => {
        return throwError(() => this.convertToDeleteCostBreakdownEntryError(error));
      }));
  }

  private convertToDeleteCostBreakdownEntryError(error: any): DeleteCostBreakdownEntryError {
    if (!(error instanceof HttpErrorResponse)) {
      return DeleteCostBreakdownEntryError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch (httpErrorResponse.status) {
      default:
        return DeleteCostBreakdownEntryError.UnspecifiedError;
    }
  }

  public getDisconnectRules(): Observable<DisconnectRuleSet> {
    return this.dataService.get<DisconnectRuleSet>(constLocal.disconnectrules, undefined, new DisconnectRuleSet);
  }

  public addDisconnectRule(disconnectRule: DisconnectRule): Observable<any> {
    return this.dataService.post(constLocal.disconnectrules, disconnectRule)
    .pipe(catchError(error => {
      return throwError(() => this.convertToAddDisconnectRuleError(error));
    }));
  }

  private convertToAddDisconnectRuleError(error: any): AddDisconnectRuleError {
    if ( !(error instanceof HttpErrorResponse) ) {
      return AddDisconnectRuleError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch(httpErrorResponse.status) {
      case 400:
        return AddDisconnectRuleError.RequestContentIncomplete;
      case 409:
        return AddDisconnectRuleError.RequestContentDuplicate;
      default:
        return AddDisconnectRuleError.UnspecifiedError;
    }
  }

  public deleteDisconnectRule(label: string, obisCode: string): Observable<any> {
    return this.dataService.delete(constLocal.disconnectrulesNames + "/" + encodeURIComponent(label) + "/" + encodeURIComponent(obisCode))
    .pipe(catchError(error => {
      return throwError(() => this.convertToDeleteDisconnectRuleError(error));
    }));
  }

  private convertToDeleteDisconnectRuleError(error: any): DeleteDisconnectRuleError {
    if ( !(error instanceof HttpErrorResponse) ) {
      return DeleteDisconnectRuleError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch(httpErrorResponse.status) {
      case 400:
        return DeleteDisconnectRuleError.DisconnectRulePathMissing;
      default:
        return DeleteDisconnectRuleError.UnspecifiedError;
    }
  }

  public getDisconnectRuleOptions(): Observable<DisconnectRuleOptionSet> {
    return this.dataService.get<DisconnectRuleOptionSet>(constLocal.disconnectrulesOptions, undefined, new DisconnectRuleOptionSet);
  }

  public getEmailRecipients(): Observable<EmailRecipientSet> {
    return this.dataService.get<EmailRecipientSet>(constLocal.emailRecipients, undefined, new EmailRecipientSet);
  }

  public addEmailRecipient(emailRecipeint: EmailRecipient): Observable<any> {
    return this.dataService.post(constLocal.emailRecipients, emailRecipeint)
    .pipe(catchError(error => {
      return throwError(() => this.convertToAddEmailRecipientError(error));
    }));
  }

  private convertToAddEmailRecipientError(error: any): AddEmailRecipientError {
    if ( !(error instanceof HttpErrorResponse) ) {
      return AddEmailRecipientError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch(httpErrorResponse.status) {
      case 400:
        return AddEmailRecipientError.RequestContentIncomplete;
      case 409:
        return AddEmailRecipientError.RequestContentDuplicate;
      default:
        return AddEmailRecipientError.UnspecifiedError;
    }
  }

  public deleteEmailRecipient(emailAddress: string): Observable<any> {
    return this.dataService.delete(constLocal.emailRecipients + "/" + encodeURIComponent(emailAddress))
    .pipe(catchError(error => {
      return throwError(() => this.convertToDeleteEmailRecipientError(error));
    }));
  }

  private convertToDeleteEmailRecipientError(error: any): DeleteEmailRecipientError {
    if ( !(error instanceof HttpErrorResponse) ) {
      return DeleteEmailRecipientError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch(httpErrorResponse.status) {
      default:
        return DeleteEmailRecipientError.UnspecifiedError;
    }
  }

  public testEmailRecipient(emailAddress: string): Observable<any> {
    return this.dataService.put(constLocal.emailRecipients + "/" + encodeURIComponent(emailAddress) + "/test")
    .pipe(catchError(error => {
      return throwError(() => this.convertToTestEmailRecipientError(error));
    }));
  }

  private convertToTestEmailRecipientError(error: any): TestEmailRecipientError {
    if ( !(error instanceof HttpErrorResponse) ) {
      return TestEmailRecipientError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch(httpErrorResponse.status) {
      case 404: 
        return TestEmailRecipientError.EmailNotFound;
      case 400:
        return TestEmailRecipientError.EmailTestFailed;
      case 504:
        return TestEmailRecipientError.EmailServerConnectionFailed;
      case 567:
        return TestEmailRecipientError.EmailServerAuthenticationFailed;
      default:
        return TestEmailRecipientError.UnspecifiedError;
    }
  }

  public getGeneratorsSeries(): Observable<GeneratorSeriesSet> {
    return this.dataService.get<GeneratorSeriesSet>(constLocal.generatorsSeries, undefined, new GeneratorSeriesSet);
  }

  public addGeneratorsSeries(generatorSeries: GeneratorSeries): Observable<any> {
    return this.dataService.post(constLocal.generatorsSeries, generatorSeries)
      .pipe(catchError(error => {
        return throwError(() => this.convertToAddGeneratorSeriesError(error));
      }));
  }

  private convertToAddGeneratorSeriesError(error: any): AddGeneratorSeriesError {
    if (!(error instanceof HttpErrorResponse)) {
      return AddGeneratorSeriesError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch (httpErrorResponse.status) {
      case 400:
        return AddGeneratorSeriesError.RequestContentIncomplete;
      case 409:
        return AddGeneratorSeriesError.RequestContentDuplicate;
      default:
        return AddGeneratorSeriesError.UnspecifiedError;
    }
  }

  public deleteGeneratorSeries(label: string, obisCode: string): Observable<any> {
    return this.dataService.delete(constLocal.generatorsSeries + "/" + encodeURIComponent(label) + "/" + encodeURIComponent(obisCode))
      .pipe(catchError(error => {
        return throwError(() => this.convertToDeleteGeneratorSeriesError(error));
      }));
  }

  private convertToDeleteGeneratorSeriesError(error: any): DeleteGeneratorSeriesError {
    if (!(error instanceof HttpErrorResponse)) {
      return DeleteGeneratorSeriesError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch (httpErrorResponse.status) {
      default:
        return DeleteGeneratorSeriesError.UnspecifiedError;
    }
  }

  public getGeneratorsBaseSeries(): Observable<GeneratorBaseSeriesSet> {
    return this.dataService.get<GeneratorBaseSeriesSet>(constLocal.generatorsBaseSeries, undefined, new GeneratorBaseSeriesSet);
  }

  public getImports(): Observable<ImportSet> {
    return this.dataService.get<ImportSet>(constLocal.imports, undefined, new ImportSet);
  }

  public addImport(importCreate: ImportCreate): Observable<any> {
    return this.dataService.post(constLocal.imports, importCreate)
      .pipe(catchError(error => {
        return throwError(() => this.convertToAddImportError(error));
      }));
  }

  private convertToAddImportError(error: any): AddImportError {
    if (!(error instanceof HttpErrorResponse)) {
      return AddImportError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch (httpErrorResponse.status) {
      case 400:
        return AddImportError.RequestContentIncomplete;
      case 409:
        return AddImportError.RequestContentDuplicate;
      default:
        return AddImportError.UnspecifiedError;
    }
  }

  public toggleImport(label: string, enabled: boolean): Observable<any> {
    return this.dataService.patch(constLocal.imports + "/" + encodeURIComponent(label), { enabled: enabled })
      .pipe(catchError(error => {
        return throwError(() => this.convertToToggleImportError(error));
      }));
  }

  private convertToToggleImportError(error: any): ToggleImportError {
    if (!(error instanceof HttpErrorResponse)) {
      return ToggleImportError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch (httpErrorResponse.status) {
      case 400:
        return ToggleImportError.RequestContentIncomplete;
      case 404:
        return ToggleImportError.LabelNotFound;
      default:
        return ToggleImportError.UnspecifiedError;
    }
  }

  public deleteImport(label: string): Observable<any> {
    return this.dataService.delete(constLocal.imports + "/" + encodeURIComponent(label))
      .pipe(catchError(error => {
        return throwError(() => this.convertToDeleteImportError(error));
      }));
  }

  private convertToDeleteImportError(error: any): DeleteImportError {
    if (!(error instanceof HttpErrorResponse)) {
      return DeleteImportError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch (httpErrorResponse.status) {
      default:
        return DeleteImportError.UnspecifiedError;
    }
  }

  public getProfileGraphs(): Observable<ProfileGraphSet> {
    return this.dataService.get<ProfileGraphSet>(constLocal.profileGraphs, undefined, new ProfileGraphSet);
  }

  public addProfileGraph(profileGraph: ProfileGraph): Observable<any> {
    return this.dataService.post(constLocal.profileGraphs, profileGraph)
    .pipe(catchError(error => {
      return throwError(() => this.convertToAddProfileGraphError(error));
    }));
  }

  private convertToAddProfileGraphError(error: any): AddProfileGraphError {
    if ( !(error instanceof HttpErrorResponse) ) {
      return AddProfileGraphError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch(httpErrorResponse.status) {
      case 400:
        return AddProfileGraphError.RequestContentIncomplete;
      case 409:
        return AddProfileGraphError.RequestContentDuplicate;
      default:
        return AddProfileGraphError.UnspecifiedError;
    }
  }

  public updateProfileGraph(period: string, page: string, title: string, profileGraph: ProfileGraph): Observable<any> {
    var params = new HttpParams().set("period", period).set("page", page).set("title", title);
    return this.dataService.put(constLocal.profileGraphs, profileGraph, params)
    .pipe(catchError(error => {
      return throwError(() => this.convertToUpdateProfileGraphError(error));
    }));
  }

  private convertToUpdateProfileGraphError(error: any): UpdateProfileGraphError {
    if ( !(error instanceof HttpErrorResponse) ) {
      return UpdateProfileGraphError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch(httpErrorResponse.status) {
      case 400:
        return UpdateProfileGraphError.RequestContentIncomplete;
      case 409:
        return UpdateProfileGraphError.ExistingProfileGraphAbsent;
      default:
        return UpdateProfileGraphError.UnspecifiedError;
    }
  }

  public swapProfileGraphRank(period: string, page: string, title1: string, title2: string): Observable<any> {
    var params = new HttpParams().set("period", period).set("page", page).set("title1", title1).set("title2", title2);
    return this.dataService.put(constLocal.profileGraphsSwaprank, null, params)
    .pipe(catchError(error => {
      return throwError(() => this.convertToSwapProfileGraphRankError(error));
    }));
  }

  private convertToSwapProfileGraphRankError(error: any): SwapProfileGraphRankError {
    if ( !(error instanceof HttpErrorResponse) ) {
      return SwapProfileGraphRankError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch(httpErrorResponse.status) {
      default:
        return SwapProfileGraphRankError.UnspecifiedError;
    }
  }

  public deleteProfileGraph(period: string, page: string, title: string): Observable<any> {
    var params = new HttpParams().set("period", period).set("page", page).set("title", title);
    return this.dataService.delete(constLocal.profileGraphs, params)
    .pipe(catchError(error => {
      return throwError(() => this.convertToDeleteProfileGraphError(error));
    }));
  }

  private convertToDeleteProfileGraphError(error: any): DeleteProfileGraphError {
    if ( !(error instanceof HttpErrorResponse) ) {
      return DeleteProfileGraphError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch(httpErrorResponse.status) {
      default:
        return DeleteProfileGraphError.UnspecifiedError;
    }
  }

  public getProfileGraphsSeries(): Observable<ProfileGraphSerieSet> {
    return this.dataService.get<ProfileGraphSerieSet>(constLocal.profileGraphsSeries, undefined, new ProfileGraphSerieSet);
  }

  public getSerieColors(): Observable<SerieColorSet> {
    return this.dataService.get<SerieColorSet>(constLocal.serieColors, undefined, new SerieColorSet);
  }

  public saveSerieColors(serieColorSet: SerieColorSet): Observable<any> {
    return this.dataService.put(constLocal.serieColors, serieColorSet);
  }

  public getApplicationProperties(): Observable<ApplicationProperties> {
    return this.dataService.get<ApplicationProperties>(constLocal.application, undefined);
  }

  public getMqttParams(): Observable<MqttParams> {
    return this.dataService.get<MqttParams>(constLocal.mqtt, undefined);
  }

  public saveMqttParams(mqttParams: MqttParams): Observable<any> {
    return this.dataService.put(constLocal.mqtt, mqttParams)
    .pipe(catchError(error => {
      return throwError(() => this.convertToSaveMqttParamsError(error));
    }));
  }

  private convertToSaveMqttParamsError(error: any): SaveMqttParamsError {
    if ( !(error instanceof HttpErrorResponse) ) {
      return SaveMqttParamsError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch(httpErrorResponse.status) {
      case 415:
        return SaveMqttParamsError.RequestContentIncomplete;
      default:
        return SaveMqttParamsError.UnspecifiedError;
    }
  }

  public testMqttParams(mqttParams: MqttParams): Observable<any> {
    return this.dataService.put(constLocal.mqttTest, mqttParams)
    .pipe(catchError(error => {
      return throwError(() => this.convertToTestMqttParamsError(error));
    }));
  }

  private convertToTestMqttParamsError(error: any): TestMqttParamsError {
    if ( !(error instanceof HttpErrorResponse) ) {
      return TestMqttParamsError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch(httpErrorResponse.status) {
      case 503:
        return TestMqttParamsError.NoConnection;
      case 415:
        return TestMqttParamsError.RequestContentIncomplete;
      default:
        return TestMqttParamsError.UnspecifiedError;
    }
  }

  public getSmtpParams(): Observable<SmtpParams> {
    return this.dataService.get<SmtpParams>(constLocal.smtp, undefined);
  }

  public saveSmtpParams(smtpParams: SmtpParams): Observable<any> {
    return this.dataService.put(constLocal.smtp, smtpParams)
    .pipe(catchError(error => {
      return throwError(() => this.convertToSaveSmtpParamsError(error));
    }));
  }

  private convertToSaveSmtpParamsError(error: any): SaveSmtpParamsError {
    if ( !(error instanceof HttpErrorResponse) ) {
      return SaveSmtpParamsError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch(httpErrorResponse.status) {
      case 400:
        return SaveSmtpParamsError.RequestContentIncomplete;
      default:
        return SaveSmtpParamsError.UnspecifiedError;
    }
  }

}

export enum AddCostBreakdownError {
  UnspecifiedError = "UnspecifiedError",
  RequestContentIncomplete = "RequestContentIncomplete",
  RequestContentDuplicate = "RequestContentDuplicate"
}

export enum DeleteCostBreakdownError {
  UnspecifiedError = "UnspecifiedError"
}

export enum AddCostBreakdownEntryError {
  UnspecifiedError = "UnspecifiedError",
  RequestContentIncomplete = "RequestContentIncomplete",
  RequestContentDuplicate = "RequestContentDuplicate"
}

export enum UpdateCostBreakdownEntryError {
  UnspecifiedError = "UnspecifiedError",
  RequestContentIncomplete = "RequestContentIncomplete",
  RequestContentDuplicate = "RequestContentDuplicate"
}

export enum DeleteCostBreakdownEntryError {
  UnspecifiedError = "UnspecifiedError"
}

export enum AddDisconnectRuleError {
  UnspecifiedError = "UnspecifiedError",
  RequestContentIncomplete = "RequestContentIncomplete",
  RequestContentDuplicate = "RequestContentDuplicate"
}

export enum DeleteDisconnectRuleError {
  UnspecifiedError = "UnspecifiedError",
  DisconnectRulePathMissing = "DisconnectRulePathMissing"
}

export enum AddEmailRecipientError {
  UnspecifiedError = "UnspecifiedError",
  RequestContentIncomplete = "RequestContentIncomplete",
  RequestContentDuplicate = "RequestContentDuplicate"
}

export enum DeleteEmailRecipientError {
  UnspecifiedError = "UnspecifiedError"
}

export enum TestEmailRecipientError {
  UnspecifiedError = "UnspecifiedError",
  EmailNotFound = "EmailNotFound",
  EmailTestFailed = "EmailTestFailed",
  EmailServerConnectionFailed = "EmailServerConnectionFailed",
  EmailServerAuthenticationFailed = "EmailServerAuthenticationFailed"
}

export enum AddGeneratorSeriesError {
  UnspecifiedError = "UnspecifiedError",
  RequestContentIncomplete = "RequestContentIncomplete",
  RequestContentDuplicate = "RequestContentDuplicate"
}

export enum DeleteGeneratorSeriesError {
  UnspecifiedError = "UnspecifiedError"
}

export enum AddImportError {
  UnspecifiedError = "UnspecifiedError",
  RequestContentIncomplete = "RequestContentIncomplete",
  RequestContentDuplicate = "RequestContentDuplicate"
}

export enum ToggleImportError {
  UnspecifiedError = "UnspecifiedError",
  RequestContentIncomplete = "RequestContentIncomplete",
  LabelNotFound = "LabelNotFound"
}

export enum DeleteImportError {
  UnspecifiedError = "UnspecifiedError"
}

export enum AddProfileGraphError {
  UnspecifiedError = "UnspecifiedError",
  RequestContentIncomplete = "RequestContentIncomplete",
  RequestContentDuplicate = "RequestContentDuplicate"
}

export enum UpdateProfileGraphError {
  UnspecifiedError = "UnspecifiedError",
  RequestContentIncomplete = "RequestContentIncomplete",
  ExistingProfileGraphAbsent = "RequestContentDuplicate"
}

export enum SwapProfileGraphRankError {
  UnspecifiedError = "UnspecifiedError",
}

export enum DeleteProfileGraphError {
  UnspecifiedError = "UnspecifiedError",
}

export enum SaveMqttParamsError {
  UnspecifiedError = "UnspecifiedError",
  RequestContentIncomplete = "RequestContentIncomplete"
}

export enum TestMqttParamsError {
  UnspecifiedError = "UnspecifiedError",
  RequestContentIncomplete = "RequestContentIncomplete",
  NoConnection = "NoConnection"
}

export enum SaveSmtpParamsError {
  UnspecifiedError = "UnspecifiedError",
  RequestContentIncomplete = "RequestContentIncomplete"
}

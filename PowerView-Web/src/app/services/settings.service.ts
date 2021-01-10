import { Injectable } from '@angular/core';
import { DataService } from './data.service';
import { Observable, of, throwError, EMPTY } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';

import { DisconnectRule } from '../model/disconnectRule';
import { DisconnectRuleSet } from '../model/disconnectRuleSet';
import { DisconnectRuleOptionSet } from '../model/disconnectRuleOptionSet';
import { EmailRecipient } from '../model/emailRecipient';
import { EmailRecipientSet } from '../model/emailRecipientSet';
import { ApplicationProperties } from '../model/applicationProperties';
import { MqttParams } from '../model/mqttParams';
import { ProfileGraph } from '../model/profileGraph';
import { ProfileGraphSet } from '../model/profileGraphSet';
import { ProfileGraphSerieSet } from '../model/profileGraphSerieSet';
import { SerieColorSet } from '../model/serieColorSet';
import { SmtpParams } from '../model/smtpParams';

import { Moment } from 'moment'
import * as moment from 'moment';
import { HttpParams } from '@angular/common/http';

const constLocal = {
  disconnectrules: "settings/disconnectrules",
  disconnectrulesNames: "settings/disconnectrules/names",
  disconnectrulesOptions: "settings/disconnectrules/options",
  emailRecipients: "settings/emailrecipients",
  application: "settings/application",
  mqtt: "settings/mqtt",
  mqttTest: "settings/mqtt/test",
  profileGraphs: "settings/profilegraphs",
  profileGraphsSwaprank: "settings/profilegraphs/swaprank",
  profileGraphsSeries: "settings/profilegraphs/series",
  profileGraphsModify: "settings/profilegraphs/modify",
  serieColors: "settings/seriecolors",
  smtp: "settings/smtp"
};

@Injectable({
  providedIn: 'root'
})
export class SettingsService {

  constructor(private dataService: DataService) { 
  }

  public getDisconnectRules(): Observable<DisconnectRuleSet> {
    return this.dataService.get<DisconnectRuleSet>(constLocal.disconnectrules, undefined, new DisconnectRuleSet);
  }

  public addDisconnectRule(disconnectRule: DisconnectRule): Observable<any> {
    return this.dataService.post(constLocal.disconnectrules, disconnectRule)
    .pipe(catchError(error => {
      return throwError(this.convertToAddDisconnectRuleError(error));
    }));
  }

  private convertToAddDisconnectRuleError(error: any): AddDisconnectRuleError {
    if ( !(error instanceof HttpErrorResponse) ) {
      return AddDisconnectRuleError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch(httpErrorResponse.status) {
      case 409:
        return AddDisconnectRuleError.RequestContentDuplicate;
      case 415:
        return AddDisconnectRuleError.RequestContentIncomplete;
      default:
        return AddDisconnectRuleError.UnspecifiedError;
    }
  }

  public deleteDisconnectRule(label: string, obisCode: string): Observable<any> {
    return this.dataService.delete(constLocal.disconnectrulesNames + "/" + label + "/" + obisCode)
    .pipe(catchError(error => {
      return throwError(this.convertToDeleteDisconnectRuleError(error));
    }));
  }

  private convertToDeleteDisconnectRuleError(error: any): DeleteDisconnectRuleError {
    if ( !(error instanceof HttpErrorResponse) ) {
      return DeleteDisconnectRuleError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch(httpErrorResponse.status) {
      case 415:
        return DeleteDisconnectRuleError.DisconnectRulePathMissing;
      default:
        return DeleteDisconnectRuleError.UnspecifiedError;
    }
  }

  public getDisconnectRuleOptions(): Observable<DisconnectRuleOptionSet> {
    return this.dataService.get<DisconnectRuleOptionSet>(constLocal.disconnectrulesOptions, undefined, new DisconnectRuleOptionSet);
  }
//
  public getEmailRecipients(): Observable<EmailRecipientSet> {
    return this.dataService.get<EmailRecipientSet>(constLocal.emailRecipients, undefined, new EmailRecipientSet);
  }

  public addEmailRecipient(emailRecipeint: EmailRecipient): Observable<any> {
    return this.dataService.post(constLocal.emailRecipients, emailRecipeint)
    .pipe(catchError(error => {
      return throwError(this.convertToAddEmailRecipientError(error));
    }));
  }

  private convertToAddEmailRecipientError(error: any): AddEmailRecipientError {
    if ( !(error instanceof HttpErrorResponse) ) {
      return AddEmailRecipientError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch(httpErrorResponse.status) {
      case 409:
        return AddEmailRecipientError.RequestContentDuplicate;
      case 415:
        return AddEmailRecipientError.RequestContentIncomplete;
      default:
        return AddEmailRecipientError.UnspecifiedError;
    }
  }

  public deleteEmailRecipient(emailAddress: string): Observable<any> {
    return this.dataService.delete(constLocal.emailRecipients + "/" + emailAddress)
    .pipe(catchError(error => {
      return throwError(this.convertToDeleteEmailRecipientError(error));
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
    return this.dataService.put(constLocal.emailRecipients + "/" + emailAddress + "/test")
    .pipe(catchError(error => {
      return throwError(this.convertToTestEmailRecipientError(error));
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

  public getProfileGraphs(): Observable<ProfileGraphSet> {
    return this.dataService.get<ProfileGraphSet>(constLocal.profileGraphs, undefined, new ProfileGraphSet);
  }

  public addProfileGraph(profileGraph: ProfileGraph): Observable<any> {
    return this.dataService.post(constLocal.profileGraphs, profileGraph)
    .pipe(catchError(error => {
      return throwError(this.convertToAddProfileGraphError(error));
    }));
  }

  private convertToAddProfileGraphError(error: any): AddProfileGraphError {
    if ( !(error instanceof HttpErrorResponse) ) {
      return AddProfileGraphError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch(httpErrorResponse.status) {
      case 409:
        return AddProfileGraphError.RequestContentDuplicate;
      case 415:
        return AddProfileGraphError.RequestContentIncomplete;
      default:
        return AddProfileGraphError.UnspecifiedError;
    }
  }

  public updateProfileGraph(periodGraphIdBase64: string, profileGraph: ProfileGraph): Observable<any> {
    return this.dataService.put(constLocal.profileGraphsModify + '/' + periodGraphIdBase64, profileGraph)
    .pipe(catchError(error => {
      return throwError(this.convertToUpdateProfileGraphError(error));
    }));
  }

  private convertToUpdateProfileGraphError(error: any): UpdateProfileGraphError {
    if ( !(error instanceof HttpErrorResponse) ) {
      return UpdateProfileGraphError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch(httpErrorResponse.status) {
      case 409:
        return UpdateProfileGraphError.ExistingProfileGraphAbsent;
      case 415:
        return UpdateProfileGraphError.RequestContentIncomplete;
      default:
        return UpdateProfileGraphError.UnspecifiedError;
    }
  }

  public swapProfileGraphRank(period: string, page: string, title1: string, title2: string): Observable<any> {
    var params = new HttpParams().set("period", period).set("page", page).set("title1", title1).set("title2", title2);
    return this.dataService.put(constLocal.profileGraphsSwaprank, null, params)
    .pipe(catchError(error => {
      return throwError(this.convertToSwapProfileGraphRankError(error));
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
      return throwError(this.convertToDeleteProfileGraphError(error));
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
      return throwError(this.convertToSaveMqttParamsError(error));
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
      return throwError(this.convertToTestMqttParamsError(error));
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
      return throwError(this.convertToSaveSmtpParamsError(error));
    }));
  }

  private convertToSaveSmtpParamsError(error: any): SaveSmtpParamsError {
    if ( !(error instanceof HttpErrorResponse) ) {
      return SaveSmtpParamsError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch(httpErrorResponse.status) {
      case 415:
        return SaveSmtpParamsError.RequestContentIncomplete;
      default:
        return SaveSmtpParamsError.UnspecifiedError;
    }
  }

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

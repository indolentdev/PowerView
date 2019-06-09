import { Component } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { NGXLogger } from 'ngx-logger';

import { Moment } from 'moment'
import * as moment from 'moment';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  constructor(private log: NGXLogger, public translate: TranslateService) {
    translate.addLangs(['en', 'da']);
    translate.setDefaultLang('en');

    const browserLang = translate.getBrowserLang();
    this.log.info("Browser language:", browserLang);
    var lang = browserLang.match(/en|da/) ? browserLang : 'en';
    this.log.info("Applying language:", lang);
    translate.use(lang);
    moment.locale(lang);
  }

}

import { Component, OnInit, ViewChild, Inject, Input, LOCALE_ID } from '@angular/core';
import { formatNumber } from '@angular/common';
import { NGXLogger } from 'ngx-logger';
import { TranslateService } from '@ngx-translate/core';
import { ObisTranslateService } from '../../../services/obis-translate.service';
import * as Highcharts from 'highcharts';
import { Profile } from '../../../model/profile';
import { ProfileSerie } from '../../../model/profileSerie';

import { Moment } from 'moment'
import * as moment from 'moment';

@Component({
  selector: 'app-profile-graph',
  templateUrl: './profile-graph.component.html',
  styleUrls: ['./profile-graph.component.css']
})
export class ProfileGraphComponent implements OnInit {
  Highcharts = Highcharts;
  chartOptions: any;

  private decimalSep: string;
  private thousandSep: string;

  @Input('profileGraph') profileGraph: Profile;
  @Input('timeFormat') timeFormat: string;

  constructor(private log: NGXLogger, @Inject(LOCALE_ID) private locale: string, private obisService: ObisTranslateService, private translateService: TranslateService) {
  }

  ngOnInit() {
    this.setSeparators();

    // set chart options only after the translations have loaded - use a dummy key to wait for translation load
    // someday figure if the highchart graph can support lazy loading of the translated strings.
    this.translateService.get("profileGraphs.common.resetZoom").subscribe(resetZoom => {
      this.setChartOptions();
      Highcharts.setOptions({
        lang: {
          resetZoom: resetZoom,
          resetZoomTitle: "" // Tooltips not used elsewhere in the application. Disable it here for consistency.
        }
      });
    });
  }

  private setSeparators() {
    this.decimalSep = "";
    this.thousandSep = "";

    var s = formatNumber(1211.3, this.locale, '1.1-1');
    var thousandIx = s.indexOf("2") - 1;
    if (thousandIx > -1)
    {
      var thousandSepCandidate = s.substr(thousandIx, 1);
      if (thousandSepCandidate === '.' || thousandSepCandidate === ',') {
        this.thousandSep = thousandSepCandidate;
      }
    }
    var decimalPointIx = s.indexOf("3") -1;
    if (decimalPointIx > -1)
    {
      this.decimalSep = s.substr(decimalPointIx, 1);
    }
//    this.log.info("Separators (thousand & decimal) :", this.thousandSep, this.decimalSep);
  }

  private setChartOptions() {
    var self = this;

    var graphDistinctYAxisIds = Array.from(new Set(this.profileGraph.series.map(x => x.serieYAxis)));
    var graphYAxes = this.getYAxes().filter(yAxis => graphDistinctYAxisIds.includes(yAxis.id));

    this.chartOptions = {
      title: {
        text: this.profileGraph.title
      },
      chart: {
        zoomType: 'x',
        resetZoomButton: {
          position: {
              align: 'left'//, // by default
              // verticalAlign: 'top', // by default
              // x: 0,
              // y: -30
          }
        }        
      },
      subtitle: {
        text: document.ontouchstart === undefined ? 
          this.translateService.instant("profileGraphs.common.DragToZoom") : this.translateService.instant("profileGraphs.common.UnpinchToZoom")
      },
      xAxis: {
        categories: this.localizeTimestamps(this.profileGraph.categories, this.timeFormat),
      },
      yAxis: graphYAxes,

      tooltip: {
        useHTML:true,
        formatter: function () { // the "this" reference is changed by the function..
          var tooltipHtml = this.x + '<table>';
          for (let point of this.points) {
            var valSplit = self.splitFormattedValue(point.y);

            tooltipHtml += 
            '<tr>' +
            '<td style="background-color:' + point.series.color + ';color:' + point.series.color + '">__</td>' +
            '<td>' + point.series.name + '</td>' +
            '<td style="text-align:right; padding-right:0px">' + valSplit[0] + '</td>' + 
            '<td style="text-align:left; padding-left:0px">' + valSplit[1] + '</td>' +
            '<td>' + point.series.options.unit + '</td>' +
            '</tr>';
          }
          tooltipHtml += '</table>';
          return tooltipHtml;
        },
        shared:true,
        borderWidth:0,
        shadow:false,
        crosshairs:true
      },
        
      legend: {
        enabled: true
      },

      plotOptions: {
        spline: {
          lineWidth: 2,
          shadow: false,
          states: {
            hover: { lineWidth: 2 }
          },
          marker: {
            enabled: false,
            states: {
              hover: {
                enabled: true,
                symbol: "circle",
                radius: 4,
                lineWidth: 1
              }
            }
          }
        },
        areaspline: {
          fillOpacity: .20,
          lineWidth: 2,
          shadow: false,
          states: {
            hover: { lineWidth: 2 }
          },
          marker: {
            enabled: false,
            states: {
              hover: {
                enabled: true,
                symbol: "circle",
                radius: 4,
                lineWidth: 1
              }
            }
          }
        }
      }, // end plotOptions
      series: this.GetSeries(this.profileGraph.series)  
    };
  }

  private localizeTimestamps(utcTimestamps: string[], format: string): string[] {
    var localTimestamps = [];
    for (var i = 0; i < utcTimestamps.length; i++) {
      var m = moment(utcTimestamps[i], moment.ISO_8601);
      var local = m.format(format);
      localTimestamps.push(local);
    }
    return localTimestamps;
  }

  private splitFormattedValue(value: number): string[]  {
    var g = this.formatValue(value).split(this.decimalSep);

    var integer = "";
    var decimal = "";
    if (g.length >= 1)
    {
      integer = g[0];
    }
    if (g.length == 2)
    {
      decimal = this.decimalSep + g[1];
    }
    return [integer, decimal];
  }

  private GetSeries(profileSeries: ProfileSerie[]): any[] {
    var series = this.obisService.AddSerieProperty(profileSeries)

    var chartSeries = [];
    for (let serie of series) {
      var serieName = serie.serie;
      var serieId = serieName.split(' ').join('-').toLowerCase();
      var chartSerie = {
          id: serieId,
          type: serie.serieType,
          name: serieName,
          color: serie.serieColor,
          yAxis: serie.serieYAxis,
          unit: serie.unit,
          data: serie.values
      };
      chartSeries.push(chartSerie);
    }
    return chartSeries;
  }

  private getYAxes(): any[] {
    var self = this;

    return [ 
      this.getYAxis("energyPeriod", "Energy", 0, true),
      this.getYAxis("energyDelta", "EnergyElement", 0),
      this.getYAxis("power", "Power", 0),
      this.getYAxis("volumePeriod", "Volume", 0, true),
      this.getYAxis("volumePeriodHiddenYAxis", undefined, 0),
      this.getYAxis("volumeDelta", "VolumeElement", 0),
      this.getYAxis("volumeDeltaHiddenYAxis", undefined, 0),
      this.getYAxis("flow", "Flow", 0),
      this.getYAxis("flowHiddenYAxis", undefined, 0),
      this.getYAxis("temp", "Temperature", 0),
      this.getYAxis("tempHiddenYAxis", undefined, 0),
      this.getYAxis("rh", "RelativeHumidity", 0, true),
      this.getYAxis("dcOutputStatusHiddenYAxis", undefined, 0),
      this.getYAxis("currencyAmount", "CurrencyAmount", undefined, true),
    ];
  }

  private getYAxis(yAxisId: string, titleResource?: string, min?: number, opposite?: boolean): any {
    var self = this;

    var yAxis: any;
    yAxis = {
      id: yAxisId,
      title: titleResource == undefined ? {text: null} : {text: this.translateYAxis(titleResource), style: {color: "black"}},
      showEmpty: false,
      labels: titleResource == undefined ? {enabled:false} : {
        formatter: function() { 
          var unit = this.axis.series[0].options.unit;
          return self.formatValue(this.value) + " " + unit; 
        }, 
        style: {color: "black"}
      },
      min: min
    };
    if (opposite != undefined)
    {
      yAxis.opposite = opposite
    }
    return yAxis;
  }

  private translateYAxis(serieKind: string): string {
    return this.translateService.instant("profileGraphs.yAxis." + serieKind);
  }

  private formatValue(value: number): string {
    var s = formatNumber(value, this.locale);
    return s;
  }
  
}

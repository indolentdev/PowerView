import { ExportSeriesGauge } from "./exportSeriesGauge";

export class ExportSeriesGaugeSet {
    timestamps: string[];
    series: ExportSeriesGauge[];

    constructor() {
        this.timestamps = [];
        this.series = [];
    }
}

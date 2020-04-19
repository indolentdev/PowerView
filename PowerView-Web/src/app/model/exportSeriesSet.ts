import { ExportSeries } from "./exportSeries";

export class ExportSeriesSet {
    timestamps: string[];
    series: ExportSeries[];

    constructor() {
        this.timestamps = [];
        this.series = [];
    }
}

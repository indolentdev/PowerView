import { ExportPeriod } from "./exportPeriod";
import { ExportSeriesDiff } from "./exportSeriesDiff";

export class ExportSeriesDiffSet {
    periods: ExportPeriod[];
    series: ExportSeriesDiff[];

    constructor() {
        this.periods = [];
        this.series = [];
    }
}

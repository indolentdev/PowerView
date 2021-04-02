import { ExportDiffPeriod } from "./exportDiffPeriod";
import { ExportSeriesDiff } from "./exportSeriesDiff";

export class ExportSeriesDiffSet {
    periods: ExportDiffPeriod[];
    series: ExportSeriesDiff[];

    constructor() {
        this.periods = [];
        this.series = [];
    }
}

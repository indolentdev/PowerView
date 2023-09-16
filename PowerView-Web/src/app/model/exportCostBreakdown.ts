import { ExportPeriod } from "./exportPeriod";
import { ExportCostBreakdownName } from "./exportCostBreakdownName";

export class ExportCostBreakdown {
    title: string;
    currency: string;
    vat: number;
    periods: ExportPeriod[];
    entries: ExportCostBreakdownName[];

    constructor() {
        this.periods = [];
        this.entries = [];
    }
}

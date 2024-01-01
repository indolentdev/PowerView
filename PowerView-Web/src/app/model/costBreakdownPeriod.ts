import { CostBreakdownEntry } from './costBreakdownEntry';
import { CostBreakdownEntryPeriod } from './costBreakdownEntryPeriod';

export class CostBreakdownPeriod {
    period: CostBreakdownEntryPeriod;
    entries: CostBreakdownEntry[];

    constructor() {
        this.entries = [];
    }
}

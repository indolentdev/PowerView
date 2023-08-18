import { CostBreakdownEntry } from './costBreakdownEntry';

export class CostBreakdown {
    title: string;
    vat: number;
    entries: CostBreakdownEntry[];

    constructor() {
        this.entries = [];
    }
}

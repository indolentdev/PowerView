import { CostBreakdownPeriod } from './costBreakdownPeriod';

export class CostBreakdownWithPeriod {
    title: string;
    currency: string;
    vat: number;
    entryPeriods: CostBreakdownPeriod[];

    constructor() {
        this.entryPeriods = [];
    }
}

export class CostBreakdownEntry {
    from: string;
    to: string;
    name: string;
    start: number;
    end: number;
    amount: number;
    currency: string;

    constructor() {
        this.start = 0;
        this.end = 24;
    }
}
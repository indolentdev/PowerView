export class CostBreakdownEntry {
    fromDate: string;
    toDate: string;
    name: string;
    startTime: number;
    endTime: number;
    amount: number;
    currency: string;

    constructor() {
        this.startTime = 0;
        this.endTime = 24;
    }
}
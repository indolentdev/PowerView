import { Moment } from 'moment'

export class ExportSpec {
    labels: string[];
    from: Moment;
    to: Moment;
    decimalSeparator: string;

    constructor() {
        this.labels = [];
    }
}

import { Moment } from 'moment'

export class ExportSpec {
    labels: string[];
    from: Moment;
    to: Moment;

    constructor() {
        this.labels = [];
    }
}

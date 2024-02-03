import { HistoryLabelTimestamp } from './historyLabelTimestamp';

export class HistoryStatus {
    interval: string;
    labelTimestamps: HistoryLabelTimestamp[];

    constructor() {
        this.labelTimestamps = [];
    }
}

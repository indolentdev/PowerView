import { EventAmplification } from './eventAmplification';

export class EventAmplificationLeak extends EventAmplification {
    startTimestamp: string;
    endTimestamp: string;
    value: number;
    unit: string;
}

import { EventAmplification } from "./eventAmplification";

export class Event {
    type: string;
    label: string;
    detectTimestamp: string;
    status: boolean;
    amplification: EventAmplification;
}

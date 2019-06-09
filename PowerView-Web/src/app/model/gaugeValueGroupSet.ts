import { GaugeValueGroup } from "./gaugeValueGroup";

export class GaugeValueGroupSet {
    timestamp: string;
    groups: GaugeValueGroup[];

    constructor() {
        this.groups = [];
    }
}

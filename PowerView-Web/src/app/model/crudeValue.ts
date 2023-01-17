
export class CrudeValue {

    timestamp: string;
    obisCode: string;
    value: number;
    scale: number;
    unit: string;
    deviceId: string;
    tags: string[];

    constructor() {
        this.tags = [];
    }
}

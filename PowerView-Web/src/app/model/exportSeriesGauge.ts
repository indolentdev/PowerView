import { ExportGaugeValue } from "./exportGaugeValue";

export class ExportSeriesGauge {
    label: string;
    obisCode: string;
    values: ExportGaugeValue[];

    constructor() {
        this.values = [];
    }
}

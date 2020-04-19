import { ExportValue } from "./exportValue";

export class ExportSeries {
    label: string;
    obisCode: string;
    values: ExportValue[];

    constructor() {
        this.values = [];
    }
}

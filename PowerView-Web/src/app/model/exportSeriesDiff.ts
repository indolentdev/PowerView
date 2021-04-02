import { ExportDiffValue } from "./exportDiffValue";

export class ExportSeriesDiff {
    label: string;
    obisCode: string;
    values: ExportDiffValue[];

    constructor() {
        this.values = [];
    }
}

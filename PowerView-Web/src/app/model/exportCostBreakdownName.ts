import { ExportCostBreakdownNameValue } from "./exportCostBreakdownNameValue";

export class ExportCostBreakdownName {
    name: string;
    values: ExportCostBreakdownNameValue[];

    constructor() {
        this.values = [];
    }
}

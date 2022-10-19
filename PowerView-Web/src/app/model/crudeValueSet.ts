import { CrudeValue } from './crudeValue';

export class CrudeValueSet {
    label: string;
    totalCount: number;
    values: CrudeValue[];

    constructor() {
        this.values = [];
    }
}

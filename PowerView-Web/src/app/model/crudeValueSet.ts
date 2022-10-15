import { CrudeValue } from './crudeValue';

export class CrudeValueSet {
    label: string;
    values: CrudeValue[];

    constructor() {
        this.values = [];
    }
}

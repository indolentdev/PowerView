import { CrudeValue } from './crudeValue';

export class CrudeValueset {
    label: string;
    values: CrudeValue[];

    constructor() {
        this.values = [];
    }
}

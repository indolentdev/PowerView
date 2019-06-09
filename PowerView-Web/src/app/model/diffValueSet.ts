import { DiffValue } from './diffValue';

export class DiffValueSet {
    from: string;
    to: string;
    registers: DiffValue[];

    constructor() {
        this.registers = [];
    }
}

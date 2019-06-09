import { GaugeValue } from './gaugeValue';

export class GaugeValueGroup {
    name: string;
    registers: GaugeValue[];

    constructor() {
        this.registers = [];
    }

}

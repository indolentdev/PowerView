import { SerieName } from './serieName';

export class ProfileGraph {
    period: string;
    page: string;
    title: string;
    interval: string;
    series: SerieName[];

    constructor() {
        this.series = [];
    }
}

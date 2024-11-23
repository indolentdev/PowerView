export class ProfileSerie {
    label: string;
    obisCode: string;
    unit: string;
    serieType: string;
    serieYAxis: string;
    serieColor: string;
    values: number[];
    deviationValues: number[][];

    constructor() {
        this.values = [];
        this.deviationValues = [];
    }    
}

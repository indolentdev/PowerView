import { ProfileSerie } from "./profileSerie";

export class Profile {
    title: string;
    categories: string[];
    series: ProfileSerie[];

    constructor() {
        this.categories = [];
        this.series = [];
    }    
}
